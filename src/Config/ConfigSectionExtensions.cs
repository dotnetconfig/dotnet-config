using System;
using System.ComponentModel;

namespace DotNetConfig
{
    /// <summary>
    /// Usability overloads for <see cref="ConfigSection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ConfigSectionExtensions
    {
        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection AddBoolean(this ConfigSection config, string variable, bool value, ConfigLevel level)
            => config with { Config = config.Config.AddBoolean(config.Section, config.Subsection, variable, value, level) };

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection AddDateTime(this ConfigSection config, string variable, DateTime value, ConfigLevel level)
            => config with { Config = config.Config.AddDateTime(config.Section, config.Subsection, variable, value, level) };

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection AddNumber(this ConfigSection config, string variable, long value, ConfigLevel level)
            => config with { Config = config.Config.AddNumber(config.Section, config.Subsection, variable, value, level) };

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection AddString(this ConfigSection config, string variable, string value, ConfigLevel level)
            => config with { Config = config.Config.AddString(config.Section, config.Subsection, variable, value, level) };

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static bool? GetBoolean(this ConfigSection config, string variable)
            => config.Config.GetBoolean(config.Section, config.Subsection, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static DateTime? GetDateTime(this ConfigSection config, string variable)
            => config.Config.GetDateTime(config.Section, config.Subsection, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static long? GetNumber(this ConfigSection config, string variable)
            => config.Config.GetNumber(config.Section, config.Subsection, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static string? GetString(this ConfigSection config, string variable)
            => config.Config.GetString(config.Section, config.Subsection, variable);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetBoolean(this ConfigSection config, string variable, bool value, ConfigLevel level)
            => config with { Config = config.Config.SetBoolean(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetBoolean(this ConfigSection config, string variable, bool value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetBoolean(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetDateTime(this ConfigSection config, string variable, DateTime value, ConfigLevel level)
            => config with { Config = config.Config.SetDateTime(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetDateTime(this ConfigSection config, string variable, DateTime value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetDateTime(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetNumber(this ConfigSection config, string variable, long value, ConfigLevel level)
            => config with { Config = config.Config.SetNumber(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetNumber(this ConfigSection config, string variable, long value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetNumber(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetString(this ConfigSection config, string variable, string value, ConfigLevel level)
            => config with { Config = config.Config.SetString(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetString(this ConfigSection config, string variable, string value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetString(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllBoolean(this ConfigSection config, string variable, bool value, ConfigLevel level)
            => config with { Config = config.Config.SetAllBoolean(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllBoolean(this ConfigSection config, string variable, bool value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetAllBoolean(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllDateTime(this ConfigSection config, string variable, DateTime value, ConfigLevel level)
            => config with { Config = config.Config.SetAllDateTime(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllDateTime(this ConfigSection config, string variable, DateTime value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetAllDateTime(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllNumber(this ConfigSection config, string variable, long value, ConfigLevel level)
            => config with { Config = config.Config.SetAllNumber(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllNumber(this ConfigSection config, string variable, long value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetAllNumber(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllString(this ConfigSection config, string variable, string value, ConfigLevel level)
            => config with { Config = config.Config.SetAllString(config.Section, config.Subsection, variable, value, null, level) };

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection SetAllString(this ConfigSection config, string variable, string value, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.SetAllString(config.Section, config.Subsection, variable, value, valueRegex, level) };

        /// <summary>
        /// Removes a variable from the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection Unset(this ConfigSection config, string variable, ConfigLevel level)
            => config with { Config = config.Config.Unset(config.Section, config.Subsection, variable, level) };

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection UnsetAll(this ConfigSection config, string variable, ConfigLevel level)
            => config with { Config = config.Config.UnsetAll(config.Section, config.Subsection, variable, null, level) };

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration section to operate on.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static ConfigSection UnsetAll(this ConfigSection config, string variable, string? valueRegex, ConfigLevel level)
            => config with { Config = config.Config.UnsetAll(config.Section, config.Subsection, variable, valueRegex, level) };
    }
}
