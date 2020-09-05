# dotnet-config API

The main usage for .NET tool authors consuming the [DotNetConfig](https://nuget.org/packages/DotNetConfig) 
API is to first build a configuration from a specific path (will assume current 
directory if omitted):

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