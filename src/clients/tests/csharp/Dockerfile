# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# copy csharp client
WORKDIR /source/clients/csharp/src/Org.OpenAPITools
COPY csharp/src/Org.OpenAPITools .

# copy proxy project
WORKDIR /source/clients/tests/csharp
COPY tests/csharp .

# publish proxy project
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY tests/temp.env /.env
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "csharp.dll"]