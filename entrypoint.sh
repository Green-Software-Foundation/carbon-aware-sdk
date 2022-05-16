#!/bin/sh
set -eax

#Exec the CarboneAwareCLI, to return the Region with Lowest Emissions
# $1 : input.location
# $2: input.config
# $3 : --lowest
OutputEmissionsData=$(/CarbonAwareCLI -l $1  -c $2 $3 ) 

#Export the Recommended Region, as a Github Action output, to be used by subsequent workflow steps
# the CLI might return several Regions, for the current version of the Github Action, we return one of the lowest
responseLocation=$(echo $OutputEmissionsData | jq '.[0].Location')
myLocation=$(echo ::set-output name=LowestEmissionsLocation::$(echo $OutputEmissionsData | jq '.[0].Location' ) )

myLocation="${myLocation#'"'}"
myLocation="${myLocation%'"'}"

echo $myLocation

echo ::set-output name=LowestEmissionsTime::$(echo $OutputEmissionsData | jq '.[0].Time')

TimeOutput=$(echo $OutputEmissionsData | jq '.[0].Time')

#Parse Time String to Convert to CronExpression
hour_minute_sec="${TimeOutput#*T}"
hour=$(echo "$hour_minute_sec" | cut -d : -f 1)
minute=$(echo "$hour_minute_sec" | cut -d : -f 2)

day_month_year="${TimeOutput%T*}"
month=$(echo "$day_month_year" | cut -d - -f 2)
day=$(echo "$day_month_year" | cut -d - -f 3)


TimeCron="$minute $hour $day $month *"

echo "::set-output name=LowestEmissionsTimeCron::$TimeCron"
