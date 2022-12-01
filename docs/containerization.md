# Containerized WebApi

The goal of this readme is to show how to build a container WebApi image that can be used to deploy the application into a container registry and that can be used later to run the service.

## Build and List Runtime Image

Use `docker` to build the WebApi images.
(Note: Make sure the run `docker` at the root branch)

```sh
cd ./$(git rev-parse --show-cdup)/src
docker build -t carbon_aware:v1 -f CarbonAware.WebApi/src/Dockerfile .
```

List `carbon_aware` image 

```sh
docker image ls carbon_aware
REPOSITORY     TAG       IMAGE ID       CREATED             SIZE
carbon_aware   v1        6293e2528bf2   About an hour ago   230MB
```

## Run WebApi Image

1. Run the image using `docker run` with host port 8000 mapped to the WebApi port 80 and configure environment variable settings for [WattTime](https://www.watttime.org) provider.

    ```sh
    docker run --rm -p 8000:80 \
    > -e DataSources__EmissionsDataSource="WattTime" \
    > -e DataSources__ForecastDataSource="WattTime" \
    > -e DataSources__Configurations__WattTime__Type="WattTime" \
    > -e DataSources__Configurations__WattTime__password="username" \
    > carbon_aware:v1
    ```
    or the [ElectricityMaps](https://www.electricitymaps.com) provider

    ```sh
    docker run --rm -p 8000:80 \
    > -e DataSources__ForecastDataSource="ElectricityMaps" \
    > -e DataSources__Configurations__ElectricityMaps__Type="ElectricityMaps" \
    > -e DataSources__Configurations__ElectricityMaps__APITokenHeader="auth-token" \
    > -e DataSources__Configurations__ElectricityMaps__APIToken="<YOUR_ELECTRICITYMAPS_TOKEN>" \
    > carbon_aware:v1
    ```

1. Verify that the WebApi is responding to requests using an HTTP client tool (e.g. `postman`, `curl`)
    ```sh
    curl -v -s -X 'POST' http://localhost:8000/emissions/forecasts/batch  -H 'accept: */*' -H 'Content-Type: application/json' -d '[
        {
            "requestedAt": "2021-11-01T00:00:00Z",
            "dataStartAt": "2021-11-01T00:05:00Z",
            "dataEndAt": "2021-11-01T23:55:00Z",
            "windowSize": 5,
            "location": "eastus"
        }
    ]'
    ...
    > POST /emissions/forecasts/batch HTTP/1.1
    > Host: localhost:8000
    ...
    < HTTP/1.1 200 OK
    < Content-Type: application/json; charset=utf-8
    ...
    < 
    [{"generatedAt":"2021-11-01T00:00:00+00:00","optimalDataPoint":{
        ...
    }}]
    ```

## Upload Image to a Container Registry

For easy image consumption, upload it to a well-known container registry, self-hosted or managed. The following are examples of using [docker hub](https://hub.docker.com) or [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-quickstart-task-cli)

### Docker Hub

Sign in to [Docker Hub](https://hub.docker.com) and create a private (or public) repository (e.g <your-username>/my-private-repo)

1. Build and Push
    ```sh
    docker login --username=your-username
    cd ./$(git rev-parse --show-cdup)/src
    docker build -t <your-username>/my-private-repo/carbon_aware:v1 -f CarbonAware.WebApi/src/Dockerfile .
    docker push <your-username>/my-private-repo/carbon_aware:v1
    ```
1. Pull

    ```sh
    docker login --username=your-username
    docker pull <your-username>/my-private-repo/carbon_aware:v1
    ```

### Azure Container Registry

1. Build and Push image
    Assuming the container registry is already created, use the user's credentials push the image using `docker` (it can be done also using [Azure CLI](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tutorial-quick-task))

    ```sh
    docker login <myacrname>.azurecr.io -u username -p <CopiedKeyFromAzurePortal>
    cd ./$(git rev-parse --show-cdup)/src
    docker build -t <myacrname>.azurecr.io/carbon_aware:v1 -f arbonAware.WebApi/src/Dockerfile .
    docker push <myacrname>.azurecr.io/carbon_aware:v1
    ```
1. Pull image

    ```sh
    docker login <myacrname>.azurecr.io -u username -p <CopiedKeyFromAzurePortal>
    docker pull <myacrname>.azurecr.io/carbon_aware:v1
    ```

## Pipeline Integration (Github Action)

To automate an image deployment from a GitHub CI/CD pipeline, the following link provides detailed information on how to build a `workflow` with all the necesarily tools in order to push an image to a container registry of user's preference (i.e. Docker Hub).

[Github Workflows](https://docs.github.com/en/actions/publishing-packages/publishing-docker-images#publishing-images-to-docker-hub)
