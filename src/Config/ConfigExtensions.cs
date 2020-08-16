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
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public static void Add<T>(this Config config, string section, string variable, T value)
            => config.Add(section, null, variable, value);

        /// <summary>
        /// Gets the value from a variable in the given section.
        /// </summary>
        /// <typeparam name="T">Type of value to return.</typeparam>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="defaultValue">Default value to return if the variable is not found.</param>
        /// <returns>The variable value or <paramref name="defaultValue"/> if not found.</returns>
        public static T Get<T>(this Config config, string section, string variable, T defaultValue = default) 
            => config.Get(section, null, variable, defaultValue);

        /// <summary>
        /// Gets the value from a variable in the given section and optional subsection.
        /// </summary>
        /// <typeparam name="T">Type of value to return.</typeparam>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="defaultValue">Default value to return if the variable is not found.</param>
        /// <returns>The variable value or <paramref name="defaultValue"/> if not found.</returns>
        public static T Get<T>(this Config config, string section, string? subsection, string variable, T defaultValue = default)
            => config.TryGet<T>(section, subsection, variable, out var value) ? value : defaultValue;

        /// <summary>
        /// Gets the value from a variable in the given section.
        /// </summary>
        /// <typeparam name="T">Type of value to return.</typeparam>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <returns>The variable value or the <c>default</c> for <typeparamref name="T"/>.</returns>
        public static T Get<T>(this Config config, string section, string variable)
            => config.Get<T>(section, null, variable);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string variable)
            => config.GetAll(section, null, variable, ValueMatcher.All);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string? subsection, string variable)
            => config.GetAll(section, subsection, variable, ValueMatcher.All);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static IEnumerable<ConfigEntry> GetAll(this Config config, string section, string variable, ValueMatcher valueMatcher)
            => config.GetAll(section, null, variable, valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void Set<T>(this Config config, string section, string variable, T value)
            => config.Set(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        public static void Set<T>(this Config config, string section, string? subsection, string variable, T value)
            => config.Set(section, subsection, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of a variable in the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void Set<T>(this Config config, string section, string variable, T value, ValueMatcher valueMatcher) 
            => config.Set(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static void SetAll<T>(this Config config, string section, string variable, T value)
            => config.SetAll(section, null, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        public static void SetAll<T>(this Config config, string section, string? subsection, string variable, T value)
            => config.SetAll(section, subsection, variable, value, ValueMatcher.All);

        /// <summary>
        /// Sets the value of all matching variables in the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void SetAll<T>(this Config config, string section, string variable, T value, ValueMatcher valueMatcher)
            => config.SetAll(section, null, variable, value, valueMatcher);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve.</typeparam>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet<T>(this Config config, string section, string variable, out T value)
            => config.TryGet(section, null, variable, out value);

        /// <summary>
        /// Removes a variable from the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static void Unset(this Config config, string section, string variable) 
            => config.Unset(section, null, variable);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public static void UnsetAll(this Config config, string section, string variable)
            => config.UnsetAll(section, null, variable, ValueMatcher.All);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section.
        /// </summary>
        /// <param name="config">The configuration accessor.</param>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public static void UnsetAll(this Config config, string section, string variable, ValueMatcher valueMatcher) 
            => config.UnsetAll(section, null, variable, valueMatcher);
    }
}
