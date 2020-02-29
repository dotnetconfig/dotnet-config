using System.Diagnostics;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Represents a configuration option.
    /// </summary>
    /// <typeparam name="T">The configuration value type.</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class ConfigEntry<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigEntry{T}" /> class with a given key, value and store level.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable value.</param>
        /// <param name="level">The origin store.</param>
        public ConfigEntry(string section, string subsection, string name, T value, ConfigLevel level)
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
        public string Subsection { get; }

        /// <summary>
        /// The variable name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The variable value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// The origin store.
        /// </summary>
        public ConfigLevel Level { get; }

        public string? Comment { get; set; }

        string DebuggerDisplay => $"{Section + (Subsection == null ? "" : ".\"" + Subsection + "\"")}.{Name} = {Value}";
    }
}
