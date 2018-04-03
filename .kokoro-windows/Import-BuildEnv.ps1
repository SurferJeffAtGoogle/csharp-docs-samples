# Copyright(c) 2017 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License"); you may not
# use this file except in compliance with the License. You may obtain a copy of
# the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
# WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
# License for the specific language governing permissions and limitations under
# the License.

Param ([switch]$Lint)

# Download the contents of gs://cloud-devrel-kokoro-resources/dotnet-docs-samples.
function New-TempDirectory {
    $tempfile = [System.IO.Path]::GetTempFileName()
    remove-item $tempfile
    new-item -type directory -path $tempfile | Out-Null
    return $tempFile
}

$env:KOKORO_GFILE_DIR = New-TempDirectory
$env:INSTALL_DIR = New-TempDirectory

gsutil cp gs://cloud-devrel-kokoro-resources/dotnet-docs-samples/* $env:KOKORO_GFILE_DIR

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip([string]$zipfile, [string]$outpath)
{
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

if ($Lint) {
    # Install codeformatter
    Unzip $env:KOKORO_GFILE_DIR\codeformatter.zip $env:INSTALL_DIR\codeformatter
    $codeformatterInstallPath = Resolve-Path $env:INSTALL_DIR\codeformatter
    $env:PATH = "$codeformatterInstallPath\bin;$env:PATH"

    # Add msbuild 14 to the path for for code-formatter.
    $env:PATH="$env:PATH;C:\Program Files (x86)\MSBuild\14.0\Bin"
    Get-Command MSBuild.exe -ErrorAction Stop

    # Lint the code
    Push-Location
    try {
        Set-Location github\dotnet-docs-samples\
        Import-Module .\BuildTools.psm1
        Lint-Code
    } finally {
        Pop-Location
    }
}

# Install phantomjs
Unzip $env:KOKORO_GFILE_DIR\phantomjs-2.1.1-windows.zip $env:INSTALL_DIR
$env:PATH="$env:INSTALL_DIR\phantomjs-2.1.1-windows\bin;$PATH"

# Install casperjs
Unzip $env:KOKORO_GFILE_DIR\n1k0-casperjs-1.0.3-0-g76fc831.zip $env:INSTALL_DIR
$casperJsInstallPath = Resolve-Path $env:INSTALL_DIR\n1k0-casperjs-76fc831
$env:PATH = "$casperJsInstallPath\batchbin;$env:PATH"
# Patch casperjs
Copy-Item -Force $PSScriptRoot\..\.kokoro\docker\bootstrap.js `
    $casperJsInstallPath\bin\bootstrap.js

# Install casperjs 1.1
Unzip $env:KOKORO_GFILE_DIR\casperjs-1.1.4-1.zip $env:INSTALL_DIR
$casperJsInstallPath = Resolve-Path $env:INSTALL_DIR\casperjs-1.1.4-1
$env:CASPERJS11_BIN = "$casperJsInstallPath\bin"

# Casperjs 1.1 needs python in the path.
$pythonPath = (Get-Command python -ErrorAction SilentlyContinue).Source
if (-not ) {
    $env:PATH = "$env:PATH;C:\Python27"
}

# Install dotnet core sdk.
choco install -y dotnetcore-sdk --version 2.0.0
choco install -y --sxs dotnetcore-sdk --version 1.1.2

# Install nuget command line.
choco install nuget.commandline