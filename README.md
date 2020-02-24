![Icon](https://raw.github.com/kzu/dotnet-config/master/docs/img/icon-32.png) dotnet-config
============

A global tool and accompanying API for managing hierarchical configurations for dotnet tools, 
using (mostly) [git config](https://git-scm.com/docs/git-config) format.

[![Build Status](https://dev.azure.com/kzu/oss/_apis/build/status/dotnet-config?branchName=master)](https://dev.azure.com/kzu/oss/_build/latest?definitionId=33&branchName=master)

## Overview

[.NET Core tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) may need 
to provide configuration options for users to customize their behavior. There is no built-in 
configuration mechanism for them, however, so this project aims to provide a uniform way 
to manage settings for all tools.

`dotnet-config` provides the following:
* A well-documented file format than can be hand-edited in any text editor.
* A dotnet global tool to manage the configuration files (much like `git config`).
* An API for dotnet tool authors to manage settings programmatically

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

	The value for many variables that specify various sizes can be suffixed with `k``, `M`, `G` or `T` 
	to mean	"scale the number by 1024", "by 1024x1024", "by 1024x1024x1024" or "by 1024x1024x1024x1024"
	respectively. The suffix is case insensitive, and can also include the `b`, as in `kb` or `MB`. 
    If the result of scaling the value exceeds the size of an integer, it can be read as a `long` (int64).

