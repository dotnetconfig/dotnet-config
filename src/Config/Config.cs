using System;
using System.Collections.Generic;
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
        from section in Character.EqualTo('[').IgnoreThen(Character.Matching(c => char.IsLetterOrDigit(c) || c == '-' || c == '.', "alphanumeric, '-' or '.'").Many())
        from subsection in Character.WhiteSpace.IgnoreThen(QuotedString.CStyle).OptionalOrDefault()
        from _ in Character.EqualTo(']')
        select (new string(section), subsection);

    internal static TextParser<(string name, string value)> VariableParser { get; } =
        from name in Character.WhiteSpace.IgnoreMany().IgnoreThen(
            Span.MatchedBy(Character.Letter.IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('-')).IgnoreMany())))
        from value in
                Character.WhiteSpace.IgnoreMany().IgnoreThen(
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                // Couldn't figure out how to fix the proper nullability constraints here
                Character.EqualTo('=').IgnoreThen(
                Character.WhiteSpace.IgnoreMany().IgnoreThen(
                QuotedString.CStyle.Or(
                    Character.ExceptIn('#', ';').Many().Select(x => new string(x).Trim())))).OptionalOrDefault())
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        select (name.ToStringValue(), value);

    internal static Dictionary<Type, Delegate> Converters { get; } = new Dictionary<Type, Delegate>
    {
        { typeof(bool), (Func<string, bool>)ConvertBoolean },
        { typeof(DateTime), new Func<string, DateTime>((value) => DateTime.Parse(value ?? throw new ArgumentNullException(nameof(value)), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)) },
        { typeof(int), (Func<string, int>)ConvertInt },
        { typeof(long), (Func<string, long>)ConvertLong },

        // Nullable versions
        { typeof(bool?), (Func<string, bool?>)ConvertNullableBoolean },
        { typeof(DateTime?), new Func<string, DateTime?>((value) => DateTime.Parse(value ?? throw new ArgumentNullException(nameof(value)), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)) },
        { typeof(int?), (Func<string, int?>)ConvertNullableInt },
        { typeof(long?), (Func<string, long?>)ConvertNullableLong },
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

    static bool ConvertBoolean(string value)
    {
        if (value == "1" || value == null || 
            value.Equals("true", StringComparison.OrdinalIgnoreCase) || 
            value.Equals("on", StringComparison.OrdinalIgnoreCase) || 
            value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value == "0" ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("no", StringComparison.OrdinalIgnoreCase))
            return false;

        throw new ArgumentException($"Unsupported boolean value {value}", nameof(value));
    }

    static bool? ConvertNullableBoolean(string value) => ConvertBoolean(value);

    static int ConvertInt(string value) => (int)ConvertLong(value);

    static int? ConvertNullableInt(string value) => (int?)ConvertLong(value);

    const long KB = 1024;

    static long ConvertLong(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Cannot convert null or empty string to an integer.");

        var parser = from number in Character.Numeric.Many()
                     from suffix in Character.In('k', 'K', 'm', 'M', 'g', 'G', 'T', 't').Optional()
                     select (number, suffix);

        var result = parser.Parse(value);
        var multiplier = result.suffix switch
        {
            'k' => KB,
            'K' => KB,
            'm' => KB * KB,
            'M' => KB * KB,
            'g' => KB * KB * KB,
            'G' => KB * KB * KB,
            't' => KB * KB * KB * KB,
            'T' => KB * KB * KB * KB,
            _ => 1
        };

        return int.Parse(new string(result.number)) * multiplier;
    }

    static long? ConvertNullableLong(string value) => ConvertLong(value);

}
