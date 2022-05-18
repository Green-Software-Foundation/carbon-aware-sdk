# exit on first error
set -e

# get env
set -o allexport
source .env
set +o allexport

# csharp
newman run tests.postman_collection.json --global-var=base_url=localhost:$CSHARP_PORT
