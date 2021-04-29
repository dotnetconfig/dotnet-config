## API

There are three main ways to access *.netconfig* values:

* [Native API](#native-api) for direct access to .netconfig values
* [Microsoft.Extensions.Configuration](#microsoftextensionsconfiguration) provider
* [System.CommandLine](#systemcommandline) for CLI apps

### Native API

[![Version](https://img.shields.io/nuget/v/DotNetConfig.svg?color=royalblue)](https://www.nuget.org/packages/DotNetConfig)
[![Downloads](https://img.shields.io/nuget/dt/DotNetConfig.svg?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig)

```
PM> Install-Package DotNetConfig
```

The main usage for .NET tool authors consuming the [DotNetConfig](https://www.nuget.org/packages/DotNetConfig) 
API is to first build a configuration from a specific path (will assume current directory 
if omitted):

```csharp
var config = Config.Build();
```

The resulting configuration will contain the hierarchical variables set in the 
current directory (or the given path), all its ancestor directories, plus global 
and system locations.

When getting values, the supported primitives are exposed as first-class methods 
for `Add`, `Get` and `Set`, so you get quite a few usability overloads for 
each of `Boolean`, `DateTime`, `Number` and `String`, such as `AddBoolean`, 
`GetDateTime`, `GetString` or `SetNumber`:

```csharp
// reads from:
// [mytool]
//   enabled = true

bool? enabled = config.GetBoolean("mytool", "enabled");

// reads from:
// [mytool.editor]
//   path = code.exe

string? path = config.GetString("mytool.editor", "path");


// reads from:
// [mytool "src/file.txt"]
//   createdOn = 2020-08-23T12:00:00Z

DateTime? created = config.GetDateTime("mytool", "src/file.txt", "createdOn");
// If value was not found, set it to the current datetime
if (created == null)
    // Would create the section if it did not previously exist, and add the variable
    config.SetDateTime("mytool", "src/file.txt", "createdOn", DateTime.Now);
```

Alternatively you can use the `TryGetXXX` methods instead, to avoid checking for 
null return values in cases where the variable (in the requested section and 
optional subsection) is not found:

```csharp
if (!config.TryGetDateTime("mytool", "src/file.txt", "createdOn", out created))
    config.SetDateTime("mytool", "src/file.txt", "createdOn", DateTime.Now);
```


Since `.netconfig` supports multi-valued variables, you can retrieve all entries as 
`ConfigEntry` and inspect or manipulate them granularly:

```csharp
foreach (ConfigEntry entry in config.GetAll("proxy", "url"))
{
    // entry.Level allows inspecting the location where the entry was read from
    if (entry.Level == ConfigLevel.System)
        // entry came from Environment.SpecialFolder.System
    else if (entry.Level == ConfigLevel.Global)
        // entry came from Environment.SpecialFolder.UserProfile
    else if (entry.Level == ConfigLevel.Local)
        // entry came from .netconfig.user file in the current dir or an ancestor directory
    else
        // local entry from current dir .netconfig or an ancestor directory

    Console.WriteLine(entry.GetString());
    // entry.GetBoolean(), entry.GetDateTime(), entry.GetNumber()
}
```

When writing values (via `AddXXX` or `SetXXX`) you can optionally specify the 
configuration level to use for persisting the value, by passing a `ConfigLevel`:

```csharp
// writes on the global .netconfig in the user's profile
//[vs "alias"]
//	comexp = run|community|exp

config = config.AddString("vs", "alias", "comexp", "run|community|exp", ConfigLevel.Global);
```

> IMPORTANT: the Config API is immutable, so if you make changes, you should update your reference
> to the newly updated Config, otherwise, subsequent changes would override prior ones.

You can explore the entire API in the [docs site](https://dotnetconfig.org/api/).

### Microsoft.Extensions.Configuration

[![Version](https://img.shields.io/nuget/v/DotNetConfig.Configuration.svg?color=royalblue)](https://www.nuget.org/packages/DotNetConfig.Configuration)
[![Downloads](https://img.shields.io/nuget/dt/DotNetConfig.Configuration.svg?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig.Configuration)

```
PM> Install-Package DotNetConfig.Configuration
```

Usage (in this example, also chaining other providers):

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile(...)
    .AddEnvironmentVariables()
    .AddIniFile(...)
    .AddDotNetConfig();
```

Given the following .netconfig: 

```gitconfig
[serve]
	port = 8080

[security "admin"]
    timeout = 60
```

You can read both values with:

```csharp
string port = config["serve:port"];  // == "8080";
string timeout = config["security:admin:timeout"];  // == "60";
```

### System.CommandLine

[![Version](https://img.shields.io/nuget/v/DotNetConfig.CommandLine.svg?color=royalblue)](https://www.nuget.org/packages/DotNetConfig.CommandLine)
[![Downloads](https://img.shields.io/nuget/dt/DotNetConfig.CommandLine.svg?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig.CommandLine)

Given the explicit goal of making **.netconfig** a first-class citizen among dotnet (global) tools, it offers 
excelent and seamless integration with [System.CommandLine](https://www.nuget.org/packages/System.CommandLine).

Let's asume you create a CLI app named `package` which manages your local cache of packages (i.e. NuGet). 
You might have a couple commands, like `download` and `prune`, like so:

```csharp
var root = new RootCommand
{
  new Command("download")
  {
    new Argument<string>("id")
  },
  new Command("prune")
  {
    new Argument<string>("id"),
    new Option<int>("days")
  },
}.WithConfigurableDefaults("package");
```

The added `WithConfigurableDefaults` invocation means that now all arguments and options can have 
their default values specified in config, such as:

```gitconfig
[package]
  id = DotNetConfig

[package "prune"]
  days = 30
```

Note how the `id` can be specified at the top level too. The integration will automatically promote 
configurable values to ancestor sections as long as they have compatible types (both `id` in `download` 
and `prune` commands are defined as `string`).

Running `package -?` from the command line will now pull the rendered default values from config, so 
you can see what will actually be used if the command is run with no values:

```
Usage:
  package [options] [command]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  download <id>  [default: DotNetConfig]
  prune <id>     [default: DotNetConfig]
```

And `package prune -?` would show:

```
Usage:
  package [options] prune [<id>]

Arguments:
  <id>  [default: DotNetConfig]

Options:
  --days <days>   [default: 30]
  -?, -h, --help  Show help and usage information
```

Since **.netconfig** supports multi-valued variables, it's great for populating default 
values for arguments or options that can be specified more than once. By making this 
simple change to the argument above:

```csharp
    new Argument<string[]>("id")
```

We can now support a configuration like the following:

```gitconfig
[package]
  id = DotNetConfig
  id = Moq
  id = ThisAssembly
```

And running the command with no `id` argument will now cause the handler to receive all three. You 
can also verify that this is the case via `download -?`, for example:

```
Usage:
  package [options] download [<id>...]

Arguments:
  <id>  [default: DotNetConfig|Moq|ThisAssembly]

Options:
  -?, -h, --help  Show help and usage information
```

All the types supported by System.CommandLine for multiple artity arguments and options are 
automatically populated: arrays, `IEnumerable<T>`, `ICollection<T>`, `IList<T>` and `List<T>`. 

For numbers, the argument/option can be either `long` or `int`. Keep in mind that since numbers in 
**.netconfig** are always `long`, truncation may occur in the latter case.
