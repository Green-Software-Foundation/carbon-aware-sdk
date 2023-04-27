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

1. Write configuration for Carbon Aware SDK to [casdk-config.yaml](casdk-config.yaml).

2. Run demonstration

```
$ podman play kube --configmap=casdk-config.yaml demo.yaml
```

3. Stop demonstration

```
$ podman play kube --down demo.yaml
```
