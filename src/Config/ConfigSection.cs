using System;
using System.Collections.Generic;

namespace DotNetConfig
{
    /// <summary>
    /// Provides access to a specific section and optional subsection.
    /// </summary>
    public record ConfigSection
    {
        internal ConfigSection(Config config, string section, string? subsection)
            => (Config, Section, Subsection)
            = (config, section, subsection);

        internal Config Config { get; init; }

        internal string Section { get; }

        internal string? Subsection { get; }

        /// <summary>
        /// Adds a value to a multi-valued variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public ConfigSection AddBoolean(string variable, bool value)
            => this with { Config = Config.AddBoolean(Section, Subsection, variable, value) };

        /// <summary>
        /// Adds a value to a multi-valued variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public ConfigSection AddDateTime(string variable, DateTime value)
            => this with { Config = Config.AddDateTime(Section, Subsection, variable, value) };

        /// <summary>
        /// Adds a value to a multi-valued variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public ConfigSection AddNumber(string variable, long value)
            => this with { Config = Config.AddNumber(Section, Subsection, variable, value) };

        /// <summary>
        /// Adds a value to a multi-valued variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public ConfigSection AddString(string variable, string value)
            => this with { Config = Config.AddString(Section, Subsection, variable, value) };

        /// <summary>
        /// Gets all values from a multi-valued variable from the current section/subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public IEnumerable<ConfigEntry> GetAll(string variable, string? valueRegex = null)
            => Config.GetAll(Section, Subsection, variable, valueRegex);

        /// <summary>
        /// Gets a string variable and applies path normalization to it, resolving 
        /// relative paths and normalizing directory separator characters to the 
        /// current platform.
        /// </summary>
        /// <param name="variable">The variable to retrieve as a resolved path.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public string? GetNormalizedPath(string variable) => Config.GetNormalizedPath(Section, Subsection, variable);

        /// <summary>
        /// Sets the value of all matching variables in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetAllBoolean(string variable, bool value, string? valueRegex = null)
            => this with { Config = Config.SetAllBoolean(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of all matching variables in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetAllDateTime(string variable, DateTime value, string? valueRegex = null)
            => this with { Config = Config.SetAllDateTime(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of all matching variables in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetAllNumber(string variable, long value, string? valueRegex = null)
            => this with { Config = Config.SetAllNumber(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of all matching variables in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetAllString(string variable, string value, string? valueRegex = null)
            => this with { Config = Config.SetAllString(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of a variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetBoolean(string variable, bool value, string? valueRegex = null)
            => this with { Config = Config.SetBoolean(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of a variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetDateTime(string variable, DateTime value, string? valueRegex = null)
            => this with { Config = Config.SetDateTime(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of a variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetNumber(string variable, long value, string? valueRegex = null)
            => this with { Config = Config.SetNumber(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Sets the value of a variable in the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection SetString(string variable, string value, string? valueRegex = null)
            => this with { Config = Config.SetString(Section, Subsection, variable, value, valueRegex) };

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public bool TryGetBoolean(string variable, out bool value) => Config.TryGetBoolean(Section, Subsection, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public bool TryGetDateTime(string variable, out DateTime value) => Config.TryGetDateTime(Section, Subsection, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public bool TryGetNumber(string variable, out long value) => Config.TryGetNumber(Section, Subsection, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public bool TryGetString(string variable, out string value) => Config.TryGetString(Section, Subsection, variable, out value);

        /// <summary>
        /// Removes a variable from the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to remove.</param>
        public ConfigSection Unset(string variable)
            => this with { Config = Config.Unset(Section, Subsection, variable) };

        /// <summary>
        /// Removes all values from a multi-valued variable from the current section/subsection.
        /// </summary>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public ConfigSection UnsetAll(string variable, string? valueRegex = null)
            => this with { Config = Config.UnsetAll(Section, Subsection, variable, valueRegex) };
    }
}
