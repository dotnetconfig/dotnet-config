using System;
using System.Diagnostics;
using System.Linq;
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
            var localLocation = false;
            string? filePath = default;
            bool nameOnly = false;
            string? defaultValue = default;
            string? type = default;

            var options = new OptionSet
            {
                { Environment.NewLine },
                { Environment.NewLine },
                { "Config file location" },
                { "global", "use global config file", _ => globalLocation = true },
                { "system", "use system config file", _ => systemLocation = true },
                { "local", "use current directory config file", _ => localLocation = true },
                { "f|file:", "use given config file", f => filePath = f },

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

                { "rename-section", "rename section: old-name new-name", _ => action = ConfigAction.List },
                { "remove-section", "remove a section: name", _ => action = ConfigAction.List },

                { "l|list", "list all", _ => action = ConfigAction.List },
                { "e|edit", "open an editor", _ => action = ConfigAction.Edit },

                { Environment.NewLine },
                { "Other" },
                { "default:", "with --get, use default value when missing entry", v => defaultValue = v },
                { "d|directory:", "use given directory for configuration file", v => defaultValue = v },
                { "name-only", "show variable names only", _ => nameOnly = true },
                { "type:", "value is given this type, either 'bool' or 'int'", t => type = t },

                { "?|h|help", "Display this help", h => help = h != null },
            };

            var extraArgs = options.Parse(args);

            if (args.Length == 1 && help)
                return ShowHelp(options);

            Config config;
            if (globalLocation)
                config = Config.Read(ConfigLevel.Global);
            else if (systemLocation)
                config = Config.Read(ConfigLevel.System);
            else if (localLocation)
                config = Config.Read(ConfigLevel.Local);
            else if (filePath != null)
                config = Config.FromFile(filePath);
            else
                config = Config.Build();

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

            switch (action)
            {
                case ConfigAction.Add:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null)
                            return ShowHelp(options);

                        config.Add(section, subsection, variable, extraArgs[1]);
                        break;
                    }
                case ConfigAction.Get:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null)
                            return ShowHelp(options);

                        if (defaultValue == null)
                            Console.WriteLine(config.Get<string>(section, subsection, variable));
                        else
                            Console.WriteLine(config.Get(section, subsection, variable, defaultValue));

                        break;
                    }
                case ConfigAction.GetAll:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null)
                            return ShowHelp(options);

                        string? regex = default;
                        if (extraArgs.Count > 1)
                            regex = extraArgs[1];

                        Action<ConfigEntry> write;
                        if (nameOnly)
                            write = e => Console.WriteLine(e.Key);
                        else
                            write = e => Console.WriteLine($"{e.Key}={(e.Value == null ? "" : e.Value.Contains(' ') ? "\"" + e.Value + "\"" : e.Value)}");

                        foreach (var entry in config.GetAll(section, subsection, variable, regex))
                        {
                            write(entry);
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

                        Action<ConfigEntry> write;
                        if (nameOnly)
                            write = e => Console.WriteLine(e.Key);
                        else
                            write = e => Console.WriteLine($"{e.Key}={(e.Value == null ? "" : e.Value.Contains(' ') ? "\"" + e.Value + "\"" : e.Value)}");

                        foreach (var entry in config.GetRegex(nameRegex, valueRegex))
                        {
                            write(entry);
                        }

                        break;
                    }
                case ConfigAction.Set:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null)
                            return ShowHelp(options);

                        var value = string.Join(' ', extraArgs.Skip(1)).Trim();
                        // Common mistake to do 'config key = value', so just remove it.
                        if (value.StartsWith('='))
                            value = new string(value[1..]).Trim();

                        config.Set(section, subsection, variable, value);
                        break;
                    }
                case ConfigAction.SetAll:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null || extraArgs.Count < 2 || extraArgs.Count > 3)
                            return ShowHelp(options);

                        var value = extraArgs[1];
                        string? regex = default;
                        if (extraArgs.Count > 2)
                            regex = extraArgs[2];

                        config.SetAll(section, subsection, variable, value);
                        break;
                    }
                case ConfigAction.Unset:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null)
                            return ShowHelp(options);

                        config.Unset(section, subsection, variable);
                        break;
                    }
                case ConfigAction.UnsetAll:
                    {
                        var (section, subsection, variable) = ParseKey(extraArgs[0]);
                        if (section == null)
                            return ShowHelp(options);

                        string? regex = default;
                        if (extraArgs.Count > 1)
                            regex = extraArgs[1];

                        config.UnsetAll(section, subsection, variable, regex);
                        break;
                    }
                case ConfigAction.List:
                    {
                        Action<ConfigEntry> write;
                        if (nameOnly)
                            write = e => Console.WriteLine(e.Key);
                        else
                            write = e => Console.WriteLine($"{e.Key}={(e.Value == null ? "" : e.Value.Contains(' ') ? "\"" + e.Value + "\"" : e.Value)}");

                        foreach (var entry in config)
                        {
                            write(entry);
                        }

                        break;
                    }
                case ConfigAction.Edit:
                case ConfigAction.RenameSection:
                case ConfigAction.RemoveSection:
                    Console.WriteLine("Not supported yet.");
                    break;
                default:
                    return ShowHelp(options);
            }

            return 0;
        }

        static (string? section, string? subsection, string name) ParseKey(string arg)
        {
            var parts = arg.Split('.');
            var variable = parts[^1];
            string? section = default;
            string? subsection = default;
            if (parts.Length > 2)
            {
                section = string.Join('.', parts[0..^2]);
                subsection = parts[^2];
            }
            else if (parts.Length == 2)
            {
                section = parts[0];
            }

            return (section, subsection, variable);
        }

        static int ShowHelp(OptionSet options)
        {
            Console.Write($"Usage: dotnet {ThisAssembly.Metadata.AssemblyName} [options]");
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }
    }
}
