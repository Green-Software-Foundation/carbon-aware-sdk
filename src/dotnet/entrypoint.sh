#!/bin/sh
set -eax

/CarbonAwareCLI -l $1  -c $2 $3 

if /CarbonAwareCLI -l $1  -c $2 $3 | grep -q '/Location'; then

    exit 0
else 

    exit 1
fi
