# dotnet-file

[![dotnet-file](https://img.shields.io/nuget/v/dotnet-file.svg?color=royalblue&label=dotnet-file)](https://nuget.org/packages/dotnet-file)

The [dotnet-file](https://github.com/kzu/dotnet-file) is a dotnet global tool for 
downloading and updating loose files from arbitrary URLs. It uses `dotnet-config` to 
persist the remove URLs and the associated ETags for downloaded files so that performing 
a `dotnet file update` can only download the necessary changes (if any).

Sample configuration:

```gitconfig
# dotnet-file GH repo download/sync for specific subfolders
[file.github "docs"]
	url = https://github.com/dotnet/aspnetcore/tree/master/docs
	url = https://github.com/dotnet/runtime/tree/master/docs/design/features

[file "docs/APIReviewProcess.md"]
	url = https://github.com/dotnet/aspnetcore/blob/master/docs/APIReviewProcess.md
	etag = 1e4acd7e1ac446f0c6d397e1ed517c54507700b85826f64745559dfb50f2acbd
[file "docs/Artifacts.md"]
	url = https://github.com/dotnet/aspnetcore/blob/master/docs/Artifacts.md
	etag = d663b7b460e871c6af17fc288d8bd2d893e29127acf417030254dd239ef42a68
...
[file "docs/design/features/tiered-compilation.md"]
	url = https://github.com/dotnet/runtime/blob/master/docs/design/features/tiered-compilation.md
	etag = 8c2706b687ea4bdaac7ba4caccf29fa529856623e292195dda4aa506e39c3d7d
[file "docs/design/features/unloadability.md"]
	url = https://github.com/dotnet/runtime/blob/master/docs/design/features/unloadability.md
	etag = 4424103e00e2fae42e6a6a8157d139de18026f2acd5d1afd6c727af03c5affeb
```