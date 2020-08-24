![Icon](https://github.com/kzu/dotnet-config/raw/master/docs/img/icon-32.png) dotnet-config
============

[![Discord Chat](https://img.shields.io/badge/chat-on%20discord-7289DA.svg)](https://discord.gg/3sEqMMB)
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

By default, configuration files are named `.netconfig` and support three storage levels: 
* Local: current directory and any ancestor directories
* Global: user profile directory, from [System.Environment.SpecialFolder.UserProfile](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=netstandard-2.0#fields).
* System: system-wide directory, from [System.Environment.SpecialFolder.System](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=netstandard-2.0#fields).

The files are read in the order given above, with first value found taking precedence. 
When multiple values are read then all values of a key from all files will be returned.

# How

## Format

Example file:

```
# .netconfig is awesome: https://dotnetconfig.org

[vs "alias"]                              # dotnet-vs global tool aliases
	comexp = run|community|exp
	preexp = run|preview|exp

[file.github]                             # dotnet-file GH repo/file download/sync sections
  # example of multi-valued variables
	url = https://github.com/dotnet/runtime/tree/master/docs/design/features
	url = https://github.com/dotnet/aspnetcore/tree/master/docs

; semi-colon too for comments
; subsections allow grouping variables
[file "docs/design/features/code-versioning.md"]
	url = https://github.com/dotnet/runtime/blob/master/docs/design/features/code-versioning.md
	etag = 74055672d93c79d517c6e5cbad968204100e805a9f87ffa777b617f68f0e4951

[file "docs/APIReviewProcess.md"]
	url = https://github.com/dotnet/aspnetcore/blob/master/docs/APIReviewProcess.md
	etag = 1e4acd7e1ac446f0c6d397e1ed517c54507700b85826f64745559dfb50f2acbd
```

The syntax follows closely the [git-config syntax](https://git-scm.com/docs/git-config#_syntax). 
The `#` and `;` characters begin comments to the end of line, blank lines are ignored.

The file consists of **sections** and **variables**. A section begins with the name of the section in 
square brackets and continues until the next section begins. Section names are case-insensitive. 
Only alphanumeric characters, `-` and `.` are allowed in section names. Each variable must belong 
to some section, which means that there must be a section header before the first setting of a 
variable.

Sections can be further divided into **subsections**. To begin a subsection put its name in double 
quotes, separated by space from the section name, in the section header, like in the example below:

```
	[section "subsection"]
```

Subsection names are case sensitive and can contain any characters except newline. Doublequote `"` 
and backslash `\` can be included by escaping them as `\"` and `\\`, respectively. 

Section headers cannot span multiple lines. Variables may belong directly to a section 
or to a given subsection. You can have `[section]` if you have `[section "subsection"]`, but you 
don't need to.

All the other lines are recognized as setting **variables**, in the form `name = value` (or just `name`, 
which is a short-hand to say that the variable is the boolean `true`). Variable names are case-insensitive, 
allow only alphanumeric characters and `-`, and must start with an alphabetic character.

Leading whitespaces after `name =`, the remainder of the line after the first comment character `#` 
or `;`, and trailing whitespaces of the line are discarded unless they are enclosed in double quotes. 
Internal whitespaces within the value are retained verbatim.

Inside double quotes, double quote `"` and backslash `\` characters must be escaped just like in a 
subsection, with `\"` and `\\`, respectively. 

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


> Note: if this section looks familiar, it's because it follows quite closely the 
> [git config](https://git-scm.com/docs/git-config) documentation itself 😉

## API

[![Version](https://img.shields.io/nuget/v/dotnet-config-lib.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-config-lib)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-config-lib.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-config-lib)

```
PM> Install-Package dotnet-config-lib
```

There is a CI feed in case you are working on a feature branch or a PR:

```
 <add key="kzu" value="https://pkg.kzu.io/index.json" />
```

The main usage for .NET tool authors consuming the [dotnet-config-lib](https://www.nuget.org/packages/dotnet-config-lib) 
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

There is a CI feed in case you are working on a feature branch or a PR:

```
> dotnet tool update -g dotnet-config --no-cache --add-source https://pkg.kzu.io/index.json
```

Current output from `dotnet config -?`:

```
Usage: dotnet config [options]

Location (uses all locations by default)
      --local                aggregate config file in current and ancestor directories
      --global               use only global config file
      --system               use only system config file
  -f, --file[=VALUE]         use only given config file

Action
      --get                  get value: name [value-regex]
      --get-all              get all values: key [value-regex]
      --get-regexp           get values for regexp: name-regex [value-regex]
      --set                  set value: name value [value-regex]
      --set-all              replace all matching variables: name value [value_regex]
      --add                  add a new variable: name value
      --unset                remove a variable: name [value-regex]
      --unset-all            remove all matches: name [value-regex]
      --remove-section       remove a section: name
      --rename-section       rename section: old-name new-name
  -l, --list                 list all
  -e, --edit                 edit the config file in an editor

Other
      --default[=VALUE]      with --get, use default value when missing entry
  -d, --directory[=VALUE]    use given directory for configuration file
      --name-only            show variable names only
      --type[=VALUE]         value is given this type, can be 'boolean', 'datetime' or 'number'
  -?, -h, --help             Display this help
```

Command line parsing is done with [Mono.Options](https://www.nuget.org/packages/mono.options) so 
all the following variants for arguments are supported: `-flag`, `--flag`, `/flag`, `-flag=value`, 
`--flag=value`, `/flag=value`, `-flag:value`, `--flag:value`, `/flag:value`, `-flag value`, 
`--flag value`, `/flag value`.

