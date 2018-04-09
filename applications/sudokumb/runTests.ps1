﻿# Copyright(c) 2017 Google Inc.
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

Import-Module -DisableNameChecking ..\..\BuildTools.psm1

Require-Platform Win*

$randomNamespace = "sudokumb-test-" + (-join (1..20 | foreach {[char] (97 + (Get-Random 26)) }))

dotnet restore Sudokumb.sln
BackupAndEdit-TextFile @("WebApp/appsettings.json", 
						 "WebSolver/appsettings.json") `
	@{"YOUR-PROJECT-ID" = $env:GOOGLE_PROJECT_ID;
	  "sudokumb" = $randomNamespace} `
{
	dotnet build Sudokumb.sln
	if ($LASTEXITCODE) {
		throw "Build failed."
	}
	Push-Location .
	$portNumber = 5510
	$url = "http://localhost:$portNumber"
	try {
		# Launch WebSolver.
		Set-Location ../WebSolver
		$webSolverJob = Run-Kestrel "http://localhost:5511"
		# Launch WebApp.
		Set-Location WebApp
		$webAppJob = Run-Kestrel $url
	} finally {
		# Stop the kestrel jobs.
		if ($webAppJob -or $webSolverJob) {
			@($webSolverJob, $webAppJob) | Stop-Job
		}
		# Clean up datastore.
		Pop-Location
		dotnet run --no-restore --no-build --project 'Remove-DatastoreNamespace/Remove-DatastoreNamespace.csproj' `
			-- -p $env:GOOGLE_PROJECT_ID -n $randomNamespace
	}		
}