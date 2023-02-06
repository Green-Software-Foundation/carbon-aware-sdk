# Client SDKs

While the web endpoints for the Grid API can be used consumed directly via REST,
the GSF is also providing a number of SDKs.

There are several example SDKs that are generated using the scripts currently.
Those include:

- Java
- Python
- JavaScript
- C#
- Golang

_Note:_ The generated clients may have imcompatibility at times due to external
factors, such as a new language version being used. If you note any
incompatibilities, please raise an issue on github and we'll look to rectify as
soon as possible via a fix or additioanl guidance where required.

The SDKs are generated based on the OpenAPI specification provided by the Grid
API service using the [Open API Generator](https://openapi-generator.tech/).
This same generator can be used to generate other clients. For example, to build
a Rust client, you could execute...

```bash
openapi-generator generate -i http://localhost:5073/swagger/v1/swagger.json -g rust -o ./rust
```

Some additional clients that could be generated, and include:

- Android
- C/C++
- Lua
- Perl
- PHP
- R
- Rust
- Swift
- TypeScript
- and many more can be found here:
  <https://openapi-generator.tech/docs/generators>

## Generation and Testing via Docker

If desired, you can use Docker to run all client generation and testing steps in
a single script. For example, if you are hosting the Grid API service locally on
port 5073, you can run the following command.

```bash
./docker-generate-and-test.sh host.docker.internal:5073
```

This will perform the following steps:

1. All supported client SDKs will be generated.

1. All supported client proxies will be built and hosted on a bridge network in
   Docker.

1. Postman integration tests are run against all client proxies.

NOTE: To ensure the same integration tests can be run against multiple client
SDKs, the test folder contains proxy web APIs. Each of those proxy web APIs
converts standardized requests into one of the native client SDK calls.

## Generate

To generate the clients, you may install the Open API Generator:
<https://openapi-generator.tech/docs/installation>. Then you can run
`./generate-client.sh` to build a full set of supported clients.

With Docker installed, you can also generate the full list of supported clients
using `./docker-generate-clients.sh` without having the Open API Generator
installed. Note that if you need to access localhost on your host machine, there
may be a different name.

In either case, you must provide the hostname of the Grid API server and the
port as a parameter, for example:

```bash
./generate-clients.sh localhost:5073

./docker-generate-clients.sh host.docker.internal:5073
```

The generated clients should not be checked into the repo, they should be
generated to the latest specifications whenever they are needed. The
`.gitignore` file ensures that the supported SDKs are never checked in, however
if new ones are generated, this must be updated.

## Test

Testing of the supported client SDKs is important before the dev branch can be
merged into master. Integration tests have been written in a Postman collection.
To ensure those same integration tests can be run against multiple client SDKs,
the test folder contains proxy web APIs. Each of those proxy web APIs converts
standardized requests into one of the native client SDK calls.

For instance, to test the dotnet client SDK, start the proxy:

```bash
cd tests/csharp
dotnet run
```

Then run the Postman collection (found at tests/tests.postman_collection.json)
against localhost:50000 (or whatever port is specified). You can use one of
these scripts to run the tests:

```bash
cd tests

./test-clients.sh localhost

./docker-test-clients.sh host.docker.internal
```

NOTE: You do not need to specify a port for the test scripts, that will be read
from the .env file.

## Configuration

In the "tests" folder, there is a .env file which contains the configuration
settings used by the client proxies for testing. Those parameters include:

**BASE_URL**: (ex. <http://localhost:5073>) This is the fully qualified base URL
of the Grid API service for which clients will be generated and tested against.

**CSHARP_PORT**: (ex. 50000) This is the port that the C# proxy will run on.
Integration tests should be run against this endpoint to validate the proper
operation of the C# client SDK.
