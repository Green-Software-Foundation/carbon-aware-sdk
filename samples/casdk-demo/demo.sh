#!/bin/sh

THISFILE=$0
BASEDIR=$(dirname $THISFILE)
SUBCMD=$1
CONFIGMAP=/tmp/casdk-config.yaml

help_and_exit () {
  echo "Usage:"
  echo "  $THISFILE [start|stop]"

  rm -f $CONFIGMAP
  exit 1
}

start () {
  # Create ConfigMap for envvars in CASDK container
  cp -f $BASEDIR/casdk-config.yaml.template $CONFIGMAP
  for CASDK_ENV in `env | grep CASDK_`; do
    KEY=`echo $CASDK_ENV | cut -d '=' -f 1 | sed -e 's/^CASDK_//'`
    VALUE=`echo $CASDK_ENV | cut -d '=' -f '2'`

    echo "  $KEY: $VALUE" >> $CONFIGMAP
  done


  # Change security context of nginx-rp.conf because it would be mounted by
  # NGINX container in demo.yaml.
  SELINUX_MODE=`getenforce 2>/dev/null`
  if [ "$SELINUX_MODE" = 'Enforcing' ]; then
    chcon -t container_file_t $BASEDIR/nginx-rp.conf
  fi

  # Start Podman
  # Move to BASEDIR because demo.yaml should refer nginx-rp.conf in that dir.
  pushd $BASEDIR > /dev/null 2>&1
  podman play kube --configmap=$CONFIGMAP demo.yaml
  popd > /dev/null 2>&1
}

stop () {
  podman play kube --down $BASEDIR/demo.yaml
  rm -f $CONFIGMAP
}

case "$SUBCMD" in
  start)
    start;;

  stop)
    stop;;

  *)
    help_and_exit;;
esac
