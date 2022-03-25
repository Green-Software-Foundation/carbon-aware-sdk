#!/bin/sh
set -eax

#Exec the CarboneAwareCLI, to return the Region with Lowest Emissions
OutputEmissionsData=$(/CarbonAwareCLI -l $1  -c $2 $3 ) 

#Export the Recommended Region, as a Github Action output, to be used by subsequent workflow steps
echo "::set-output name=LowestEmissionsLocation::$(echo $OutputEmissionsData | jq '.[0]')"
