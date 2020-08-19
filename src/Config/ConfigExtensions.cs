using System;
using System.Collections.Generic;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Usability overloads for <see cref="Config"/>.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static void AddBoolean(this Config config, string section, string variable, bool value) => config.AddBoolean(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static void AddDateTime(this Config config, string section, string variable, DateTime value) => config.AddDateTime(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static void AddNumber(this Config config, string section, string variable, long value) => config.AddNumber(section, null, variable, value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to add the value to.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static void AddString(this Config config, string section, string variable, string value) => config.AddString(section, null, variable, value);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static bool? GetBoolean(this Config config, string section, string? subsection, string variable)
            => config.TryGetBoolean(section, subsection, variable, out var value) ? value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static bool? GetBoolean(this Config config, string section, string variable) => config.GetBoolean(section, null, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static DateTime? GetDateTime(this Config config, string section, string? subsection, string variable)
            => config.TryGetDateTime(section, subsection, variable, out var value) ? value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static DateTime? GetDateTime(this Config config, string section, string variable) => config.GetDateTime(section, null, variable);

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static long? GetNumber(this Config config, string section, string? subsection, string variable)
            => config.TryGetNumber(section, subsection, variable, out var value) ? value : null;

        /// <summary>
        /// Retrieves a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to get the value from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The value or <see langword="null"/> if not found.</returns>
        public static long? GetNumber(this Config config, string section, string variable) => config.GetNumber(section, null, variable);

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
        public static string? GetString(this Config config, string section, string variable) => config.GetString(section, null, variable);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to get the values from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string variable) => config.GetAll(section, null, variable, ValueMatcher.All);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="config">The configuration to get the values from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string? subsection, string variable) => config.GetAll(section, subsection, variable, ValueMatcher.All);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="config">The configuration to get the values from.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string variable, ValueMatcher valueMatcher) => config.GetAll(section, null, variable, valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetBoolean(this Config config, string section, string variable, bool value) => config.SetBoolean(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetBoolean(this Config config, string section, string? subsection, string variable, bool value) => config.SetBoolean(section, subsection, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetBoolean(this Config config, string section, string variable, bool value, ValueMatcher valueMatcher) => config.SetBoolean(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetDateTime(this Config config, string section, string variable, DateTime value) => config.SetDateTime(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetDateTime(this Config config, string section, string? subsection, string variable, DateTime value) => config.SetDateTime(section, subsection, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetDateTime(this Config config, string section, string variable, DateTime value, ValueMatcher valueMatcher) => config.SetDateTime(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetNumber(this Config config, string section, string variable, long value) => config.SetNumber(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetNumber(this Config config, string section, string? subsection, string variable, long value) => config.SetNumber(section, subsection, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetNumber(this Config config, string section, string variable, long value, ValueMatcher valueMatcher) => config.SetNumber(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetString(this Config config, string section, string variable, string value) => config.SetString(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void SetString(this Config config, string section, string? subsection, string variable, string value) => config.SetString(section, subsection, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetString(this Config config, string section, string variable, string value, ValueMatcher valueMatcher) => config.SetString(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static void SetAllBoolean(this Config config, string section, string variable, bool value) => config.SetAllBoolean(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetAllBoolean(this Config config, string section, string variable, bool value, ValueMatcher valueMatcher) => config.SetAllBoolean(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static void SetAllDateTime(this Config config, string section, string variable, DateTime value) => config.SetAllDateTime(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetAllDateTime(this Config config, string section, string variable, DateTime value, ValueMatcher valueMatcher) => config.SetAllDateTime(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static void SetAllNumber(this Config config, string section, string variable, long value) => config.SetAllNumber(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetAllNumber(this Config config, string section, string variable, long value, ValueMatcher valueMatcher) => config.SetAllNumber(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static void SetAllString(this Config config, string section, string variable, string value) => config.SetAllString(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetAllString(this Config config, string section, string variable, string value, ValueMatcher valueMatcher) => config.SetAllString(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetBoolean(this Config config, string section, string variable, out bool value) => config.TryGetBoolean(section, null, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetDateTime(this Config config, string section, string variable, out DateTime value) => config.TryGetDateTime(section, null, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetNumber(this Config config, string section, string variable, out long value) => config.TryGetNumber(section, null, variable, out value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetString(this Config config, string section, string variable, out string value) => config.TryGetString(section, null, variable, out value);


        /// <summary>
        /// Removes a variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static void Unset(this Config config, string section, string variable) => config.Unset(section, null, variable);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static void UnsetAll(this Config config, string section, string variable) => config.UnsetAll(section, null, variable, ValueMatcher.All);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void UnsetAll(this Config config, string section, string variable, ValueMatcher valueMatcher) => config.UnsetAll(section, null, variable, valueMatcher);

        /// <summary>
        /// Remove the given section from the configuration file.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="section">The section to remove.</param>
        public static void RemoveSection(this Config config, string section) => config.RemoveSection(section, null);

        /// <summary>
        /// Renames a section.
        /// </summary>
        /// <param name="config">The configuration to operate on.</param>
        /// <param name="oldSection">The old section name to rename.</param>
        /// <param name="newSection">The new section name to use.</param>
        public static void RenameSection(this Config config, string oldSection, string newSection) => config.RenameSection(oldSection, null, newSection, null);
    }
}
