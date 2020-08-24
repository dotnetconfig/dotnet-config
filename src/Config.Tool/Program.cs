using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Mono.Options;

namespace Microsoft.DotNet
{
    class Program
    {
        static int Main(string[] args)
        {
            var help = false;

            var action = ConfigAction.None;
            var systemLocation = false;
            var globalLocation = false;
            string? path = Directory.GetCurrentDirectory();
            bool nameOnly = false;
            string? defaultValue = default;
            string? type = default;
            bool debug = false;

            var options = new OptionSet
            {
                { Environment.NewLine },
                { Environment.NewLine },
                { "Location (uses all locations by default)" },
                { "global", "use global config file", _ => globalLocation = true },
                { "system", "use system config file", _ => systemLocation = true },
                { "path:", "use given config file or directory", f => path = f },

                { Environment.NewLine },
                { "Action" },
                { "get", "get value: name [value-regex]", _ => action = ConfigAction.Get },
                { "get-all", "get all values: key [value-regex]", _ => action = ConfigAction.GetAll },
                { "get-regexp", "get values for regexp: name-regex [value-regex]", _ => action = ConfigAction.GetRegexp },
                { "set", "set value: name value [value-regex]", _ => action = ConfigAction.Set },
                { "set-all", "replace all matching variables: name value [value_regex]", _ => action = ConfigAction.SetAll },
                { "replace-all", "replace all matching variables: name value [value_regex]", _ => action = ConfigAction.SetAll, true },

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

            Config config;
            if (globalLocation)
                config = Config.Build(ConfigLevel.Global);
            else if (systemLocation)
                config = Config.Build(ConfigLevel.System);
            else 
                config = Config.Build(path);

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
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);

                        if (extraArgs.Count != 2)
                            return ShowHelp(options);

                        if ((kind == ValueKind.Boolean && !string.IsNullOrEmpty(extraArgs[1]) && !ConfigParser.TryParseBoolean(extraArgs[1], out error)) || 
                            (kind == ValueKind.Number && !ConfigParser.TryParseNumber(extraArgs[1], out error)) || 
                            kind == ValueKind.DateTime && !DateTime.TryParse(extraArgs[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                        {
                            if (error != null)
                                return ShowError(error);

                            Console.Error.WriteLine($"unexpected datetime value `{extraArgs[1]}`, expected ISO 8601 (aka round-trip) format.");
                            return -1;
                        }

                        config.AddString(section!, subsection, variable!, extraArgs[1]);
                        break;
                    }
                case ConfigAction.Get:
                    {
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);

                        var value = config.GetString(section!, subsection, variable!);
                        Console.WriteLine(value ?? defaultValue ?? "");
                        break;
                    }
                case ConfigAction.GetAll:
                    {
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);

                        var matcher = ValueMatcher.All;
                        if (extraArgs.Count > 1)
                            matcher = ValueMatcher.From(extraArgs[1]);

                        foreach (var entry in config.GetAll(section!, subsection, variable!, matcher))
                        {
                            entryWriter(entry);
                        }
                        break;
                    }
                case ConfigAction.GetRegexp:
                    {
                        if (extraArgs.Count == 0 || extraArgs.Count > 2)
                            return ShowHelp(options);

                        string nameRegex = extraArgs[0];
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
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);

                        var value = string.Join(' ', extraArgs.Skip(1)).Trim();
                        // It's a common mistake to do 'config key = value', so just remove it.
                        if (value.StartsWith('='))
                            value = new string(value[1..]).Trim();

                        if ((kind == ValueKind.Boolean && !string.IsNullOrEmpty(value) && !ConfigParser.TryParseBoolean(value, out error)) ||
                            (kind == ValueKind.Number && !ConfigParser.TryParseNumber(value, out error)) ||
                            kind == ValueKind.DateTime && !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                        {
                            if (error != null)
                                return ShowError(error);

                            Console.Error.WriteLine($"unexpected datetime value `{value}`, expected ISO 8601 (aka round-trip) format.");
                            return -1;
                        }

                        config.SetString(section!, subsection, variable!, value);
                        break;
                    }
                case ConfigAction.SetAll:
                    {
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);

                        if (extraArgs.Count < 2 || extraArgs.Count > 3)
                            return ShowHelp(options);

                        var value = extraArgs[1];
                        var matcher = ValueMatcher.All;
                        if (extraArgs.Count > 2)
                            matcher = ValueMatcher.From(extraArgs[2]);

                        if ((kind == ValueKind.Boolean && !string.IsNullOrEmpty(value) && !ConfigParser.TryParseBoolean(value, out error)) ||
                            (kind == ValueKind.Number && !ConfigParser.TryParseNumber(value, out error)) ||
                            kind == ValueKind.DateTime && !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                        {
                            if (error != null)
                                return ShowError(error);

                            Console.Error.WriteLine($"unexpected datetime value `{value}`, expected ISO 8601 (aka round-trip) format.");
                            return -1;
                        }

                        config.SetAllString(section!, subsection, variable!, value, matcher);
                        break;
                    }
                case ConfigAction.Unset:
                    {
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);


                        config.Unset(section!, subsection, variable!);
                        break;
                    }
                case ConfigAction.UnsetAll:
                    {
                        if (!ConfigParser.TryParseKey(extraArgs[0], out var section, out var subsection, out var variable, out error))
                            return ShowError(error);

                        var matcher = ValueMatcher.All;
                        if (extraArgs.Count > 1)
                            matcher = ValueMatcher.From(extraArgs[1]);

                        config.UnsetAll(section!, subsection, variable!, matcher);
                        break;
                    }
                case ConfigAction.List:
                    foreach (var entry in config)
                    {
                        entryWriter(entry);
                    }
                    break;
                case ConfigAction.Edit:
                    if (!Config.Build(path).TryGetString("core", null, "editor", out var editor))
                    {
                        var cmd = Environment.OSVersion.Platform == PlatformID.Unix ? "which" : "where";
                        editor = Process.Start(new ProcessStartInfo(cmd, "code") { RedirectStandardOutput = true }).StandardOutput.ReadLine() ?? "";
                    }

                    if (!string.IsNullOrEmpty(editor))
                        Process.Start(editor, config.FilePath);
                    else
                        Process.Start(new ProcessStartInfo(config.FilePath) { UseShellExecute = true });

                    break;
                case ConfigAction.RemoveSection:
                    if (extraArgs.Count != 1)
                        return ShowHelp(options);

                    config.RemoveSection(extraArgs[0]);
                    break;
                case ConfigAction.RenameSection:
                    if (extraArgs.Count != 2)
                        return ShowHelp(options);

                    if (!ConfigParser.TryParseSection(extraArgs[0], out var oldSection, out var oldSubsection, out error))
                        return ShowError(error);

                    if (!ConfigParser.TryParseSection(extraArgs[1], out var newSection, out var newSubsection, out error))
                        return ShowError(error);

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

            var colon = error.IndexOf(':');
            if (colon != -1)
                Console.Error.WriteLine(error[++colon..].Trim());
            else
                Console.Error.WriteLine(error.Trim());

            return -1;
        }

        static int ShowHelp(OptionSet options)
        {
#pragma warning disable CS0436 // Type conflicts with imported type
            Console.Write($"Usage: dotnet {ThisAssembly.Metadata.AssemblyName} [options]");
#pragma warning restore CS0436 // Type conflicts with imported type
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }
    }
}
