![Icon](https://raw.githubusercontent.com/dotnetconfig/dotnet-config/main/docs/img/icon-32.png) dotnet-config
============

[![CLI NuGet](https://img.shields.io/nuget/v/dotnet-config.svg?label=nuget.cli&color=royalblue)](https://www.nuget.org/packages/dotnet-config) [![API NuGet](https://img.shields.io/nuget/v/DotNetConfig.svg?label=nuget.api&color=royalblue)](https://www.nuget.org/packages/DotNetConfig) [![Downloads](https://img.shields.io/nuget/dt/DotNetConfig?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/dotnetconfig/dotnet-config/blob/main/license.txt) [![Discord Chat](https://img.shields.io/badge/chat-on%20discord-7289DA.svg)](https://discord.gg/x4qhjYd) [![GitHub](https://img.shields.io/badge/-github-181717.svg?logo=GitHub)](https://github.com/dotnetconfig/dotnet-config) 

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/dotnet-config/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json) [![CI Status](https://github.com/dotnetconfig/dotnet-config/workflows/build/badge.svg?branch=main)](https://github.com/dotnetconfig/dotnet-config/actions?query=branch%3Amain+workflow%3Abuild+)

<p>
<b><a href="#why">Why</a></b>
|
<b><a href="#what">What</a></b>
|
<b><a href="#who">Who</a></b>
|
<b><a href="#how">How</a></b>
|
<b><a href="#format">Format</a></b>
|
<b><a href="#api">API</a></b>
|
<b><a href="#cli">CLI</a></b>
</p>

# Why

`dotnet-config` (or `.netconfig`) provides a uniform mechanism for 
[.NET Core tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) to store and 
read configuration values in a predictable format which can be manipulated through a command 
line tool, an API and also manually in any text editor by the user.

Just like [git config](https://git-scm.com/docs/git-config) provides a uniform way of storing 
settings for all git commands, the goal of `dotnet-config` is to foster the same level of 
consistency across all .NET tools. The format is (mostly) compatible with it too and therefore 
leverages the learnings of the git community around configuration for arbitrary tools.

# What

`dotnet-config` provides the following:
* A well-documented file format than can be hand-edited in any text editor.
* A dotnet global tool to manage the configuration files (much like [git config](https://git-scm.com/docs/git-config)).
* An API for dotnet tool authors to read/write settings.

By default, configuration files are named `.netconfig` and support four storage levels: 
* Local: a `.netconfig.user` file alongside the `Default` level.
* Default: current directory and any ancestor directories.
* Global: user profile directory, from [System.Environment.SpecialFolder.UserProfile](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=netstandard-2.0#fields).
* System: system-wide directory, from [System.Environment.SpecialFolder.System](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=netstandard-2.0#fields).

The files are read in the order given above, with first value found taking precedence. 
When multiple values are read then all values of a key from all files will be returned.

> `.netconfig.user` can be used to keep local-only settings separate from team-wide settings 
> in source control, and it's already a commonly ignored extension in 
> [.gitignore](https://github.com/github/gitignore/blob/master/VisualStudio.gitignore#L9).

# Who

The following are some of the tools that leverage *.netconfig* to provide flexible configuration persistence 
options:

[![dotnet-eventgrid](https://img.shields.io/nuget/v/dotnet-eventgrid.svg?color=royalblue&label=dotnet-eventgrid)](https://dotnetconfig.org/who/dotnet-eventgrid)
[![dotnet-file](https://img.shields.io/nuget/v/dotnet-file.svg?color=royalblue&label=dotnet-file)](https://dotnetconfig.org/who/dotnet-file)
[![dotnet-serve](https://img.shields.io/nuget/v/dotnet-serve.svg?color=royalblue&label=dotnet-serve)](https://dotnetconfig.org/who/dotnet-serve)
[![dotnet-vs](https://img.shields.io/nuget/v/dotnet-vs.svg?color=royalblue&label=dotnet-vs)](https://dotnetconfig.org/who/dotnet-vs)
[![reportgenerator](https://img.shields.io/nuget/v/dotnet-reportgenerator-globaltool.svg?color=royalblue&label=reportgenerator)](https://dotnetconfig.org/who/reportgenerator)
[![sleet](https://img.shields.io/nuget/v/sleet.svg?color=royalblue&label=sleet)](https://dotnetconfig.org/who/sleet)

Learn more about how the various tools leverage `.netconfig` in the [Who](https://dotnetconfig.org/who) 
section of our docs site.


# How

## Format

Example file:

```gitconfig
# .netconfig is awesome: https://dotnetconfig.org

[serve]
	port = 8080
	gzip                    #shorthand for gzip=true

[vs "alias"]
	comexp = run|community|exp
	preexp = run|preview|exp

[file]
	# example of multi-valued variables
	url = https://github.com/dotnet/runtime/tree/master/docs/design/features
	url = https://github.com/dotnet/aspnetcore/tree/master/docs

; subsections allow grouping variables
[file "docs/design/features/code-versioning.md"]
	url = https://github.com/dotnet/runtime/blob/master/docs/design/features/code-versioning.md
	etag = 7405567...

[file "docs/APIReviewProcess.md"]
	url = https://github.com/dotnet/aspnetcore/blob/master/docs/APIReviewProcess.md
	etag = 1e4acd7...

[mytool]
    description = "\t tab and \n newline escapes, plus \\ backslash are valid"
    title = My tool is great    # internal whitespace preserved without needing double quotes
    path = C:\\tool             # backslashes always escaped, inside or outside double quotes
    size = 500kb                # numbers can have a multiplier (case insensitive) suffix kb, mb, gb, tb 
    max-size = 1T               # the 'b' is optional.
    compress = true             # multiple variants for boolean: true|false|yes|no|on|off|1|0
    secure   = yes
    localized = off
    enabled  = 1

; array like syntax for complex objects
[myArray "0"] # indecies must be unique
    description = 1st item description
    name = Fero
[myArray "1"]
    description = 2nd item description
    name = Jozo
```

The syntax follows closely the [git-config syntax](https://git-scm.com/docs/git-config#_syntax). 
The `#` and `;` characters begin comments to the end of line, blank lines are ignored.

The file consists of **sections** and **variables**. A section begins with the name of the section in 
square brackets and continues until the next section begins. Section names are case-insensitive. 
Only alphanumeric characters and `-` are allowed in section names. Each variable must belong 
to some section, which means that there must be a section header before the first setting of a 
variable. 

Sections can be further divided into **subsections**. To begin a subsection put its name in double 
quotes, separated by space from the section name, in the section header, like in the example below:

```gitconfig
[section "subsection"]
```

Subsection names are case sensitive and can contain any characters except newline. Doublequote `"` 
and backslash `\` can be included by escaping them as `\"` and `\\`, respectively. 

Section headers cannot span multiple lines. Variables may belong directly to a section 
or to a given subsection. You can have `[section]` if you have `[section "subsection"]`, but you 
don't need to.

All the other lines are recognized as setting **variables**, in the form `name = value` (or just `name`, 
which is a short-hand to say that the variable is the boolean `true`). Variable names are case-insensitive, 
allow only alphanumeric characters and `-`, and must start with an alphabetic character. Variables may 
appear multiple times; we say then that the variable is *multivalued*.

Leading whitespaces after `name =`, the remainder of the line after the first comment character `#`
or `;`, and trailing whitespaces of the line are discarded unless they are enclosed in double quotes. 
Internal whitespaces within the value are retained verbatim.

Backslash `\` characters must always be escaped with `\\`. Double quotes must either be escaped with 
`\"` or be properly balanced, which causes the whitespace within to be preserved verbatim.

Beside `\"` and `\\`, only `\n` for newline character (NL) and `\t` for horizontal tabulation (HT, TAB) 
escape sequences are recognized.


> NOTE: when using the CLI or API, these escaping rules are applied automatically

### Values

Values of many variables are treated as a simple string, but there are variables that take values of 
specific types and there are rules as to how to spell them.

* *boolean*

	When a variable is said to take a *boolean* value, many synonyms are accepted for `true` and 
	`false`; these are all case-insensitive.
	
	* *true*: boolean true literals are `yes`, `on`, `true`, and `1`. Also, a variable defined without 
	`=<value>` is taken as `true`.

	* *false*: boolean false literals are `no`, `off`, `false`, `0` and the empty string.

* *datetime*

	Variables of this type are always parsed/written using ISO 8601 (or [round-trip](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-round-trip-o-o-format-specifier)) format.

* *number*

	The value for many variables that specify various sizes can be suffixed with `k`, `M`, `G` or `T` 
	to mean	"scale the number by 1024", "by 1024x1024", "by 1024x1024x1024" or "by 1024x1024x1024x1024"
	respectively. The suffix is case insensitive, and can also include the `b`, as in `kb` or `MB`.

### Array of complex objects

Creation of more complex objects in array is possible via subsections. Lets say that we have following configuration object:

```csharp
public class WatchedProcess
{
    public string Name { get; set; }
    public string ApplicationPath { get; set; }
}
```

We would like to retrieve from our configuration as `IList<WatchedProcess>`. Even if git-config syntax does not have direct support for this scenario, we are able to create list of complex object with subsection and [ConfigurationRoot](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.configurationroot?view=dotnet-plat-ext-6.0).

`ConfigurationRoot` supports working with arrays by creating indices for items as "subsections". This allows us to create a section selector that picks values from the array based on the element index. For example, `WatchedProcess:0:Name` selects the value `Name` from the first item in the array. This means we can use indices as subsections and create an array of complex objects as follows:

```gitconfig
[WatchedProcesses "0"] # indicies must be unique
	ApplicationPath = "C:\\MyProcessPath\ABCD"
	Name = NServiceBus.Host

[WatchedProcesses "1"] # indicies must be unique
	ApplicationPath = "C:\\MyProcessPath2\ABCD"
	Name = NServiceBus.Host

[WatchedProcesses "2"] # indicies must be unique
	ApplicationPath = "C:\\MyProcessPath2\ABCD"
	Name = NServiceBus.Host
```

With this configuration we are able to retrieve array of complex objects in following way:
```csharp
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddDotNetConfig();
var configurationRoot = configurationBuilder.Build();
var watchedProcesses = configurationRoot.GetSection(nameof(WatchedProcess)).Get<IList<Json.Appsettings.WatchedProcess>>();
```

> NOTE: be sure that your array items have unique index

## API

There are three main ways to access *.netconfig* values:

* [Native API](#native-api) for direct access to .netconfig values
* [Microsoft.Extensions.Configuration](#microsoftextensionsconfiguration) provider
* [System.CommandLine](#systemcommandline) for CLI apps

### Native API

[![Version](https://img.shields.io/nuget/v/DotNetConfig.svg?color=royalblue)](https://www.nuget.org/packages/DotNetConfig) [![Downloads](https://img.shields.io/nuget/dt/DotNetConfig.svg?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig)

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

[![Version](https://img.shields.io/nuget/v/DotNetConfig.Configuration.svg?color=royalblue)](https://www.nuget.org/packages/DotNetConfig.Configuration) [![Downloads](https://img.shields.io/nuget/dt/DotNetConfig.Configuration.svg?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig.Configuration)

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

[![Version](https://img.shields.io/nuget/v/DotNetConfig.CommandLine.svg?color=royalblue)](https://www.nuget.org/packages/DotNetConfig.CommandLine) [![Downloads](https://img.shields.io/nuget/dt/DotNetConfig.CommandLine.svg?color=darkmagenta)](https://www.nuget.org/packages/DotNetConfig.CommandLine)

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


## CLI

[![Version](https://img.shields.io/nuget/v/dotnet-config.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-config) [![Downloads](https://img.shields.io/nuget/dt/dotnet-config.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-config)

<!-- #cli -->
The command line tool allows you to inspect and modify configuration files used by your dotnet tools. 
Installation is the same as for any other dotnet tool: 

```
> dotnet tool install -g dotnet-config
```

Reading and writing variables don't require any special options. The following lines first 
write a variable value and then retrieve its value:

```
> dotnet config mytool.myvariable myvalue
> dotnet config mytool.myvariable
myvalue
```

The value is returned verbatim via the standard output, so you can assign it directly to a 
variable, for example.

All current options from running `dotnet config -?` are:

```
Usage: dotnet config [options]

Location (uses all locations by default)
      --local                use .netconfig.user file
      --global               use global config file
      --system               use system config file
      --path[=VALUE]         use given config file or directory

Action
      --get                  get value: name [value-regex]
      --get-all              get all values: key [value-regex]
      --get-regexp           get values for regexp: name-regex [value-regex]
      --set                  set value: name value [value-regex]
      --set-all              set all matches: name value [value-regex]
      --add                  add a new variable: name value
      --unset                remove a variable: name [value-regex]
      --unset-all            remove all matches: name [value-regex]
      --remove-section       remove a section: name
      --rename-section       rename section: old-name new-name
  -l, --list                 list all
  -e, --edit                 edit the config file in an editor

Other
      --default[=VALUE]      with --get, use default value when missing entry
      --name-only            show variable names only
      --type[=VALUE]         value is given this type, can be 'boolean', 'datetime' or 'number'
  -?, -h, --help             Display this help
```

Command line parsing is done with [Mono.Options](https://www.nuget.org/packages/mono.options) so 
all the following variants for arguments are supported: `-flag`, `--flag`, `/flag`, `-flag=value`, 
`--flag=value`, `/flag=value`, `-flag:value`, `--flag:value`, `/flag:value`, `-flag value`, 
`--flag value`, `/flag value`.

<!-- #cli -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->