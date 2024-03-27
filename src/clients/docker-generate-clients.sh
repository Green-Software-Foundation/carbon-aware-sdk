if [ -z "$1" ]
then
    echo "You must provide the first parameter as the host name of the service to generate the clients for (ex. host.docker.internal:5073)."
    exit 1
fi

# remove previous folders
rm -r ./java
rm -r ./python
rm -r ./javascript
rm -r ./csharp
rm -r ./golang

# quit on error
set -e

# java
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g java \
  -o /local/java

# python
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g python \
  -o /local/python

# javascript
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g javascript \
  -o /local/javascript

# csharp
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g csharp-netcore \
  -o /local/csharp \
  --additional-properties=targetFramework=net8.0

# golang
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g go \
  -o /local/golang
