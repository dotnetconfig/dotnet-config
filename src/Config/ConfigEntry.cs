using System.Diagnostics;
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
            Name = name;
            Value = value;
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
        public string Name { get; }

        /// <summary>
        /// Gets the variable value.
        /// </summary>
        public string? Value { get; }

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

                return sb.Append('.').Append(Name).ToString();
            }
        }

        string DebuggerDisplay => $"{Key}={Value}";
    }
}
