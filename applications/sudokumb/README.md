# Sudoku + Dumb = Sudokumb.

This is the companion sample for number of [medium.com stories](https://medium.com/@SurferJeff).

**Sudokumb** is a [Sudoku](https://en.wikipedia.org/wiki/Sudoku) solver. In order to demonstrate some cool features of <a href="https://cloud.google.com/">Google Cloud Platform <img src="http://cloud.google.com/_static/images/cloud/products/logos/svg/gcp.svg" width=32></a>,
Sudokumb solves the puzzles in a very dumb way.

## Smart

Most of what Sudokumb does is actually pretty smart.

### <img src="http://cloud.google.com/_static/images/cloud/products/logos/svg/datastore.svg" width=64> Sudokumb stores user info in Datastore.

[Learn more](./DatastoreUserStore/README.md).

### <img src="http://cloud.google.com/_static/images/cloud/products/logos/svg/stackdriver.svg" width=64> Sudokumb logs and reports errors and traces to Stackdriver.

[Learn more](./Stackdriver.md).

### <img src="http://cloud.google.com/_static/images/cloud/products/logos/svg/kms.svg" width=64> Sudokumb secures forms and cookies with [Key Management Service](https://cloud.google.com/kms/)

[Learn more](./KmsDataProtectionProvider/README.md).

### <img src="http://cloud.google.com/_static/images/cloud/products/logos/svg/appengine.svg" width=64> Sudokumb automatically scales on [App Engine](https://cloud.google.com/appengine/docs/flexible/dotnet/).

[Learn more](./AppEngine.md).

## Dumb

### <img src="http://cloud.google.com/_static/images/cloud/products/logos/svg/pubsub.svg" width=64> Sudokumb distributes tiny fragments of work via [Google Cloud Pub/Sub](https://cloud.google.com/pubsub/docs/)

[Learn more](./WebLib/PubSub.md).

# Building and Running locally.

## Prerequisites

Yeah, there's a ton of prerequisites.  But every one of them is necessary.
Hang in there.

1.  **Follow the instructions in the [root README](../../../README.md).**
  
2.  Install the [.NET Core SDK, version 2.0](https://github.com/dotnet/core/blob/master/release-notes/download-archives/1.1.4-download.md).

6.  [Click here](https://console.cloud.google.com/flows/enableapi?apiid=cloudkms.googleapis.com&showconfirmation=true) 
	to enable [Google Cloud Key Management Service](https://cloud.google.com/kms/)
	for your project.

10. Edit [appsettings.json](appsettings.json).

	Replace `YOUR-PROJECT-ID` with your Google project id.


## ![PowerShell](../.resources/powershell.png) Using PowerShell

### Run Locally

```ps1
PS C:\dotnet-docs-samples\appengine\flexible\SocialAuth> dotnet restore
PS C:\dotnet-docs-samples\appengine\flexible\SocialAuth> dotnet run
```
### Deploy to App Engine

6.  Before deploying to app engine, you must copy your user secrets to your Google
project metadata with this powershell script:

	```psm1
	PS C:\dotnet-docs-samples\appengine\flexible\SocialAuth> .\Upload-UserSecrets
	```

7.  Deploy with gcloud:

	```psm1
	PS C:\dotnet-docs-samples\appengine\flexible\SocialAuth> gcloud beta app deploy .\bin\Release\PublishOutput\app.yaml
	```


## ![Visual Studio](../.resources/visual-studio.png) Using Visual Studio

### Run Locally

6.  Before deploying to app engine, you must copy your user secrets to your Google
project metadata with this powershell script:

	```psm1
	PS C:\dotnet-docs-samples\appengine\flexible\SocialAuth> .\Upload-UserSecrets
	```

Open **SocialAuth.csproj**, and Press **F5**.

### Deploy to App Engine

1.  In Solution Explorer, right-click the **SocialAuth** project and choose **Publish SocialAuth to Google Cloud**.

2.  Click **App Engine Flex**.

3.  Click **Publish**.
