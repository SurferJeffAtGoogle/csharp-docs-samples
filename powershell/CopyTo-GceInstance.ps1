Param([string] $Instance, [string] $Zone, $Credential)

$Instance = 'instance-3'
$Zone = 'us-central1-f'

$tag = "$Instance-tcp-5986"

gcloud compute instances add-tags $Instance --zone=$Zone --tags=$tag | Write-Host
gcloud compute firewall-rules create $tag --allow=tcp:5986 `
    --description="Allow powershell remote sessions." `
    --target-tags=$tag | Write-Host
$i = gcloud compute instances list $Instance --zones=$Zone --format=json | ConvertFrom-Json
$externalIp = $i.NetworkInterfaces[0].accessConfigs.natIp
if (-not $Credential) { $Credential = Get-Credential }
if ($True) {
    $out = gcloud compute reset-windows-password $Instance --zone $Zone --user $Credential.UserName --quiet
    foreach ($line in $out) {
        if ($line -match 'password:\s+(\S+)') {
            $matches[1] | Write-Host
            $password = ConvertTo-SecureString $matches[1] -AsPlainText -Force
            $Credential = New-Object System.Management.Automation.PSCredential ($Credential.UserName, $password)
        }
    }
}
$session = New-PSSession -ComputerName $externalIp -UseSSL `
    -SessionOption (New-PSSessionOption -SkipCACheck -SkipCNCheck) `
    -Credential $Credential
