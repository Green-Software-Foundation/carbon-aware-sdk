Date.prototype.addHours = function(h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
}

const MAX=700/100; //MAX value for the Intensity

var Global={
    map: null,
    Dmin: null,
    Dmax: null,
    data: [],
    location: [],
    intervals: [0, 5*MAX, 25*MAX, 40*MAX, 60*MAX, 100*MAX],
    colors:["green", "lightgreen", "yellow","red", "brown"]
}

window.onload=init;




var UI={
    handlerPlay:null,
    _play:function(){
        var D=document.getElementById("event-end").innerHTML;
        var d=new Date(D);
        d.addHours(1);
        if (d>Global.Dmax)
            d=Global.Dmin;
        document.getElementById("event-end").innerHTML=d.toISOString();
        var dateSlider = document.getElementById('slider-date');
        dateSlider.noUiSlider.set([null,d]);
        Draw.updateLocationData(d);
    },
    play:function() {
        UI.handlerPlay=setInterval(UI._play,1000);
    },
    stop:function() {
        clearInterval(UI.handlerPlay);
    },
    resizeWindow:function(){
        document.getElementById("myMap").style.height=(window.innerHeight-210)+"px"; 
    },
    init:function(){
        document.getElementById("btnPlay").addEventListener('click', UI.play);
        document.getElementById("btnStop").addEventListener('click', UI.stop);
        window.onresize=UI.resizeWindow;
        UI.resizeWindow();
    }
}

var Draw={
    drawLegend:function(){
        var sH="<div>Legend</div>";
        for(var f=0;f<Global.intervals.length-1;f++){
            var v0=Global.intervals[f];
            var v1=Global.intervals[f+1];
            sH+=`<div style="width:60px;background-color:${Global.colors[f]};text-align:center;">${v0}-${v1}</div>`;
        }
        return sH;
    },
    updateLocationData:function(date){
        Global.locations.forEach(l=>{
            if (l.pushpin){
                var regionName=l.name;
                var time=date;
                var D=Global.data.filter(el=>el.Location==regionName && el.Time>=date.toISOString()).sort((el,el1)=>{return (el.Time-el1.Time)});
                var value=-1;
                if (D.length>0)
                {
                    value=D[0].Rating;
                }
                //green 0, 
                var vv=Global.intervals.findIndex(el=>(el>=value));
                var c=Global.colors[vv];

                //Location, Time, Rating
                l.pushpin.setOptions({ color: c });

                Microsoft.Maps.Events.addHandler(l.pushpin, 'click', function () { ShowChart(l.name); });

            }
       });
    }

}


function slider(min,max){

    function timestamp(str) {
        return new Date(str).getTime();
    }
    
    var formatNumber={
        to:function(value){
            var D=new Date(value).toISOString();
            return D.substring(0,10)+ "<br>" + D.substring(11,19);
        },
        toFull:function(value){
            var D=new Date(value).toISOString();
            return D.substring(0,10) + " " + D.substring(11,19);
        },
        from:function(value){
            return value;
        }
    }
    
    var dateSlider = document.getElementById('slider-date');

    noUiSlider.create(dateSlider, {
        range: {
            min: timestamp(min),
            max: timestamp(max)
        },
        tooltips: {
            to:formatNumber.toFull,
            from:formatNumber.from // tooltip with custom formatting
        },
    
        step:  60 * 60 * 1000,
    
        start: [timestamp(min), timestamp(max-10000000)],
    
        pips: {
            mode: 'positions',
            values:[0,25,50,75,100],
            format: formatNumber
        }
    });
    
    var dateValues = [
        document.getElementById('event-start'),
        document.getElementById('event-end')
    ];
    
    var formatter = new Intl.DateTimeFormat('en-GB', {
        dateStyle: 'full'
    });
    
    dateSlider.noUiSlider.on('update', function (values, handle) {
        var D=new Date(+values[handle]);
        dateValues[handle].innerHTML =D.toISOString();
        Draw.updateLocationData(D);
    });
    
    // To disable one handle
    var origins = dateSlider.querySelectorAll('.noUi-origin');
    origins[0].setAttribute('disabled', true);
    origins[0].setAttribute('style', "display:none");
}

async function init(){
    UI.init();
    document.getElementById("divLegend").innerHTML=Draw.drawLegend();

    var resp=await fetch('data/azure_locations.json');
    let azure_geo = await resp.json();

    var source=document.getElementById("selDataSource").value;
    var resp=await fetch(source);
    Global.data = await resp.json();

    var listLocation=[];
    if (Global.data.length==0){
        alert("No data found");
        return;
    }
    var minTime=Global.data[0].Time;
    var maxTime=Global.data[0].Time;
    Global.data.forEach(element => {
        listLocation.push(element.Location);
        if (minTime>element.Time)
            minTime=element.Time;
        if (maxTime<element.Time)
            maxTime=element.Time;
    });
    const unique=Array.from(new Set(listLocation));

    Global.locations=[];
    unique.forEach(element=>{
        var azCenter=azure_geo.find(el=>el.RegionName==element);
        if (azCenter!=undefined)
            Global.locations.push({name:element, Latitude:azCenter.Latitude, Longitude:azCenter.Longitude})
        else
            console.log("UNKNOWN:" + element)
    })

    //MIN AND MAX DATE
    Global.Dmin=new Date(minTime);
    Global.Dmax=new Date(maxTime);

    //DRAW SLIDER
    slider(Global.Dmin,Global.Dmax);

    //DRAW MAP
    var navigationBarMode = Microsoft.Maps.NavigationBarMode;
    Global.map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
        center: new Microsoft.Maps.Location(51.50632, -0.12714),
        navigationBarMode: navigationBarMode.compact,
        zoom: 1
    });

    Global.locations.forEach(el=>{
        var pos=new Microsoft.Maps.Location(el.Latitude, el.Longitude);
        var pushpin = new Microsoft.Maps.Pushpin(pos, { iconStyle:33,color: 'yellow' });
        Global.map.entities.push(pushpin);
        el.pushpin=pushpin;
    });

    var D=document.getElementById("event-end").innerHTML;
    Draw.updateLocationData(new Date(D));

    
}


function ShowChart(location){
    // Select modal
    var mpopup = document.getElementById('mpopupBox');
    
    // Select close action element
    var close = document.getElementsByClassName("close")[0];
    
    // Close modal once close element is clicked
    close.onclick = function() {
        mpopup.style.display = "none";
    };
    
    // Close modal when user clicks outside of the modal box
    window.onclick = function(event) {
        if (event.target == mpopup) {
            mpopup.style.display = "none";
        }
    };

    mpopup.style.display = "block";

    var d=Global.data.filter(el=>el.Location==location);
    var data=[];
    d.forEach(l=>{
        data.push({x:l.Time, y:l.Rating});
    });
    console.log(data)

    var options = {
        chart: {
            type: 'line'
        },
        series: [{
            name: 'Intensity',
            data: data
        }],
        xaxis: {
            type:'datetime'
        },
        animations: {
            enabled: false,
            animateGradually:{
                enabled:false
            },
            dynamicAnimation:{
                enabled:false
            }
        }
    }

    var chart = new ApexCharts(document.getElementById("chart"), options);
    chart.render();
}
