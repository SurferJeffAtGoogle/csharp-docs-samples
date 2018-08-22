##############################################################################
#.SYNOPSIS
# Create a new kms encryption key and store it in appsecrets.json.keyname
#
#.PARAMETER keyRingId
# The key ring id to store the key in.
#
#.PARAMETER keyId
# The id for the new key.
#
#.OUTPUTS
# The full name of the new key. 
#
#.EXAMPLE
# .\New-EncryptionKey.ps1
# projects/<your-project-id>/locations/global/keyRings/webapp/cryptoKeys/appsecrets
##############################################################################
Param ([string]$keyRingId = 'webapp', [string]$keyId = 'appsecrets')

# Check to see if the key ring already exists.
$globalKeyRings = (gcloud kms keyrings list --format json --location global | convertfrom-json).name
$matchingKeyRing = foreach ($globalKeyRing in $globalKeyRings) {
    if ($globalKeyRing.EndsWith('/' + $keyRingId)) {
        $globalKeyRing
        break
    }
}
if (-not $matchingKeyRing) {
    # Create the new key ring.
    Write-Information "Creating new key ring $keyRingId..." 
    gcloud kms keyrings create $keyRingId --location global
}

# Create the new key.
Write-Information "Creating new key $keyId..."
gcloud kms keys create $keyId --location global --keyring $keyRingId --purpose=encryption
# Write the new key name to appsecrets.json.keyname.
$keyName = (gcloud kms keys list --location global --keyring $keyRingId --format json | ConvertFrom-Json).name | Where-Object {$_ -like "*/$keyId" }
$keyName | Out-File appsecrets.json.keyname
$keyName