# Carbon Aware SDK demonstration on Podman

This folder contains an example for Carbon Aware SDK and Swagger UI for the SDK. The user can demonstrate Carbon Aware SDK via Swagger UI with just one command.

This demonstration uses `podman play kube` to deploy apps like a Kubernetes application. Deployment is defined in [demo.yaml](demo.yaml).

## Requirements

- Podman

## Ports to open

Podman creates virtual network, then all of containers in the deployment would be located flatten. So Each containers can access each other as a `localhost`. We need to consider TCP port:

* 8080: Carbon Aware SDK
* 8081: Swagger UI
* 8082: NGINX (for reverse proxy)

NGINX is a reverse proxy to both Carbon Aware SDK and Swagger UI. To avoid CORS error, you can access Swagger UI via NGINX ( http://localhost:8082/swagger-ui/ ).

## Reverse proxy rule

See [nginx-rp.conf](nginx-rp.conf)

/ -> Carbon Aware SDK  
/swagger.yaml -> OpenAPI document provided by Carbon Aware SDK  
/swagger-ui/ -> Swagger UI for Carbon Aware SDK

## How to run

1. Set environment variables prefixed with `CASDK_`: e.g. `CASDK_DataSources__EmissionsDataSource`

2. Start demonstration

```
./demo.sh start
```

:::warning

* [demo.sh](demo.sh) would create `/tmp/casdk-config.yaml` which may contain credentials (e.g. API token of backend service). This file would be removed by `./demo.sh stop`.
* [demo.sh](demo.sh) would change security context of [nginx-rp.conf](nginx-rp.conf) to `container_file_t` if SELinux is enabled. It would not recover in `./demo.sh stop`, so you need to recover manually via `restorecon` if need.

:::

3. Access endpoints (e.g. http://localhost:8082/swagger-ui/ )

4. Stop demonstration

```
./demo.sh stop
```

## Example

Run demonstration with ElectricityMapsFree datasource

```
export CASDK_DataSources__EmissionsDataSource=ElectricityMapsFree
export CASDK_DataSources__Configurations__ElectricityMapsFree__Type=ElectricityMapsFree
export CASDK_DataSources__Configurations__ElectricityMapsFree__token=YOUR_SECRET_TOKEN

./demo.sh start
```
