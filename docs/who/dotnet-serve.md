# dotnet-serve

[![dotnet-serve](https://img.shields.io/nuget/v/dotnet-serve.svg?color=royalblue&label=dotnet-serve)](https://nuget.org/packages/dotnet-serve)

The [dotnet-serve](https://github.com/natemcmaster/dotnet-serve) is a simple 
command-line HTTP server.

It leverages `dotnet-config` to [augment and reuse options](https://github.com/natemcmaster/dotnet-serve#reusing-options-with-netconfig) 
so they don't have to be passed in constantly via the command line as arguments. 
The hierarchical nature of `.netconfig` makes it very convenient to centralize 
settings related to HTTPS, certificates and others globally (either at the user 
or system level), so that in most cases you just have to run `dotnet serve` from 
any folder and get a consistent behavior throughout your machine.

To save the options used in a particular run to the current directory's `.netconfig`, just append 
`--save-options`:

```
dotnet serve -p 8080 --gzip --cors --quiet --save-options
```

After running that command, a new `.netconfig` will be created (if there isn't one already there) 
with the following section for `dotnet-serve`:

```gitconfig
[serve]
	port = 8080
	gzip
	cors
	quiet
```

You can also add multiple `header`, `mime` type mappings and `exclude-file` entries can be provided as
individual variables, such as:

```gitconfig
[serve]
	port = 8080
	header = X-H1: value
	header = X-H2: value
	mime = .cs=text/plain
	mime = .vb=text/plain
	mime = .fs=text/plain
	exclude-file = app.config
	exclude-file = appsettings.json
```

You can place those settings in any parent folder and it will be reused across all descendent 
folders, or they can also be saved to the global (user profile) or system locations. To easily 
configure these options at those levels, use the `dotnet-config` tool itself:

```
dotnet config --global --set serve.port 8000
dotnet config --global --add serve.mime .csproj=text/plain
```

This will default the port to `8000` whenever a port is not specified in the command line, 
and it will always add the given mime type mapping. You can open the saved `.netconfig` 
at `%USERPROFILE%\.netconfig` or `~/.netconfig`.