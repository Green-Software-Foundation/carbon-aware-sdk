#!/bin/bash
if [ -z "$1" ]
then
    echo "You must provide the first parameter as the host name of the service to generate the clients for (ex. localhost:5073)."
    exit 1
fi

rm -rf ./generated 
mkdir ./generated
cd generated 

openapi-generator-cli generate -i http://$1/swagger/v1/swagger.json -g java -o ./java
openapi-generator-cli generate -i http://$1/swagger/v1/swagger.json -g python -o ./python
openapi-generator-cli generate -i http://$1/swagger/v1/swagger.json -g javascript -o ./javascript
openapi-generator-cli generate -i http://$1/swagger/v1/swagger.json -g csharp-netcore -o ./csharp --additional-properties=targetFramework=net8.0
openapi-generator-cli generate -i http://$1/swagger/v1/swagger.json -g go -o ./golang
