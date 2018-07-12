Param([string] $Instance, [string] $Zone, $Credential)

$Instance = 'instance-3'
$Zone = 'us-central1-f'

$tag = "$Instance-tcp-5986"

gcloud compute instances add-tags $Instance --zone=$Zone --tags=$tag
gcloud compute firewall-rules create $tag --allow=tcp:5986 `
    --description="Allow powershell remote sessions." `
    --target-tags=$tag
$i = gcloud compute instances list $Instance --zones=$Zone --format=json | ConvertFrom-Json
$externalIp = $i.NetworkInterfaces[0].accessConfigs.natIp
if (-not $Credential) { $Credential = Get-Credential }
New-PSSession -UseSSL -Authentication Digest -ComputerName $externalIp `
    -Credential $Credential