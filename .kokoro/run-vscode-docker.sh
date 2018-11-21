# Build the docker image that includes visual studio code.
pushd .
cd vscode-docker
docker build -t gcr.io/cloud-devrel-kokoro-resources/dotnet-vscode .
popd

# Add permission for the root user to display windows on your desktop.
xhost local:root

# Run 
docker run -d \
    --net="host" \
    -h dotnet-docs-samples-vscode \
    -e DISPLAY=$DISPLAY \
    -e MYUID=$(id -u) \
    -e MYGID=$(id -g) \
    -e MYUSERNAME=$(id -un) \
    -e SSH_AUTH_SOCK=$SSH_AUTH_SOCK \
    -v $(dirname $SSH_AUTH_SOCK):$(dirname $SSH_AUTH_SOCK) \
    -v /tmp/.X11-unix:/tmp/.X11-unix \
    -v $HOME:$HOME \
    -w $HOME \
    gcr.io/cloud-devrel-kokoro-resources/dotnet-vscode
