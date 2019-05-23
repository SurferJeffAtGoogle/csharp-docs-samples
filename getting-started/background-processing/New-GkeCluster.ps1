$projectId = gcloud config get-value project
# Choose a zone near you.  Use `gcloud compute zones list` to see the full list.
$zone = us-central-1f

# Create a service account.  The processes in the GKE cluster will run
# using this service account.
gcloud iam service-accounts create translator

# Give the service account permission to use exactly the services we need.
$roles = 'storage.objectViewer', `
    'logging.logWriter', 'monitoring.metricWriter', 'monitoring.viewer', `
    'pubsub.publisher', 'pubsub.subscriber', 'datastore.user'
foreach ($role in $roles) {
    gcloud projects add-iam-policy-binding $projectId `
        --member=serviceAccount:translator@$projectId.iam.gserviceaccount.com `
        --role=roles/$role
}

# Create the GKE cluster.
gcloud container clusters create translate `
    --service-account=translator@$projectId.iam.gserviceaccount.com `
    --zone=$zone

# Create a pubsub topic and subscription, if they don't already exist.
$topicExists = gcloud pubsub topics describe translate-requests 2> $null 
if (-not $topicExists) {
    gcloud pubsub topics create translate-requests
}
$subscriptionExists = gcloud pubsub subscriptions describe translate-requests 2> $null
if ($subscriptionExists) {
    # Delete the old one to shutdown because there's no way to convert a
    # push subscription to a pull subscription.
    gcloud beta pubsub subscriptions delete translate-requests 
}
gcloud beta pubsub subscriptions create translate-requests

