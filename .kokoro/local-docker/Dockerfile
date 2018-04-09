FROM gcr.io/cloud-devrel-kokoro-resources/dotnet:latest

ARG KOKORO_GFILE_DIR

# Copy secrets from kokoro gfile.
COPY $KOKORO_GFILE_DIR /kokoro_gfile_dir
ENV KOKORO_GFILE_DIR /kokoro_gfile_dir

# Copy in this repo.
RUN git clone -b sudoku https://github.com/SurferJeffAtGoogle/csharp-docs-samples.git /dotnet-docs-samples

WORKDIR /dotnet-docs-samples/
