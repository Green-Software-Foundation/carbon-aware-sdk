# exit on first error
set -e

# get env
set -o allexport
source .env
set +o allexport

# csharp
docker run --rm \
  -v ${PWD}:/local \
  postman/newman run /local/tests.postman_collection.json \
  --global-var=base_url=docker.for.mac.localhost:$CSHARP_PORT
