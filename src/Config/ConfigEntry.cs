using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Represents a configuration option.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ConfigEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigEntry" /> class with a given key, value and store level.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable value.</param>
        /// <param name="level">The origin store.</param>
        public ConfigEntry(string section, string? subsection, string name, string? value, ConfigLevel? level)
        {
            Section = section;
            Subsection = subsection;
            Variable = name;
            RawValue = value;
            Level = level;
        }

        /// <summary>
        /// Gets the section containing the entry.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Gets the optional subsection containing the entry.
        /// </summary>
        public string? Subsection { get; }

        /// <summary>
        /// Gets the variable name.
        /// </summary>
        public string Variable { get; }

        /// <summary>
        /// Gets the variable raw value.
        /// </summary>
        public string? RawValue { get; }

        /// <summary>
        /// Gets the origin store. <see langword="null"/> if not either <see cref="ConfigLevel.Global"/> 
        /// or <see cref="ConfigLevel.System"/>.
        /// </summary>
        public ConfigLevel? Level { get; }

        /// <summary>
        /// Gets or sets the optional comment.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets the key for the entry.
        /// </summary>
        public string Key
        {
            get
            {
                var sb = new StringBuilder(Section);
                if (Subsection != null)
                {
                    sb = sb.Append('.');
                    if (Subsection.IndexOfAny(new[] { ' ', '\\', '"', '.' }) == -1)
                        sb = sb.Append(Subsection);
                    else
                        sb = sb.Append("\"").Append(Subsection.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"");
                }

                return sb.Append('.').Append(Variable).ToString();
            }
        }

        /// <summary>
        /// Gets the typed <see cref="bool"/> value for the entry.
        /// </summary>
        /// <returns>The <see cref="bool"/> corresponding to the <see cref="RawValue"/>.</returns>
        /// <exception cref="FormatException">The <see cref="RawValue"/> cannot be converted to <see cref="bool"/>.</exception>
        public bool GetBoolean()
        {
            // Empty or null variable value is true for a boolean
            if (string.IsNullOrWhiteSpace(RawValue))
                return true;

            // Regular bool parsing
            if (bool.TryParse(RawValue, out var value))
                return value;

            // Special cases for common variants of boolean users can use.
            return RawValue switch
            {
                "yes" or "on" or "1" => true,
                "no" or "off" or "0" => false,
                _ => throw new FormatException($"Value '{RawValue}' cannot be converted to boolean."),
            };
        }

        /// <summary>
        /// Gets the typed <see cref="DateTime"/> value for the entry.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> corresponding to the <see cref="RawValue"/>.</returns>
        /// <exception cref="FormatException">The <see cref="RawValue"/> cannot be converted to <see cref="DateTime"/>.</exception>
        public DateTime GetDateTime() => DateTime.Parse(RawValue ?? throw new FormatException(Key), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        /// <summary>
        /// Gets the typed <see cref="long"/> value for the entry.
        /// </summary>
        /// <returns>The <see cref="long"/> corresponding to the <see cref="RawValue"/>.</returns>
        /// <exception cref="FormatException">The <see cref="RawValue"/> cannot be converted to <see cref="long"/>.</exception>
        public long GetNumber() => long.Parse(RawValue ?? throw new FormatException(Key), CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the <see cref="string"/> value for the entry.
        /// </summary>
        /// <returns>The <see cref="string"/> from the <see cref="RawValue"/>.</returns>
        /// <exception cref="FormatException">The <see cref="RawValue"/> cannot be converted to <see cref="string"/>, because it does not have a value.</exception>
        public string GetString() => RawValue ?? throw new FormatException(Key);

        string DebuggerDisplay => $"{Key}={RawValue}";
    }
}
