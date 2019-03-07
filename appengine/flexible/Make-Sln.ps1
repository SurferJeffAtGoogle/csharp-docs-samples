$csprojs = gci -Recurse *.csproj
foreach ($csproj in $csprojs) {
    $sln = $csproj.DirectoryName + '\' + $csproj.BaseName + '.sln'
    if (Test-Path $sln) {
        continue  # Solution file already exists.  Nothing to do.
    }
    Push-Location .
    Set-Location $csproj.DirectoryName
    dotnet new sln
    dotnet sln add $csproj.Name
    Pop-Location
}
