# Sleet

[![sleet](https://img.shields.io/nuget/v/sleet.svg?color=royalblue&label=sleet)](https://nuget.org/packages/sleet)

[Sleet](https://github.com/emgarten/Sleet) is a static (serverless) NuGet package 
feed generator, which supports for a JSON format as well as `.netconfig`.

All the available [settings](https://github.com/emgarten/Sleet/blob/master/doc/client-settings.md) 
are supported via `.netconfig`. 

Some examples are:

1. Azure Blob Storage-based feed:

```gitconfig
[sleet "feed"]
	type = azure
	container = feed
	connectionString = "DefaultEndpointsProtocol=https;AccountName=;AccountKey=;BlobEndpoint="
	path = https://yourStorageAccount.blob.core.windows.net/feed/
```

2. AWS S3-based feed:

```gitconfig
[sleet "feed"]
	type = s3
	path = https://s3.amazonaws.com/my-bucket-feed/
	bucketName = my-bucket-feed
	region = us-west-2
	accessKeyId = IAM_ACCESS_KEY_ID
	secretAccessKey = IAM_SECRET_ACCESS_KEY
```

3. Local directory feed:
```gitconfig
[sleet "myLocalFeed"]
	type = local
	path = C:\\myFeed
```

See the [docs](https://github.com/emgarten/Sleet/blob/master/doc/index.md) for all 
available settings. 