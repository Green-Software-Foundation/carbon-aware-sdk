# Client SDKs

While the web endpoints for the Grid API can be used consumed directly via REST, the GSF is also providing a number of SDKs.

There are several supported SDKs that are tested with each release. Those include:

-   **Java**: _link to the latest package_
-   **Python**: _link to the latest package_
-   **JavaScript**: _link to the latest package_
-   **C#**: _link to the latest package_
-   **Golang**: _link to the latest package_

The SDKs are generated based on the OpenAPI specification provided by the Grid API service using the [Open API Generator](https://openapi-generator.tech/). This same generator can be used to generate other clients. For example, to build a Rust client, you could execute...

```bash
openapi-generator generate -i http://localhost:5073/swagger/v1/swagger.json -g rust -o ./rust
```

Some additional clients that could be generated, but are not supported by the GSF, include:

-   Android
-   C/C++
-   Lua
-   Perl
-   PHP
-   R
-   Rust
-   Swift
-   TypeScript
-   and many more can be found here: <https://openapi-generator.tech/docs/generators>

## Generate

To generate the clients, you may install the Open API Generator: <https://openapi-generator.tech/docs/installation>. Then you can run `./generate-client.sh` to build a full set of supported clients.

With Docker installed, you can also generate the full list of supported clients using `./docker-generate-clients.sh` without having the Open API Generator installed. Note that if you need to access localhost on your host machine, there may be a different name.

In either case, you must provide the hostname of the Grid API server as a parameter, for example:

```bash
./generate-clients.sh localhost:5073

./docker-generate-clients.sh localhost:5073

./docker-generate-clients.sh docker.for.mac.localhost:5073
```

The generated clients should not be checked into the repo, they should be generated to the latest specifications whenever they are needed. The `.gitignore` file ensures that the supported SDKs are never checked in, however if new ones are generated, this must be updated.

## Test

Testing of the supported client SDKs is important before the dev branch can be merged into master. Integration tests have been written in a Postman collection. To ensure those same integration tests can be run against multiple client SDKs, the test folder contains proxy web APIs. Each of those proxy web APIs converts standardized requests into one of the native client SDK calls.

For instance, to test the dotnet client SDK, start the proxy:

```bash
cd tests/csharp
dotnet run
```

Then run the Postman collection (found at tests/tests.postman_collection.json) against localhost:50000 (or whatever port is specified).
