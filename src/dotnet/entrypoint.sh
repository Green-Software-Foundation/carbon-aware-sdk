#!/bin/sh
set -eax

OutputEmissionsData=$(/CarbonAwareCLI -l $1  -c $2 $3 ) 

echo "::set-output name=OutputEmissionsData::$(echo $OutputEmissionsData | jq '.Location')"

