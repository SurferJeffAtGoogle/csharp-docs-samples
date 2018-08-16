$keyRing = 'WebApp'
$keyId = 'appsecrets'
gcloud kms keys create $keyId --location global --keyring $keyRing --purpose=encryption
$keyName = (gcloud kms keys list --location global --keyring WebApp --format json | ConvertFrom-Json).name | Where-Object {$_ -like "*/$keyId" }
$keyName | Out-File appsecrets.json.keyname