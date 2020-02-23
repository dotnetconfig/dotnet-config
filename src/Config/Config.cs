using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using Superpower;
using Superpower.Parsers;

/// <summary>
/// Provides access to .netconfig configuration options.
/// </summary>
internal abstract class Config
{
    /// <summary>
    /// Default filename, equal to '.netconfig'.
    /// </summary>
    public const string FileName = ".netconfig";

    /// <summary>
    /// Default global location, equal to <see cref="Environment.SpecialFolder.UserProfile"/> plus 
    /// <see cref="DefaultFileName"/>.
    /// </summary>
    public static string GlobalLocation { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), FileName);

    /// <summary>
    /// Default system location, equal to <see cref="Environment.SpecialFolder.System"/> plus 
    /// <see cref="DefaultFileName"/>.
    /// </summary>
    public static string SystemLocation { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), FileName);

    internal static TextParser<(string section, string subsection)> SectionParser { get; } =
        from section in Character.EqualTo('[').IgnoreThen(Identifier.CStyle.AtLeastOnceDelimitedBy(Character.EqualTo('.')))
        from name in Character.WhiteSpace.IgnoreThen(QuotedString.CStyle).OptionalOrDefault()
        from _ in Character.EqualTo(']')
        select (string.Join(".", section.Select(x => x.ToStringValue())), name);

    internal static TextParser<(string name, string value)> VariableParser { get; } =
        from name in Character.WhiteSpace.IgnoreMany().IgnoreThen(Identifier.CStyle)
        from value in
                Character.WhiteSpace.IgnoreMany().IgnoreThen(
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                // Couldn't figure out how to fix the proper nullability constraints here
                Character.EqualTo('=').IgnoreThen(
                Character.WhiteSpace.IgnoreMany().IgnoreThen(
                QuotedString.CStyle.Or(
                    Character.ExceptIn('#', ';', ' ').Many().Select(x => new string(x))))).OptionalOrDefault())
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        select (name.ToStringValue(), value);

    internal static Dictionary<Type, Delegate> Converters { get; } = new Dictionary<Type, Delegate>
    {
        { typeof(bool), (Func<string, bool>)ConvertBoolean },
        { typeof(DateTime), new Func<string, DateTime>((value) => DateTime.Parse(value ?? throw new ArgumentNullException(nameof(value)), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)) },
        // Integral types, see https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
        { typeof(sbyte), new Func<string, sbyte>((value) => sbyte.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(byte), new Func<string, byte>((value) => byte.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(short), new Func<string, short>((value) => short.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(ushort), new Func<string, ushort>((value) => ushort.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(int), new Func<string, int>((value) => int.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(uint), new Func<string, uint>((value) => uint.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(long), new Func<string, long>((value) => long.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(ulong), new Func<string, ulong>((value) => ulong.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        // Floating-point types, see https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
        { typeof(float), new Func<string, float>((value) => float.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(double), new Func<string, double>((value) => double.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(decimal), new Func<string, decimal>((value) => decimal.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },

        // Nullable versions
        { typeof(bool?), (Func<string, bool?>)ConvertNullableBoolean },
        { typeof(DateTime?), new Func<string, DateTime?>((value) => DateTime.Parse(value ?? throw new ArgumentNullException(nameof(value)), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)) },
        { typeof(sbyte?), new Func<string, sbyte?>((value) => sbyte.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(byte?), new Func<string, byte?>((value) => byte.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(short?), new Func<string, short?>((value) => short.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(ushort?), new Func<string, ushort?>((value) => ushort.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(int?), new Func<string, int?>((value) => int.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(uint?), new Func<string, uint?>((value) => uint.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(long?), new Func<string, long?>((value) => long.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(ulong?), new Func<string, ulong?>((value) => ulong.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(float?), new Func<string, float?>((value) => float.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(double?), new Func<string, double?>((value) => double.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        { typeof(decimal?), new Func<string, decimal?>((value) => decimal.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
    };

    /// <summary>
    /// Access the hierarchical configuration from the local, global and system locations.
    /// </summary>
    /// <param name="directory">Optional directory to use to read the local configuration from. Defaults to the current directory.</param>
    public static Config Build(string? directory = null)
    {
        var configs = new List<Config>();
        var dir = new DirectoryInfo(directory ?? Directory.GetCurrentDirectory());

        while (dir != null && dir.Exists)
        {
            var file = Path.Combine(dir.FullName, FileName);
            if (File.Exists(file))
                configs.Add(Read(file));

            dir = dir.Parent;
        }

        if (File.Exists(GlobalLocation))
            configs.Add(Read(GlobalLocation));
        if (File.Exists(SystemLocation))
            configs.Add(Read(SystemLocation));

        return new AggregateConfig(configs);
    }

    /// <summary>
    /// Access the configuration from a specific store.
    /// </summary>
    public static Config Read(ConfigLevel store)
    {
        return null;
    }

    /// <summary>
    /// Reads the configuration from a specific file.
    /// </summary>
    public static Config Read(string filePath) => new FileConfig(filePath);

    public abstract bool TryGet<T>(string section, string? subsection, string variable, out T value);

    public T Get<T>(string section, string? subsection, string variable, T defaultValue = default)
        => TryGet<T>(section, subsection, variable, out var value) ? value : defaultValue;

    protected internal T ConvertTo<T>(string value)
    {
        if (Converters.TryGetValue(typeof(T), out var @delegate) &&
            @delegate is Func<string, T> converter)
        {
            return converter(value);
        }
        else
        {
            throw new NotSupportedException($"Conversion from \"{value}\" to {typeof(T)} is not supported.");
        }
    }

    static bool ConvertBoolean(string value) => value switch
    {
        null => true,
        "true" => true,
        "yes" => true,
        "on" => true,
        "1" => true,
        "false" => false,
        "no" => false,
        "off" => false,
        "0" => false,
        _ => throw new ArgumentException($"Unsupported boolean value {value}", nameof(value))
    };

    static bool? ConvertNullableBoolean(string value) => ConvertBoolean(value);
}
