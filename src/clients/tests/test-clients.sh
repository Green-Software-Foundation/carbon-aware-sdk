#!/bin/zsh

if [ -z "$1" ]
then
    echo "You must provide the first parameter as the host name of the service being tested (ex. localhost). The ports will be read from the .env file."
    exit 1
fi

# exit on first error
set -e

# get env
set -o allexport
source .env
set +o allexport

# csharp
newman run tests.postman_collection.json --global-var=base_url=$1:$CSHARP_PORT
