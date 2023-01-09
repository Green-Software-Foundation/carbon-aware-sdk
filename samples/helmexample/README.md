# How-To: Deploy to AKS using Helm

## Setup Dev Environment (optional)

The easiest way to setup your environment is to use a VSCode dev container.
Suggested setup is with the latest `debian` OS with `az CLI` and `kubectl-helm`
packages enabled.

## Setup Resource Group in Azure

1. Create a new Kubernetes service following the default settings
1. Create a container registry
1. Create a key vault

## Push an image to the container registry

The following steps illustrates how to push a webservice image in order to be
deployed in AKS using ACR (Azure Container Registry).

1. Build a `published` image using for instancee the following
   [Dockerfile](https://docs.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows#create-the-dockerfile)

   ```sh
   docker build -t myapp:v1 .
   ```

1. Login into ACR using the username and password credentials that are needed in
   order to push. See Access Keys section of the ACR Portal.

   ```sh
   docker login <acrloginserver>.azurecr.io
   ```

1. Tag the image following ACR tagging scheme

   ```sh
   docker tag myapp <acrloginserver>.azurecr.io/myapp:v1
   ```

1. Push to ACR

   ```sh
   docker push <acrloginserver>.azurecr.io/myapp:v1
   ```

After these steps, using Azure's Portal ACR resource an image should be
available under repositories following the naming convention mentioned above.

### Give cluster access to ACR

To attach the ACR to the cluster so that the image can be accessed, run

```bash
az aks update -n <cluster-name> -g <resource-group-name> --attach-acr <acr-name>
```

## Create a new Helm chart (optional)

Run `helm` to ensure you have the helm CLI running properly. Then you can create
a new helm chart by running

```bash
helm create <chart-name>
```

## Setting up the Helm chart

Once you've got your helm chart open (whether from scratch or existing), the
main files you will likely be working with are `Chart.yaml` and `values.yaml`.

### Chart.yaml

In `Chart.yaml`, we won't need to make any changes but make note of the chart
`name`, as you will need to reference it in commands later on.

### Values.yaml

In `values.yaml`, we need to change a couple fields. In the `image` section, you
will need to set

- The `repository` field to be `<acr-login-server>/<image-repository>`
- The `pullPolicy` field to be `IfNotPresent` (pulls a new image if not present)
  or `Always` (pulls a new image every time).
- The `tag` field if you need a particuar tag version

Set the `nameOverride` and `fullNameOverride` fields to make it easier to
reference your helm chart, and ensure they are not the same.

In the `serviceAccount` section, ensure that

- The `name` field is set to the name of the helm chart from `Chart.yaml`.

In the `monitorConfig` section, you will need to adjust the `liveness` and
`readiness` that the helm chart will ping to ensure the sdk is ready. As a
default, you should set the path to `/health`.

## Connecting to AKS from Helm

To connect to AKS you need to be logged into the azure. Run `az login` and
follow the prompts to get access to your azure subscriptions.

To set the right subscription for the service, run:

```bash
az account set --subscription <subscription-id>
```

To give credentials for the resource group to the kubernetes service, run:

```bash
az aks get-credentials --resource-group <resource-group-name> --name <kubernetes-service-name>
```

With these two commands, helm should be setup to access AKS and be able to
deploy on it. With the current setup, Helm is not directly accessing the ACR to
pull the image, put instead is going through the cluster (which is why we gave
the cluster authorized access to the ACR in the earlier section). If you do need
helm to access your ACR for any reason, you will need to register it and login
with the following

```bash
helm registry login <acr-login-server> \
  --username <acr-username> \
  --password <acr-password>
```

The neccessary credentials can be found by opening your ACR in the Azure portal
and going to credentials.

### Installing your helm chart

To install your helm chart, you should run

```bash
helm install <fullNameOverride> <helm-chart-name>/
```

(If you run into an error, see the [troubleshooting](#troubleshooting) section.
below.)

### Deploying your helm chart

If the installation was successful, helm should give you a console out message
for your to copy and paste to deploy the chart. We've replicated below for quick
reference (you will need to fill in `nameOverride` and `fullNameOverride`):

```bash
export POD_NAME=$(kubectl get pods --namespace default -l "app.kubernetes.io/name=<nameOverride>,app.kubernetes.io/instance=<fullNameOverride>" -o jsonpath="{.items[0].metadata.name}")
export CONTAINER_PORT=$(kubectl get pod --namespace default $POD_NAME -o jsonpath="{.spec.containers[0].ports[0].containerPort}")
echo "Visit http://127.0.0.1:8080 to use your application"
kubectl --namespace default port-forward $POD_NAME 8080:$CONTAINER_PORT
```

If the deployment works properly, you should be able to visit the link they
provide and use it to query the SDK

## Troubleshooting

### Error connecting to the kubernetes service

If you get

```text
Error: INSTALLATION FAILED: Kubernetes cluster unreachable: Get "http://localhost:8080/version": dial tcp 127.0.0.1:8080: connect: connection refused
```

that means that helm cannot connect to kubernetes. Helm connects to kuberentes
either via the $KUBECONFIG env variable, or (if it's not set) looks for the
default location kubectl files are location (`~/.kube/config`). This may occur
the first time to try connecting the helm chart or if you clear the
files/variables. To fix, follow the cli commands in the
[connecting to AKS](#connecting-to-aks-from-helm) section and that should
automatically generate the proper config files.

### Error installing the helm chart

If you get
`Error: INSTALLATION FAILED: cannot re-use a name that is still in use` when you
tried to install the helm chart, it means there is still an instance of that
chart installed. If you started this instance, you can simply skip the install
step and continue. If you're unsure that it's the right installation, or you've
made changes, first run `helm uninstall <fullNameOverride>`. Once it's
uninstalled, you can redo the helm install step.

### Error pulling image

If there is an issue pulling the image from ACR, the pod will deploy but will
fail to start. If you check the status of the pod (using the kubectl commands
below in the azure portal) you will see the `ImagePullBackOff` status and a note
that the pod has not started. This may be for a couple reasons:

1. The cluster isn't authorized to access the ACR registry. Ensure you've run
   [this command](#give-cluster-access-to-acr).
2. The image reference in helm is incorrect. Review the Values.yaml
   [section](#valuesyaml) to ensure you've got the right reference in the
   image repository field.

### Error deploying the helm chart

If you get an error deploying the helm chart and have ensured the image is
pulling properly, one possible error may be with the liveliness and readiness
probes. If those are failing, the deployment will fail to start properly. Ensure
that the paths provided in the `deployment.yaml` file are valid and that the sdk
can actual spin up correctly.

### Useful kubectl commands to check on cluster

- List all deployments in all namespaces:
  `kubectl get deployments --all-namespaces=true`
- List details about a specific deployment:
  `kubectl describe deployment <deployment-name> --namespace <namespace-name>`
- Delete a specific deployment: `kubectl delete deployment <deployment-name>`
- List pods: `kubectl get pods`
- Check on a specific pod: `kubectl describe pod <pod-name>`
- Get logs on a specific pod: `kubectl logs <pod-name>`
- Delete a specific pod: `kubectl delete pod <pod-name>`

## References

Helm 3: [docs](https://helm.sh/docs/),
[image docs](https://helm.sh/docs/chart_best_practices/pods/#images)

MS Docs:
[Creating an ingress controller in AKS](https://docs.microsoft.com/en-us/azure/aks/ingress-basic?tabs=azure-cli),
[Authenticate with ACR from AKS](https://docs.microsoft.com/en-us/azure/aks/cluster-container-registry-integration?tabs=azure-cli#access-with-kubernetes-secret)

Github Issue:
[Deploying a container from ACR to AKS](https://github.com/MicrosoftDocs/azure-docs/issues/33430)

ContainIQ:
[Troubleshooting ImagePullBackOff Error](https://www.containiq.com/post/kubernetes-imagepullbackoff)
