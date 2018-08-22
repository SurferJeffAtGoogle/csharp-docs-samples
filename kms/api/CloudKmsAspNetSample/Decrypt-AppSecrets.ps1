##############################################################################
#.SYNOPSIS
# Decrypts appsecrets.json.encrypted.
#
#.DESCRIPTION
# Uses the kms key named by appsecrets.json.keyname to decrypt 
# appsecrets.json.encrypted.  Overwrites file appsecrets.json.
#
#.EXAMPLE
# .\Decrypt-AppSecrets.ps1
##############################################################################
gcloud kms decrypt --plaintext-file appsecrets.json --ciphertext-file appsecrets.json.encrypted --key (Get-Content appsecrets.json.keyname) 