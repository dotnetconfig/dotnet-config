﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DotNetConfig
{
    /// <summary>
    /// Usability overloads for <see cref="Config"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ConfigExtensions
    {
        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static Config AddBoolean(this Config config, string section, string variable, bool value)
            => config.AddBoolean(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="subsection">The subsection containing the variable.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddBoolean(this Config config, string section, string? subsection, string variable, bool value, ConfigLevel level)
            => Write(config, level, x => x.AddBoolean(section, subsection, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddBoolean(this Config config, string section, string variable, bool value, ConfigLevel level)
            => Write(config, level, x => x.AddBoolean(section, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static Config AddDateTime(this Config config, string section, string variable, DateTime value)
            => config.AddDateTime(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">The subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddDateTime(this Config config, string section, string? subsection, string variable, DateTime value, ConfigLevel level)
            => Write(config, level, x => x.AddDateTime(section, subsection, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddDateTime(this Config config, string section, string variable, DateTime value, ConfigLevel level)
            => Write(config, level, x => x.AddDateTime(section, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static Config AddNumber(this Config config, string section, string variable, long value)
            => config.AddNumber(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddNumber(this Config config, string section, string variable, long value, ConfigLevel level)
            => Write(config, level, x => x.AddNumber(section, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">The subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddNumber(this Config config, string section, string? subsection, string variable, long value, ConfigLevel level)
            => Write(config, level, x => x.AddNumber(section, subsection, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static Config AddString(this Config config, string section, string variable, string value)
            => config.AddString(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddString(this Config config, string section, string variable, string value, ConfigLevel level)
            => Write(config, level, x => x.AddString(section, variable, value));

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">The optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config AddString(this Config config, string section, string? subsection, string variable, string value, ConfigLevel level)
            => Write(config, level, x => x.AddString(section, subsection, variable, value));

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static bool? GetBoolean(this Config config, string section, string? subsection, string variable)
            => config.TryGetBoolean(section, subsection, variable, out var value) ? (bool?)value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static bool? GetBoolean(this Config config, string section, string variable)
            => config.GetBoolean(section, null, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static DateTime? GetDateTime(this Config config, string section, string? subsection, string variable)
            => config.TryGetDateTime(section, subsection, variable, out var value) ? (DateTime?)value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static DateTime? GetDateTime(this Config config, string section, string variable)
            => config.GetDateTime(section, null, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static long? GetNumber(this Config config, string section, string? subsection, string variable)
            => config.TryGetNumber(section, subsection, variable, out var value) ? (long?)value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static long? GetNumber(this Config config, string section, string variable)
            => config.GetNumber(section, null, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static string? GetString(this Config config, string section, string? subsection, string variable)
            => config.TryGetString(section, subsection, variable, out var value) ? value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static string? GetString(this Config config, string section, string variable)
            => config.GetString(section, null, variable);

        /// <summary>
        /// Gets a string variable and applies path normalization to it, resolving 
        /// relative paths and normalizing directory separator characters to the 
        /// current platform.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve as a resolved path.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static string? GetNormalizedPath(this Config config, string section, string variable)
            => config.GetNormalizedPath(section, null, variable);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to get the values from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string variable)
            => config.GetAll(section, null, variable, null);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="config">The configuration to get the values from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string? subsection, string variable)
            => config.GetAll(section, subsection, variable, null);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetBoolean(this Config config, string section, string variable, bool value)
            => config.SetBoolean(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetBoolean(this Config config, string section, string? subsection, string variable, bool value)
            => config.SetBoolean(section, subsection, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetBoolean(this Config config, string section, string variable, bool value, string? valueRegex)
            => config.SetBoolean(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetBoolean(this Config config, string section, string variable, bool value, ConfigLevel level)
            => Write(config, level, x => x.SetBoolean(section, null, variable, value));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetBoolean(this Config config, string section, string? subsection, string variable, bool value, ConfigLevel level)
            => Write(config, level, x => x.SetBoolean(section, subsection, variable, value));

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetBoolean(this Config config, string section, string variable, bool value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetBoolean(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetBoolean(this Config config, string section, string? subsection, string variable, bool value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetBoolean(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetDateTime(this Config config, string section, string variable, DateTime value)
            => config.SetDateTime(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetDateTime(this Config config, string section, string? subsection, string variable, DateTime value)
            => config.SetDateTime(section, subsection, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetDateTime(this Config config, string section, string variable, DateTime value, string? valueRegex)
            => config.SetDateTime(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetDateTime(this Config config, string section, string variable, DateTime value, ConfigLevel level)
            => Write(config, level, x => x.SetDateTime(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetDateTime(this Config config, string section, string? subsection, string variable, DateTime value, ConfigLevel level)
            => Write(config, level, x => x.SetDateTime(section, subsection, variable, value, null));

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetDateTime(this Config config, string section, string variable, DateTime value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetDateTime(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetDateTime(this Config config, string section, string? subsection, string variable, DateTime value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetDateTime(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetNumber(this Config config, string section, string variable, long value)
            => config.SetNumber(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetNumber(this Config config, string section, string? subsection, string variable, long value)
            => config.SetNumber(section, subsection, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetNumber(this Config config, string section, string variable, long value, string? valueRegex)
            => config.SetNumber(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetNumber(this Config config, string section, string variable, long value, ConfigLevel level)
            => Write(config, level, x => x.SetNumber(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetNumber(this Config config, string section, string? subsection, string variable, long value, ConfigLevel level)
            => Write(config, level, x => x.SetNumber(section, subsection, variable, value, null));

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetNumber(this Config config, string section, string variable, long value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetNumber(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetNumber(this Config config, string section, string? subsection, string variable, long value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetNumber(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetString(this Config config, string section, string variable, string value)
            => config.SetString(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static Config SetString(this Config config, string section, string? subsection, string variable, string value)
            => config.SetString(section, subsection, variable, value, null);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetString(this Config config, string section, string variable, string value, ConfigLevel level)
            => Write(config, level, x => x.SetString(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetString(this Config config, string section, string? subsection, string variable, string value, ConfigLevel level)
            => Write(config, level, x => x.SetString(section, subsection, variable, value, null));

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetString(this Config config, string section, string? subsection, string variable, string value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetString(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static Config SetAllBoolean(this Config config, string section, string variable, bool value)
            => config.SetAllBoolean(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetAllBoolean(this Config config, string section, string variable, bool value, string? valueRegex)
            => config.SetAllBoolean(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllBoolean(this Config config, string section, string variable, bool value, ConfigLevel level)
            => Write(config, level, x => x.SetAllBoolean(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllBoolean(this Config config, string section, string variable, bool value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllBoolean(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllBoolean(this Config config, string section, string? subsection, string variable, bool value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllBoolean(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static Config SetAllDateTime(this Config config, string section, string variable, DateTime value)
            => config.SetAllDateTime(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetAllDateTime(this Config config, string section, string variable, DateTime value, string? valueRegex)
            => config.SetAllDateTime(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllDateTime(this Config config, string section, string variable, DateTime value, ConfigLevel level)
            => Write(config, level, x => x.SetAllDateTime(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllDateTime(this Config config, string section, string variable, DateTime value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllDateTime(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllDateTime(this Config config, string section, string? subsection, string variable, DateTime value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllDateTime(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static Config SetAllNumber(this Config config, string section, string variable, long value)
            => config.SetAllNumber(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetAllNumber(this Config config, string section, string variable, long value, string? valueRegex)
            => config.SetAllNumber(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllNumber(this Config config, string section, string variable, long value, ConfigLevel level)
            => Write(config, level, x => x.SetAllNumber(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllNumber(this Config config, string section, string variable, long value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllNumber(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllNumber(this Config config, string section, string? subsection, string variable, long value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllNumber(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static Config SetAllString(this Config config, string section, string variable, string value)
            => config.SetAllString(section, null, variable, value, null);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config SetAllString(this Config config, string section, string variable, string value, string? valueRegex)
            => config.SetAllString(section, null, variable, value, valueRegex);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllString(this Config config, string section, string variable, string value, ConfigLevel level)
            => Write(config, level, x => x.SetAllString(section, null, variable, value, null));

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllString(this Config config, string section, string variable, string value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllString(section, null, variable, value, valueRegex));

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config SetAllString(this Config config, string section, string? subsection, string variable, string value, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => x.SetAllString(section, subsection, variable, value, valueRegex));

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetBoolean(this Config config, string section, string variable, out bool value)
            => config.TryGetBoolean(section, null, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetDateTime(this Config config, string section, string variable, out DateTime value)
            => config.TryGetDateTime(section, null, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetNumber(this Config config, string section, string variable, out long value)
            => config.TryGetNumber(section, null, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetString(this Config config, string section, string variable, out string value)
            => config.TryGetString(section, null, variable, out value);

        /// <summary>
        /// Removes a variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static Config Unset(this Config config, string section, string variable)
            => config.Unset(section, null, variable);

        /// <summary>
        /// Removes a variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config Unset(this Config config, string section, string variable, ConfigLevel level)
            => Write(config, level, x => x.Unset(section, null, variable));

        /// <summary>
        /// Removes a variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config Unset(this Config config, string section, string? subsection, string variable, ConfigLevel level)
            => Write(config, level, x => x.Unset(section, subsection, variable));

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config UnsetAll(this Config config, string section, string variable, ConfigLevel level)
            => Write(config, level, x => config.UnsetAll(section, null, variable, null));

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        public static Config UnsetAll(this Config config, string section, string variable, string? valueRegex)
            => config.UnsetAll(section, null, variable, valueRegex);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config UnsetAll(this Config config, string section, string variable, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => config.UnsetAll(section, null, variable, valueRegex));

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Filter returned entries to those where the value matches the given expression.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config UnsetAll(this Config config, string section, string? subsection, string variable, string? valueRegex, ConfigLevel level)
            => Write(config, level, x => config.UnsetAll(section, subsection, variable, valueRegex));

        /// <summary>
        /// Remove the given section from the configuration file.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section to remove.</param>
        public static Config RemoveSection(this Config config, string section) => config.RemoveSection(section, null);

        /// <summary>
        /// Remove the given section from the configuration file.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section to remove.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config RemoveSection(this Config config, string section, ConfigLevel level)
            => Write(config, level, x => config.RemoveSection(section, null));

        /// <summary>
        /// Renames a section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="oldSection">The old section name to rename.</param>
        /// <param name="newSection">The new section name to use.</param>
        public static Config RenameSection(this Config config, string oldSection, string newSection)
            => config.RenameSection(oldSection, null, newSection, null);

        /// <summary>
        /// Renames a section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="oldSection">The old section name to rename.</param>
        /// <param name="newSection">The new section name to use.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config RenameSection(this Config config, string oldSection, string newSection, ConfigLevel level)
            => Write(config, level, x => config.RenameSection(oldSection, null, newSection, null));

        /// <summary>
        /// Renames a section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="oldSection">The old section name to rename.</param>
        /// <param name="oldSubsection">The optional old subsection to rename.</param>
        /// <param name="newSection">The new section name to use.</param>
        /// <param name="newSubsection">The optional new subsection to use.</param>
        /// <param name="level">The configuration level to operate on.</param>
        public static Config RenameSection(this Config config, string oldSection, string? oldSubsection, string newSection, string? newSubsection, ConfigLevel level)
            => Write(config, level, x => config.RenameSection(oldSection, oldSubsection, newSection, newSubsection));

        static Config Write(Config config, ConfigLevel level, Func<Config, Config> function)
        {
            if (config.Level == level)
            {
                return function(config);
            }

            if (config is AggregateConfig aggregate &&
                aggregate.Files.FirstOrDefault(x => x.Level == level) is Config cfg)
            {
                return function(cfg);
            }

            throw new ArgumentException($"Could not write variable for level {level} using the given config.", nameof(config));
        }
    }
}
