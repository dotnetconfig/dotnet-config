![Icon](https://github.com/kzu/dotnet-config/raw/master/docs/img/icon-32.png) dotnet-config
============

A global tool and accompanying API for managing hierarchical configurations for dotnet tools, 
using (mostly) [git config](https://git-scm.com/docs/git-config) format.

[![Build Status](https://dev.azure.com/dotnetconfig/dotnetconfig/_apis/build/status/dotnet-config?branchName=master)](https://build.azdo.io/dotnetconfig/dotnetconfig/1)
[![License](https://img.shields.io/github/license/dotnetconfig/dotnet-config.svg?color=blue)](https://github.com/dotnetconfig/dotnet-config/blob/master/LICENSE)

dotnet-config | dotnet-config-lib
:------------: | :------------:
[![Version](https://img.shields.io/nuget/v/dotnet-config.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-config)|[![Version](https://img.shields.io/nuget/v/dotnet-config-lib.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-config-lib)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-config.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-config)|[![Downloads](https://img.shields.io/nuget/dt/dotnet-config-lib.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-config-lib)

Installing or updating (same command can be used for both):

```
dotnet tool update -g dotnet-config
```

To get the CI version:

```
dotnet tool update -g dotnet-config --no-cache --add-source https://pkg.kzu.io/index.json
```

## Overview

[.NET Core tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) may need 
to provide configuration options for users to customize their behavior. There is no built-in 
configuration mechanism for them, however, so this project aims to provide a uniform way 
to manage settings for all tools.

`dotnet-config` provides the following:
* A well-documented file format than can be hand-edited in any text editor.
* A dotnet global tool to manage the configuration files (much like `git config`).
* An API for dotnet tool authors to read/write settings.

By default, configuration files are named `.netconfig` and support three storage levels: 
* Local: current directory and any ancestor directories
* Global: user profile directory, from `System.Environment.SpecialFolder.UserProfile`.
* System: system-wide directory, from `System.Environment.SpecialFolder.System`.

The files are read in the order given above, with first value found taking precedence. 
When multiple values are taken then all values of a key from all files will be used.


## Format

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

* *integer*

	The value for many variables that specify various sizes can be suffixed with `k`, `M`, `G` or `T` 
	to mean	"scale the number by 1024", "by 1024x1024", "by 1024x1024x1024" or "by 1024x1024x1024x1024"
	respectively. The suffix is case insensitive, and can also include the `b`, as in `kb` or `MB`. 
    If the result of scaling the value exceeds the size of an integer, it can be read as a `long` (int64).

### CLI

Command line parsing is done with [Mono.Options](https://www.nuget.org/packages/mono.options) so 
all the following variants for arguments are supported: `-flag`, `--flag`, `/flag`, `-flag=value`, `--flag=value`, 
`/flag=value`, `-flag:value`, `--flag:value`, `/flag:value`, `-flag value`, `--flag value`, `/flag value`.

Current output from `dotnet config -?`:

```
Usage: dotnet config [options]

Config file location
      --global               use global config file
      --system               use system config file
      --local                use current directory config file
  -f, --file[=VALUE]         use given config file

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
  -e, --edit                 open an editor

Other
      --default[=VALUE]      with --get, use default value when missing entry
  -d, --directory[=VALUE]    use given directory for configuration file
      --name-only            show variable names only
      --type[=VALUE]         value is given this type, either 'bool' or 'int'
  -?, -h, --help             Display this help
```
