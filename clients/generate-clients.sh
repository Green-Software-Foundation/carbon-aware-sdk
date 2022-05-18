rm -r ./csharp
openapi-generator generate -i http://localhost:5073/swagger/v1/swagger.json -g csharp-netcore -o ./csharp
