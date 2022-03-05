#!/bin/sh
set -eax

OutputEmissionsData=$(/CarbonAwareCLI -l $1  -c $2 $3 | jq '.Location') 

echo "::set-output name=OutputEmissionsData::$(echo $OutputEmissionsData)"

