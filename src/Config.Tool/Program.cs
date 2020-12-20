using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.Options;

namespace DotNetConfig
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception e)
            {
                return ShowError(e.Message);
            }
        }

        static int Run(string[] args)
        {
            var help = false;

            var action = ConfigAction.None;
            var useSystem = false;
            var useGlobal = false;
            var useLocal = false;
            var path = Directory.GetCurrentDirectory();
            var nameOnly = false;
            string? defaultValue = default;
            string? type = default;
            var debug = false;

            var options = new OptionSet
            {
                { Environment.NewLine },
                { Environment.NewLine },
                { "Location (uses all locations by default)" },
                { "local", "use .netconfig.user file", _ => useLocal = true },
                { "global", "use global config file", _ => useGlobal = true },
                { "system", "use system config file", _ => useSystem = true },
                { "path:", "use given config file or directory", f => path = f },

                { Environment.NewLine },
                { "Action" },
                { "get", "get value: name [value-regex]", _ => action = ConfigAction.Get },
                { "get-all", "get all values: key [value-regex]", _ => action = ConfigAction.GetAll },
                { "get-regexp", "get values for regexp: name-regex [value-regex]", _ => action = ConfigAction.GetRegexp },
                { "set", "set value: name value [value-regex]", _ => action = ConfigAction.Set },
                { "set-all", "set all matches: name value [value-regex]", _ => action = ConfigAction.SetAll },
                { "replace-all", "replace all matches: name value [value-regex]", _ => action = ConfigAction.SetAll, true },

                //{ "get-urlmatch", "get value specific for the URL: section[.var] URL", _ => action = ConfigAction.Get },
                { "add", "add a new variable: name value", _ => action = ConfigAction.Add },
                { "unset", "remove a variable: name [value-regex]", _ => action = ConfigAction.Unset },
                { "unset-all", "remove all matches: name [value-regex]", _ => action = ConfigAction.UnsetAll },

                { "remove-section", "remove a section: name", _ => action = ConfigAction.RemoveSection },
                { "rename-section", "rename section: old-name new-name", _ => action = ConfigAction.RenameSection },

                { "l|list", "list all", _ => action = ConfigAction.List },
                { "e|edit", "edit the config file in an editor", _ => action = ConfigAction.Edit },

                { Environment.NewLine },
                { "Other" },
                { "default:", "with --get, use default value when missing entry", v => defaultValue = v },
                { "name-only", "show variable names only", _ => nameOnly = true },
                { "type:", "value is given this type, can be 'boolean', 'datetime' or 'number'", t => type = t },
                { "debug", "add some extra logging for troubleshooting purposes", _ => debug = true, true },

                { "?|h|help", "Display this help", h => help = h != null },
            };

            var extraArgs = options.Parse(args);

            if (debug)
            {
                Console.WriteLine($"::debug::args[{args.Length}]:: {string.Join(" ", args.Select(x => x.IndexOf(' ') != -1 ? $"\"{x}\"" : x))}");
                Console.WriteLine($"::debug::extraargs[{extraArgs.Count}]:: {string.Join(" ", extraArgs.Select(x => x.IndexOf(' ') != -1 ? $"\"{x}\"" : x))}");
            }

            if (args.Length == 1 && help)
                return ShowHelp(options);

            // Can only use one location
            if ((useGlobal && (useSystem || useLocal)) ||
                (useSystem && (useGlobal || useLocal)) ||
                (useLocal && (useGlobal || useSystem)))
            {
                return ShowError("Can only specify one config location.");
            }

            ConfigLevel? level = null;
            Config config;
            if (useGlobal)
            {
                config = Config.Build(ConfigLevel.Global);
                level = ConfigLevel.Global;
            }
            else if (useSystem)
            {
                config = Config.Build(ConfigLevel.System);
                level = ConfigLevel.System;
            }
            else
            {
                config = Config.Build(path);
                if (useLocal)
                    level = ConfigLevel.Local;
            }

            // Can be a get or a set, depending on whether a value is provided.
            if (action == ConfigAction.None)
            {
                if (extraArgs.Count == 1)
                    action = ConfigAction.Get;
                else if (extraArgs.Count > 1)
                    action = ConfigAction.Set;
                else
                    return ShowHelp(options);
            }

            // TODO: For any action that isn't a simple get (which may rely on the output being just the 
            // value retrieved), render to the output window the warnings about invalid entries

            Action<ConfigEntry> entryWriter;
            if (nameOnly)
                entryWriter = e => Console.WriteLine(e.Key);
            else
                entryWriter = e => Console.WriteLine($"{e.Key}={(e.RawValue == null ? "" : e.RawValue.Contains(' ') ? "\"" + e.RawValue + "\"" : e.RawValue)}");

            if (type == "date")
                type = "datetime";
            if (type == "bool")
                type = "boolean";

            var kind = ValueKind.String;
            if (type != null && !Enum.TryParse(type, true, out kind))
            {
                Console.Error.WriteLine($"Error: invalid type '{type}'. Expected one of: 'boolean', 'bool', 'datetime', 'date' or 'number'.");
                return -1;
            }

            string? error = default;

            switch (action)
            {
                case ConfigAction.Add:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);
                        if (extraArgs.Count != 2)
                            return ShowHelp(options);

                        if ((kind == ValueKind.Boolean && !string.IsNullOrEmpty(extraArgs[1]) && !TextRules.TryValidateBoolean(extraArgs[1], out error)) ||
                            (kind == ValueKind.Number && !TextRules.TryValidateNumber(extraArgs[1], out error)) ||
                            kind == ValueKind.DateTime && !DateTime.TryParse(extraArgs[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                        {
                            if (error != null)
                                return ShowError(error);

                            return ShowError($"Unexpected datetime value `{extraArgs[1]}`, expected ISO 8601 (aka round-trip) format.");
                        }

                        if (level != null)
                            config.AddString(section!, subsection, variable!, extraArgs[1], level.Value);
                        else
                            config.AddString(section!, subsection, variable!, extraArgs[1]);

                        break;
                    }
                case ConfigAction.Get:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);

                        var value = config.GetString(section!, subsection, variable!);
                        Console.WriteLine(value ?? defaultValue ?? "");
                        break;
                    }
                case ConfigAction.GetAll:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);

                        string? valueRegex = default;
                        if (extraArgs.Count > 1)
                            valueRegex = extraArgs[1];

                        foreach (var entry in config.GetAll(section!, subsection, variable!, valueRegex))
                        {
                            entryWriter(entry);
                        }
                        break;
                    }
                case ConfigAction.GetRegexp:
                    {
                        if (extraArgs.Count == 0 || extraArgs.Count > 2)
                            return ShowHelp(options);

                        var nameRegex = extraArgs[0];
                        string? valueRegex = default;
                        if (extraArgs.Count > 1)
                            valueRegex = extraArgs[1];

                        foreach (var entry in config.GetRegex(nameRegex, valueRegex))
                        {
                            entryWriter(entry);
                        }
                        break;
                    }
                case ConfigAction.Set:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);

                        var value = string.Join(' ', extraArgs.Skip(1)).Trim();
                        // It's a common mistake to do 'config key = value', so just remove it.
                        if (value.StartsWith('='))
                            value = new string(value[1..]).Trim();

                        if ((kind == ValueKind.Boolean && !string.IsNullOrEmpty(value) && !TextRules.TryValidateBoolean(value, out error)) ||
                            (kind == ValueKind.Number && !TextRules.TryValidateNumber(value, out error)) ||
                            kind == ValueKind.DateTime && !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                        {
                            if (error != null)
                                return ShowError(error);

                            Console.Error.WriteLine($"unexpected datetime value `{value}`, expected ISO 8601 (aka round-trip) format.");
                            return -1;
                        }

                        if (level != null)
                            config.SetString(section!, subsection, variable!, value, level.Value);
                        else
                            config.SetString(section!, subsection, variable!, value);

                        break;
                    }
                case ConfigAction.SetAll:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);

                        if (extraArgs.Count < 2 || extraArgs.Count > 3)
                            return ShowHelp(options);

                        var value = extraArgs[1];
                        string? valueRegex = default;
                        if (extraArgs.Count > 2)
                            valueRegex = extraArgs[2];

                        if ((kind == ValueKind.Boolean && !string.IsNullOrEmpty(value) && !TextRules.TryValidateBoolean(value, out error)) ||
                            (kind == ValueKind.Number && !TextRules.TryValidateNumber(value, out error)) ||
                            kind == ValueKind.DateTime && !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                        {
                            if (error != null)
                                return ShowError(error);

                            Console.Error.WriteLine($"unexpected datetime value `{value}`, expected ISO 8601 (aka round-trip) format.");
                            return -1;
                        }

                        if (level != null)
                            config.SetAllString(section!, subsection, variable!, value, valueRegex, level.Value);
                        else
                            config.SetAllString(section!, subsection, variable!, value, valueRegex);

                        break;
                    }
                case ConfigAction.Unset:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);
                        if (level != null)
                            config.Unset(section!, subsection, variable!, level.Value);
                        else
                            config.Unset(section!, subsection, variable!);

                        break;
                    }
                case ConfigAction.UnsetAll:
                    {
                        TextRules.ParseKey(extraArgs[0], out var section, out var subsection, out var variable);

                        string? valueRegex = default;
                        if (extraArgs.Count > 1)
                            valueRegex = extraArgs[1];

                        if (level != null)
                            config.UnsetAll(section!, subsection, variable!, valueRegex, level.Value);
                        else
                            config.UnsetAll(section!, subsection, variable!, valueRegex);

                        break;
                    }
                case ConfigAction.List:
                    foreach (var entry in config)
                    {
                        entryWriter(entry);
                    }
                    break;
                case ConfigAction.Edit:
                    if (!Config.Build(path).TryGetString("config", null, "editor", out var editor))
                    {
                        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                        var cmd = isWindows ? "where" : "which";
                        var psi = new ProcessStartInfo(cmd)
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        };
                        var extensions = isWindows && Environment.GetEnvironmentVariable("PATHEXT") is { } v
                                       ? v.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                                       : Array.Empty<string>();
                        foreach (var extension in extensions.DefaultIfEmpty(""))
                            psi.ArgumentList.Add("code" + extension);
                        using var process = Process.Start(psi)!;
                        editor = process.StandardOutput.ReadLine() ?? "";
                    }

                    var configFile = config.FilePath;
                    if (config is AggregateConfig aggregate &&
                        aggregate.Files.FirstOrDefault(x => x.Level == level) is Config levelConfig)
                        configFile = levelConfig.FilePath;

                    if (!string.IsNullOrEmpty(editor))
                        Process.Start(editor, configFile);
                    else
                        Process.Start(new ProcessStartInfo(configFile) { UseShellExecute = true });

                    break;
                case ConfigAction.RemoveSection:
                    if (extraArgs.Count != 1)
                        return ShowHelp(options);

                    if (level != null)
                        config.RemoveSection(extraArgs[0], level.Value);
                    else
                        config.RemoveSection(extraArgs[0]);

                    break;
                case ConfigAction.RenameSection:
                    if (extraArgs.Count != 2)
                        return ShowHelp(options);

                    TextRules.ParseSection(extraArgs[0], out var oldSection, out var oldSubsection);
                    TextRules.ParseSection(extraArgs[1], out var newSection, out var newSubsection);

                    if (level != null)
                        config.RenameSection(oldSection!, oldSubsection, newSection!, newSubsection, level.Value);
                    else
                        config.RenameSection(oldSection!, oldSubsection, newSection!, newSubsection);

                    break;
                default:
                    return ShowHelp(options);
            }

            return 0;
        }

        static int ShowError(string? error)
        {
            if (error == null)
                return -1;

            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                var colon = error.IndexOf(':');
                if (colon != -1)
                    Console.Error.WriteLine(error[++colon..].Trim());
                else
                    Console.Error.WriteLine(error.Trim());
            }
            finally
            {
                Console.ResetColor();
            }

            return -1;
        }

        static int ShowHelp(OptionSet options)
        {
            Console.Write($"Usage: dotnet config [options]");
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }
    }
}
