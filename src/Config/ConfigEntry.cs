using System.Diagnostics;
using System.Text;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Represents a configuration option.
    /// </summary>
    /// <typeparam name="T">The configuration value type.</typeparam>
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
        public ConfigEntry(string section, string? subsection, string name, string? value, ConfigLevel level)
        {
            Section = section;
            Subsection = subsection;
            Name = name;
            Value = value;
            Level = level;
        }

        /// <summary>
        /// The section containing the entry.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Optional subsection containing the entry.
        /// </summary>
        public string? Subsection { get; }

        /// <summary>
        /// The variable name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The variable value.
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// The origin store.
        /// </summary>
        public ConfigLevel Level { get; }

        public string? Comment { get; set; }

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

                return sb.Append('.').Append(Name).ToString();
            }
        }

        string DebuggerDisplay => $"{Key}={Value}";
    }
}
