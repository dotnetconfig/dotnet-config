using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotNetConfig;

namespace System.CommandLine
{
    /// <summary>
    /// Extension methods to automatically read default values for arguments and 
    /// options from .netconfig.
    /// </summary>
    /// <remarks>
    /// After invoking <see cref="WithConfigurableDefaults"/> on a command, all its 
    /// arguments and options in the entire command tree are processed according to 
    /// these heuristics:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Only arguments/options without a default value are processed
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Section matches root command name, subsection (dot - separated) for each additional nested
    /// command level(i.e. `[mytool "mycommand.myverb"]`)
    /// </description>
    /// </item>
    /// <description>
    /// Compatible arguments/options(same name/type) can be placed in ancestor section/subsection to affect 
    /// default value of entire subtree
    /// </description>
    /// <item>
    /// <description>
    /// All the types supported by System.CommandLine for multiple artity arguments and options are
    /// automatically populated: arrays, `IEnumerable{T}`, `ICollection{T}`, `IList{T}` and `List{T}`: 
    /// .netconfig can provide multi-valued variables for those
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Numbers can be either `int` or `long`.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class CommandLineExtensions
    {
        static readonly HashSet<Type> configurableTypes = new HashSet<Type>(new[]
        {
            typeof(string),
            typeof(long),
            typeof(long?),
            typeof(int),
            typeof(int?),
            typeof(bool),
            typeof(bool?),
            typeof(DateTime),
            typeof(DateTime?),
        });

        /// <summary>
        /// Register default value factories for all arguments and options in the command tree 
        /// which don't have a default value already.
        /// </summary>
        /// <typeparam name="T">Type of command, inferred from usage.</typeparam>
        /// <param name="command">The command to set default values for.</param>
        /// <param name="section">Optional root section for this command arguments and its tree. If not provided, the <paramref name="command"/>'s <c>Name</c>
        /// property will be used. For a <see cref="RootCommand"/>, this is the app's assembly name.</param>
        /// <param name="configuration">Optional pre-built configuration. If not provided, <see cref="Config.Build(string?)"/> will 
        /// be used to build a default configuration.</param>
        /// <returns>The command where defaults have been applied.</returns>
        public static T WithConfigurableDefaults<T>(this T command, string? section = default, Config? configuration = default) where T : Command
        {
            configuration ??= Config.Build();
            section ??= command.Name;

            var arguments = new HashSet<IArgument>();
            CollectArguments(command, arguments);

            var sections = new ConcurrentDictionary<(string? subsection, string variable), HashSet<IArgument>>();
            PopulateSections(command, sections, arguments, null);

            // Clear sections with multiple variables but with incompatible types
            foreach (var entry in sections.Where(x => x.Value.GroupBy(arg => arg.ValueType).Skip(1).Any()).ToArray())
                sections.TryRemove(entry.Key, out _);

            // Remaining sections are all the final arguments for defaulting.
            var factories = new ConcurrentDictionary<IArgument, DefaultValueFactory>();
            foreach (var sharedSection in sections)
            {
                foreach (var argument in sharedSection.Value)
                {
                    factories.GetOrAdd(argument, arg => new DefaultValueFactory(arg, configuration, section))
                        .AddSubsection(sharedSection.Key.subsection);
                }
            }

            foreach (var factory in factories.Values)
                factory.SetDefaultValue();

            return command;
        }

        static void PopulateSections(Command command,
            ConcurrentDictionary<(string? subsection, string variable), HashSet<IArgument>> sections,
            HashSet<IArgument> arguments,
            string? subsection = null)
        {
            void Add(IEnumerable<IArgument> args)
            {
                foreach (var arg in args)
                {
                    if (subsection != null)
                    {
                        var parts = subsection.Split('.');
                        for (var i = 1; i <= parts.Length; i++)
                            sections.GetOrAdd((string.Join(".", parts[0..i]), arg.Name), _ => new()).Add(arg);

                        sections.GetOrAdd((null, arg.Name), _ => new()).Add(arg);
                    }
                    else
                    {
                        sections.GetOrAdd((subsection, arg.Name), _ => new()).Add(arg);
                    }
                }
            }

            Add(command.Arguments.Where(arg => arguments.Contains(arg)));
            Add(command.Options.OfType<IOption>().Select(x => x.Argument).Where(x => arguments.Contains(x)));

            foreach (var child in command.OfType<Command>())
                PopulateSections(child, sections, arguments, subsection == null ? child.Name : subsection + "." + child.Name);
        }

        static void CollectArguments(
            Command command,
            HashSet<IArgument> arguments)
        {
            foreach (var symbol in command)
            {
                if (symbol is Command nested)
                {
                    CollectArguments(nested, arguments);
                }
                else
                {
                    var arg = symbol as IArgument ?? (symbol as IOption)?.Argument;
                    if (arg == null || arg.HasDefaultValue)
                        continue;

                    // This check must go first since typeof(string) is also enumerable.
                    if (configurableTypes.Contains(arg.ValueType))
                    {
                        arguments.Add(arg);
                    }
                    else if (arg.Arity.MaximumNumberOfValues > 1)
                    {
                        if (arg.ValueType.IsArray && configurableTypes.Contains(arg.ValueType.GetElementType()))
                            arguments.Add(arg);
                        else if (arg.ValueType.IsGenericType &&
                            IsCompatibleGenericType(arg.ValueType) &&
                            configurableTypes.Contains(arg.ValueType.GetGenericArguments()[0]))
                            arguments.Add(arg);
                    }
                }

                // These are the supported conversions in System.CommandLine, see 
                // https://github.com/dotnet/command-line-api/blob/main/src/System.CommandLine.Tests/Binding/TypeConversionTests.cs#L566-L587
                static bool IsCompatibleGenericType(Type type) =>
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                    type.GetGenericTypeDefinition() == typeof(IList<>) ||
                    type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    type.GetGenericTypeDefinition() == typeof(List<>);
            }
        }

        class DefaultValueFactory
        {
            readonly Argument argument;
            readonly Config config;
            readonly string section;
            readonly HashSet<string?> subsections = new();
            string?[] orderedSubsections = Array.Empty<string?>();

            public DefaultValueFactory(IArgument argument, Config config, string section)
                => (this.argument, this.config, this.section)
                // NOTE: this is the only downcast to Argument, since SetDefaultValueFactory is not 
                // present at the interface level.
                = (argument as Argument ?? throw new ArgumentException(), config, section);

            public void AddSubsection(string? subsection) => subsections.Add(subsection);

            public void SetDefaultValue()
            {
                orderedSubsections = subsections.OrderByDescending(x => x?.Length ?? 0).ToArray();

                if (argument.Arity.MaximumNumberOfValues > 1)
                {
                    // We have already validated that it's either an array or IEnumerable<T>
                    var elementType = argument.ArgumentType.IsArray ?
                        argument.ArgumentType.GetElementType() :
                        argument.ArgumentType.GetGenericArguments()[0];

                    switch (elementType)
                    {
                        case Type type when type.IsAssignableFrom(typeof(string)):
                            SetDefaultStrings();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(long)):
                            SetDefaultNumbers();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(int)):
                            SetDefaultIntegers();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(bool)):
                            SetDefaultBooleans();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(DateTime)):
                            SetDefaultDateTimes();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (argument.ArgumentType)
                    {
                        case Type type when type.IsAssignableFrom(typeof(string)):
                            SetDefaultString();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(long)) || type.IsAssignableFrom(typeof(int)):
                            SetDefaultNumber();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(bool)):
                            SetDefaultBoolean();
                            break;
                        case Type type when type.IsAssignableFrom(typeof(DateTime)):
                            SetDefaultDateTime();
                            break;
                        default:
                            break;
                    }
                }
            }

            void SetDefaultStrings()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    var values = new List<string>();
                    foreach (var subsection in orderedSubsections)
                        values.AddRange(config.GetAll(section, subsection, argument.Name).Select(x => x.GetString()));

                    if (argument.ArgumentType.IsArray)
                        return values.ToArray();

                    return values;
                });
            }

            void SetDefaultNumbers()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    var values = new List<long>();
                    foreach (var subsection in orderedSubsections)
                        values.AddRange(config.GetAll(section, subsection, argument.Name).Select(x => x.GetNumber()));

                    if (argument.ArgumentType.IsArray)
                        return values.ToArray();

                    return values;
                });
            }

            void SetDefaultIntegers()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    var values = new List<int>();
                    foreach (var subsection in orderedSubsections)
                        values.AddRange(config.GetAll(section, subsection, argument.Name).Select(x => (int)x.GetNumber()));

                    if (argument.ArgumentType.IsArray)
                        return values.ToArray();

                    return values;
                });
            }

            void SetDefaultBooleans()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    var values = new List<bool>();
                    foreach (var subsection in orderedSubsections)
                        values.AddRange(config.GetAll(section, subsection, argument.Name).Select(x => x.GetBoolean()));

                    if (argument.ArgumentType.IsArray)
                        return values.ToArray();

                    return values;
                });
            }

            void SetDefaultDateTimes()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    var values = new List<DateTime>();
                    foreach (var subsection in orderedSubsections)
                        values.AddRange(config.GetAll(section, subsection, argument.Name).Select(x => x.GetDateTime()));

                    if (argument.ArgumentType.IsArray)
                        return values.ToArray();

                    return values;
                });
            }

            void SetDefaultString()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    foreach (var subsection in orderedSubsections)
                    {
                        if (config.TryGetString(section, subsection, argument.Name, out var value))
                            return value;
                    }
                    return default;
                });
            }

            void SetDefaultNumber()
            {
                if (argument.ArgumentType == typeof(int))
                {
                    argument.SetDefaultValueFactory(() =>
                    {
                        foreach (var subsection in orderedSubsections)
                        {
                            if (config.TryGetNumber(section, subsection, argument.Name, out var value))
                                return (int)value;
                        }
                        return default;
                    });
                }
                else
                {
                    argument.SetDefaultValueFactory(() =>
                    {
                        foreach (var subsection in orderedSubsections)
                        {
                            if (config.TryGetNumber(section, subsection, argument.Name, out var value))
                                return value;
                        }
                        return default;
                    });
                }
            }

            void SetDefaultBoolean()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    foreach (var subsection in orderedSubsections)
                    {
                        if (config.TryGetBoolean(section, subsection, argument.Name, out var value))
                            return value;
                    }
                    return default;
                });
            }

            void SetDefaultDateTime()
            {
                argument.SetDefaultValueFactory(() =>
                {
                    foreach (var subsection in orderedSubsections)
                    {
                        if (config.TryGetDateTime(section, subsection, argument.Name, out var value))
                            return value;
                    }
                    return default;
                });
            }
        }
    }
}
