##############################################################################
#.SYNOPSIS
# Encrypts appsecrets.json.
#
#.DESCRIPTION
# Uses the kms key named by appsecrets.json.keyname to encrypt appsecrets.json.
# Overwrites file appsecrets.json.encrypted.
#
#.EXAMPLE
# .\Encrypt-AppSecrets.ps1
##############################################################################
gcloud kms encrypt --plaintext-file appsecrets.json --ciphertext-file appsecrets.json.encrypted --key (Get-Content appsecrets.json.keyname)