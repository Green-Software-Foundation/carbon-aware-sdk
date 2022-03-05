#!/bin/sh
set -eax

/CarbonAwareCLI -l $1  -c $2 $3 

echo "::set-output name=OutputEmissionsData::$(echo $OutputEmissionsData)"

