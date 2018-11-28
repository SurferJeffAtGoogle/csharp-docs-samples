FROM gcr.io/cloud-devrel-kokoro-resources/dotnet:latest

# Install visual studio code
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
RUN install -o root -g root -m 644 microsoft.gpg /etc/apt/trusted.gpg.d/
RUN sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/vscode stable main" > /etc/apt/sources.list.d/vscode.list'
RUN apt-get update
RUN apt-get install code -y
RUN apt-get install libasound2 -y
ENV SHELL /usr/bin/pwsh

ENTRYPOINT [ "/usr/bin/code", "--user-data-dir", "$HOME", "-n", "-w"]