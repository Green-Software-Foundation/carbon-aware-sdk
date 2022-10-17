#!/bin/bash
set -x

DOTNET_SOLUTION=$1
DEST_PACKAGES=$2
if [ -z $DOTNET_SOLUTION ] || [ -z $DEST_PACKAGES ]
then
    printf "Missing parameters. Usage: $0 DOTNET_SOLUTION DEST_PACKAGES\n"
    printf "Example: $0 src/CarbonAwareSDK.sln /mypackages"
    exit 1
fi

mkdir -p $DEST_PACKAGES
# Remove existing packages
find $DEST_PACKAGES \( -name '*.nupkg' -o -name '*.snupkg' \) -exec rm {} \;
# Setup package metadata
REVISION=$(git rev-parse HEAD)
BRANCH=dev
dotnet build $DOTNET_SOLUTION
dotnet pack $DOTNET_SOLUTION -o $DEST_PACKAGES -c Debug \
    -p:RepositoryBranch=$BRANCH \
    -p:SourceRevisionId=$REVISION \
    -p:IncludeSymbols="true" \
    -p:SymbolPackageFormat="snupkg"
