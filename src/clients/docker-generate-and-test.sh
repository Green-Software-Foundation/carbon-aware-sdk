if [ -z "$1" ]
then
    echo "You must provide the first parameter as the host name of the service to generate the clients for (ex. host.docker.internal:5073)."
    exit 1
fi

# define cleanup
__cleanup ()
{
    # stop the clients
    echo "stopping the clients..."
    docker stop csharp-c174aa48-8aba-4576-8fbe-6ac10be0032c

    # remove the network
    echo "removing the network..."
    docker network rm network-c174aa48-8aba-4576-8fbe-6ac10be0032c

    # remove the temp env
    rm tests/temp.env
}
trap __cleanup EXIT

# run script to generate clients
echo "generating all clients..."
./docker-generate-clients.sh $1

# create temp.env file
echo "BASE_URL=http://$1\nPORT=80" > tests/temp.env

# build docker images for each client
echo "building the docker images for each client..."
ts=`date +%m-%d-%YT%H-%M-%S`
docker build -t "gsf-csharp-client:$ts" -f ./tests/csharp/Dockerfile .

# build a network
docker network create -d bridge network-c174aa48-8aba-4576-8fbe-6ac10be0032c

# run the clients
echo "starting each client..."
docker run -d --rm --name csharp-c174aa48-8aba-4576-8fbe-6ac10be0032c --network network-c174aa48-8aba-4576-8fbe-6ac10be0032c "gsf-csharp-client:$ts"

# wait for the clients to startup
echo "waiting 30 seconds to ensure all clients are started up..."
sleep 30
set -e

# test csharp
echo "testing the C# client..."
docker run --rm \
  -v ${PWD}:/local \
  --network network-c174aa48-8aba-4576-8fbe-6ac10be0032c \
  postman/newman run /local/tests/tests.postman_collection.json \
  --global-var=base_url=csharp-c174aa48-8aba-4576-8fbe-6ac10be0032c:80
