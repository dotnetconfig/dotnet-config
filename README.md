![Icon](https://github.com/dotnetconfig/dotnet-config/raw/master/docs/img/icon-32.png) dotnet-config
============

[![GitHub](https://img.shields.io/badge/-github-181717.svg?logo=GitHub)](https://github.com/dotnetconfig/dotnet-config)
[![Discord Chat](https://img.shields.io/badge/chat-on%20discord-7289DA.svg)](https://discord.gg/x4qhjYd)
[![License](https://img.shields.io/github/license/dotnetconfig/dotnet-config.svg?color=blue)](https://github.com/dotnetconfig/dotnet-config/blob/master/LICENSE)
[![Build Status](https://dev.azure.com/dotnetconfig/dotnetconfig/_apis/build/status/dotnet-config?branchName=master)](https://build.azdo.io/dotnetconfig/dotnetconfig/1)

<p>
<b><a href="#why">Why</a></b>
|
<b><a href="#what">What</a></b>
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

The following are tools that leverage *.netconfig* to provide flexible configuration persistence 
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
    gzip

[vs "alias"]
	comexp = run|community|exp
	preexp = run|preview|exp

[file.github]
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



## API

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


Since `.netconfig` supports multi-valued variables, you can retrieve them all 
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

config.AddString("vs", "alias", "comexp", "run|community|exp", ConfigLevel.Global);
```

You can explore the entire API in the [docs site](https://dotnetconfig.org/api/).


## CLI

[![Version](https://img.shields.io/nuget/v/dotnet-config.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-config)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-config.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-config)

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

