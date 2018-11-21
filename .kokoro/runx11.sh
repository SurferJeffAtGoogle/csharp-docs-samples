#!/bin/bash

xhost local:root

docker run -ti \
    --net="host" \
    --name=vscode \
    -h vscode \
    -e DISPLAY=$DISPLAY \
    -e MYUID=$(id -u) \
    -e MYGID=$(id -g) \
    -e MYUSERNAME=$(id -un) \
    -e SSH_AUTH_SOCK=$SSH_AUTH_SOCK \
    -v $(dirname $SSH_AUTH_SOCK):$(dirname $SSH_AUTH_SOCK) \
    -v /tmp/.X11-unix:/tmp/.X11-unix \
    -v $HOME:$HOME \
    -w $HOME \
    vscode
