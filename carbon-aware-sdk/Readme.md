
# Placeholder - Work in Progress

## Intent

Today, as part of the tenets laid down by Green software foundation there are 3 ways in which we can design and build green software
	- Building applications that are carbon efficient i.e. using less energy from the datacenters to run the software .
	- Building applications that are hardware efficient i.e. using lesser number of  hardware components or lower hardware specifications for running the same workload
	- Building applications that are carbon aware i.e. Running the workload at the right time of the day at the right data center location so that carbon consumption is minimized.

## What is the Carbon Aware SDK project ?
To help build applications that are carbon aware, the Carbon Aware SDK project is an initiative to bring home the concept of building sustainable applications to the development community. There are multiple flavors of the SDK being planned.

* For native integration with standard programming languages ,a library will be developed that developers can integrate with directly .
* For integration with applications using standard protocols like REST , a Web API will be exposed 
* A command line interface will also be made available to account for scenarios like build and deployment pipelines that do not have a graphical user interface. 

## Why is there a need for a SDK ?

It is important that we enable developers to solve business problems and abstract away the need to spend time in identifying the most green cloud data centers in which they need to run their application. Developers will be highly productive when they don’t have to manage nuances around infrastructure and ours is an attempt or a step towards that direction.

As developers build applications in their favorite programming languages, they need to add reference to the Carbon Aware SDK to take advantage and get to know 
	1) The best time of the day to run their application at a specific datacenter
	2) The most carbon efficient data center among a group of datacenters
	3) The most carbon efficient data center and the specific time at which to run the application in that data center.


## How does it work ?

Today there are standard 3rd party data providers like Watt Time that track the greenness of the electricity that powers the data centers.  Watt Time provides subscription data that helps track the % of energy powering the datacenters that is driven by renewables. The SDK  internally makes calls to these data providers (Watt time, Electricity map for today and we may add more providers in the future) and helps provide the data center with lowest carbon emissions to calling applications. 

Additionally by integrating with the scheduling APIs available with cloud providers, the approach could also be extended to allocating the workload in the appropriate datacenter based on output from Carbon Aware SDK. For example, if we have to deploy a bunch of containers to the cloud the allocation algorithm can make appropriate choices for the best time and data center to run these container workloads based on the output.

## How do we validate ?
One of the recommended ways to calculate the carbon emission quotient of an application is by measurement of Software Carbon Intensity specification (SCI). The SCI formula can be applied here for measurement and comparison of datacenter options .

The carbon aware core SDK conforms to the SCI specification, using it in an application should help it reduce it’s SCI score, either through feedback and manual enhancements, or in real time automated decision making.


## How can applications leverage it ?

Listed below are some use case scenarios where Carbon Aware SDK can be leveraged.

### Developer Enablement

Martin is a Python developer and he is building a sentiment analytics application that goes through tonnes of customer feedback data and does scoring using supervised learning techniques. At the time of coding his application, he would like to evaluate the different location options in Americas for hosting the application. Also since this is not a real time customer facing application, he would like to also schedule the application using either Azure functions or Azure Web jobs.

Martin uses the Native Python SDK available and integrates it into the Sentiment Analytics applications. He designs logging component that will help him log the emissions profile of the energy provided for the workload, at different locations in Americas - EastUS, WestUS, Central US etc . By looking at the logs, Martin will be able to identify the right time of the day and the right datacenter where to run the application. 

### Devops Enablement

To be Added

