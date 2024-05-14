FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update

RUN apt-get install -y iputils-ping procps

RUN echo Installing Code Generator CLI

RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
RUN apt-get install -y nodejs build-essential default-jre 
RUN npm install @openapitools/openapi-generator-cli -g

RUN echo Done!

