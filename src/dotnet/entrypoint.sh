#!/bin/sh
set -eax

/CarbonAwareCLI -l $1  -c $2 $3 

/CarbonAwareCLI -l $1  -c $2 $3 | jq '.Location' 

echo "::set-output name=OutputEmissionsData::$(echo $OutputEmissionsData)"

