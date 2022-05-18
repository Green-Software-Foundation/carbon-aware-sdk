if [ -z "$1" ]
then
    echo "You must provide the first parameter as the host name of the service to generate the clients for (ex. localhost:5073)."
    exit 1
fi

# java
rm -r ./java
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g java \
  -o /local/java

# python
rm -r ./python
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g python \
  -o /local/python

# javascript
rm -r ./javascript
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g javascript \
  -o /local/javascript

# csharp
rm -r ./csharp
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g csharp-netcore \
  -o /local/csharp \
  --additional-properties=targetFramework=net6.0

# golang
rm -r ./golang
docker run --rm \
  -v ${PWD}:/local \
  openapitools/openapi-generator-cli generate \
  -i http://$1/swagger/v1/swagger.json \
  -g go \
  -o /local/golang
