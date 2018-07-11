Param([string] $User, [string] $Password, [string] $Instance)

$tag = "$Instance-tcp-5986"

gcloud compute instances add-tags $Instance --tags=$tag
gcloud compute firewall-rules create --allow=tcp:5986 `
    --description="Allow powershell remote sessions." `
    --target-tags=$tag

