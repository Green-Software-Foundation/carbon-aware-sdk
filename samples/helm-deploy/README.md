# How-To: Deploy to Kubernetes using Helm

## Setup Dev Environment (optional)

The easiest way to setup your environment is to install [Kind](https://kind.sigs.k8s.io/) or [Minikube](https://minikube.sigs.k8s.io/docs/start/). It is also valid to setup a cloud provider kubernetes cluster (i.e. AKS, EKS, GKE, ...)

## Push an image to the Docker registry

The following steps illustrates how to push a webservice image in order to be
deployed in Kubernetes clsuter.

1. Build an image using the following
   [Dockerfile](.../../../../src/CarbonAware.WebApi/src/Dockerfile)

   ```sh
   cd <Dockerfile path>
   docker build -t <registry>/myapp:v1 .
   ```

1. Login into docker using the username and password credentials that are needed in
   order to push.

   ```sh
   docker login 
   ```

1. Push to Docker registry 

   ```sh
   docker push <registry>/myapp:v1
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

- The `repository` field to be `<image-repository>`
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


### Installing your helm chart

To install your helm chart, you should run

```bash
helm install carbon-aware ./samples/helm-deploy/
```

(If you run into an error, see the [troubleshooting](#troubleshooting) section.
below.)

### Deploying your helm chart

If the installation was successful, helm should give you a console out message
for your to copy and paste to deploy the chart. We've replicated below for quick
reference (you will need to fill in `nameOverride` and `fullNameOverride`):

```bash
NAME: carbon-aware
LAST DEPLOYED: Thu Apr 13 17:39:51 2023
NAMESPACE: default
STATUS: deployed
REVISION: 1
TEST SUITE: None
NOTES:
1. Get the application URL by running these commands:
  export POD_NAME=$(kubectl get pods --namespace default -l "app.kubernetes.io/name=carbon-aware-sdk,app.kubernetes.io/instance=carbon-aware" -o jsonpath="{.items[0].metadata.name}")
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
files/variables.

### Error installing the helm chart

If you get `Error: INSTALLATION FAILED: cannot re-use a name that is still in use` when you
tried to install the helm chart, it means there is still an instance of that
chart installed. If you started this instance, you can simply skip the install
step and continue. If you're unsure that it's the right installation, or you've
made changes, first run `helm uninstall <fullNameOverride>`. Once it's
uninstalled, you can redo the helm install step.


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

- Helm 3: [docs](https://helm.sh/docs/),
[image docs](https://helm.sh/docs/chart_best_practices/pods/#images)

- ContainIQ:
[Troubleshooting ImagePullBackOff Error](https://www.containiq.com/post/kubernetes-imagepullbackoff)
