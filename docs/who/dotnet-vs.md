# dotnet-vs

[![dotnet-vs](https://img.shields.io/nuget/v/dotnet-vs.svg?color=royalblue&label=dotnet-vs)](https://nuget.org/packages/dotnet-vs)

The [dotnet-vs](https://github.com/kzu/dotnet-vs) tool uses `dotnet-config` to persist command aliases, 
just like GIT aliases, that run `Visual Studio` (or its installer) with various switches.

Any of the commands supported by the `vs` global tool can be saved as an alias by simply appending 
`--save [alias]` at the end of the command line arguments. Next time you need to execute the same 
command, you can just use `vs [alias]` instead.

Example:

* Run Visual Studio Community edition's experimental instance, with activity logging enabled, save as `test`

    vs run com exp /log --save=test

  From that point on, the same command can be run simply with:

    vs test


Sample configuration:

```gitconfig
[vs "alias"]
	comexp = run|community|exp
	preexp = run|preview|exp
	test = run|com|exp|/log
```

