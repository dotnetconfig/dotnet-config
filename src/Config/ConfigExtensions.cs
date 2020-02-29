namespace Microsoft.DotNet
{
    /// <summary>
    /// Usability overloads for <see cref="Config"/>.
    /// </summary>
    internal static class ConfigExtensions
    {
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
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve.</typeparam>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet<T>(this Config config, string section, string variable, out T value)
            => config.TryGet<T>(section, null, variable, out value);
    }
}
