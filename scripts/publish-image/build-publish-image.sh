#!/usr/bin/env bash

set -o errexit
set -o nounset
set -o pipefail

if ! type docker > /dev/null; then
  exit 1
fi

: "${REGISTRY:=${REGISTRY}}"
: "${SDK_IMG_NAME:=carbon-aware}"
: "${SDK_TAG:=${SDK_TAG}}"

error() {
    echo "$@" >&2
    exit 1
}

OUTPUT_TYPE=type=registry
BUILDPLATFORM=linux/amd64
BUILDX_BUILDER_NAME=img-builder
QEMU_VERSION=5.2.0-2

if ! docker buildx ls | grep $BUILDX_BUILDER_NAME; then \
    docker run --rm --privileged multiarch/qemu-user-static:$QEMU_VERSION --reset -p yes; \
    docker buildx create --name $BUILDX_BUILDER_NAME --use; \
    docker buildx inspect $BUILDX_BUILDER_NAME --bootstrap; \
fi

docker buildx build \
    --file Dockerfile \
    --output=$OUTPUT_TYPE \
    --platform="$BUILDPLATFORM" \
    --pull \
    --tag $REGISTRY/$SDK_IMG_NAME:$SDK_TAG .
