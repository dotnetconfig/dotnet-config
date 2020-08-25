using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Provides access to .netconfig configuration options.
    /// </summary>
    public abstract class Config : IEnumerable<ConfigEntry>
    {
        /// <summary>
        /// Default filename, equal to '.netconfig'.
        /// </summary>
        public const string FileName = ".netconfig";

        /// <summary>
        /// Default global location, equal to <see cref="Environment.SpecialFolder.UserProfile"/> plus 
        /// <see cref="FileName"/>.
        /// </summary>
        public static string GlobalLocation { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), FileName);

        /// <summary>
        /// Default system location, equal to <see cref="Environment.SpecialFolder.System"/> plus 
        /// <see cref="FileName"/>.
        /// </summary>
        public static string SystemLocation { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), FileName);

        /// <summary>
        /// Builds configuration from the given path, which can be a directory or a file path. 
        /// If ommited, the <see cref="Directory.GetCurrentDirectory"/> will be used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned configuration will contain the aggregate hierarchical configuration 
        /// from the given directory (or file) and any ancestor directories, plus 
        /// <see cref="ConfigLevel.Global"/> and <see cref="ConfigLevel.System"/> locations.
        /// </para>
        /// </remarks>
        public static Config Build(string? path = null)
        {
            AggregateConfig configs;
            DirectoryInfo dir;

            if (string.IsNullOrEmpty(path))
                path = Path.Combine(Directory.GetCurrentDirectory(), FileName);

            if (File.Exists(path))
            {
                configs = new AggregateConfig(new FileConfig(path!));
                dir = new DirectoryInfo(Path.GetDirectoryName(path)).Parent;
            }
            else if (Directory.Exists(path) || !Path.HasExtension(path))
            {
                // First entry is always a file in the given path, even if it's missing.
                // This allows writing to the specified directory.
                configs = new AggregateConfig(new FileConfig(Path.Combine(path, FileName)));
                dir = new DirectoryInfo(path).Parent;
            }
            else
            {
                // If the path does not point to an existing directory
                // we consider it a file path as a fallback.
                configs = new AggregateConfig(new FileConfig(path!));
                dir = new DirectoryInfo(Path.GetDirectoryName(path)).Parent;
            }

            // [config] root = true stops the directory walking
            while (configs.GetBoolean("config", "root") != true && dir != null && dir.Exists)
            {
                var file = Path.Combine(dir.FullName, FileName);
                if (File.Exists(file))
                    configs.Files.Add(new FileConfig(file));

                dir = dir.Parent;
            }

            // But global+system configs are still always added.
            // TODO: maybe we could offer a way to opt-out of those too?

            // Don't read the global location if we're building the system location or it's been opted out explicitly
            if (SystemLocation != path && GlobalLocation != path && configs.GetBoolean("config", "global") != false)
                configs.Files.Add(new FileConfig(GlobalLocation, ConfigLevel.Global));
            if (SystemLocation != path && configs.GetBoolean("config", "system") != false)
                configs.Files.Add(new FileConfig(SystemLocation, ConfigLevel.System));

            if (configs.Files.Count == 1)
                return configs.Files[0];

            return configs;
        }

        /// <summary>
        /// Access the configuration from a specific store.
        /// </summary>
        public static Config Build(ConfigLevel store) => 
            store switch 
            {
                ConfigLevel.Global => Build(GlobalLocation),
                ConfigLevel.System => Build(SystemLocation),
                _ => throw new ArgumentException(nameof(store))
            };

        /// <summary>
        /// Creates the <see cref="Config"/> and sets <see cref="FilePath"/> to the given <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        protected Config(string filePath) => FilePath = filePath;

        /// <summary>
        /// Path to the file that will be used to save values when writing 
        /// changes to disk.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets the optional configuration level for this config. 
        /// <see langword="null"/> unless the <see cref="FilePath"/> equals 
        /// <see cref="GlobalLocation"/> or <see cref="SystemLocation"/>.
        /// </summary>
        public ConfigLevel? Level => 
            this is AggregateConfig ? null :
            FilePath == GlobalLocation ? (ConfigLevel?)ConfigLevel.Global : 
            FilePath == SystemLocation ? (ConfigLevel?)ConfigLevel.System : null;

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public abstract void AddBoolean(string section, string? subsection, string variable, bool value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public abstract void AddDateTime(string section, string? subsection, string variable, DateTime value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public abstract void AddNumber(string section, string? subsection, string variable, long value);

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public abstract void AddString(string section, string? subsection, string variable, string value);

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, ValueMatcher valueMatcher);

        /// <summary>
        /// Gets all values where the key (section plus subsection and variable name) match 
        /// the <paramref name="nameRegex"/> and optionally also the <paramref name="valueRegex"/>.
        /// </summary>
        /// <param name="nameRegex">Regular expression to match against the key (section plus subsection and variable name).</param>
        /// <param name="valueRegex">Optional regular expression to match against the variable values.</param>
        public abstract IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetBoolean(string section, string? subsection, string variable, bool value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetDateTime(string section, string? subsection, string variable, DateTime value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetNumber(string section, string? subsection, string variable, long value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetString(string section, string? subsection, string variable, string value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetAllBoolean(string section, string? subsection, string variable, bool value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetAllDateTime(string section, string? subsection, string variable, DateTime value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetAllNumber(string section, string? subsection, string variable, long value, ValueMatcher valueMatcher);

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void SetAllString(string section, string? subsection, string variable, string value, ValueMatcher valueMatcher);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public abstract bool TryGetBoolean(string section, string? subsection, string variable, out bool value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public abstract bool TryGetDateTime(string section, string? subsection, string variable, out DateTime value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public abstract bool TryGetNumber(string section, string? subsection, string variable, out long value);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public abstract bool TryGetString(string section, string? subsection, string variable, out string value);

        /// <summary>
        /// Removes a variable from the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public abstract void Unset(string section, string? subsection, string variable);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueMatcher">Filter returned entries to those where the value is matched by <see cref="ValueMatcher.Matches(string?)"/>.</param>
        public abstract void UnsetAll(string section, string? subsection, string variable, ValueMatcher valueMatcher);

        /// <summary>
        /// Remove the given section from the configuration file.
        /// </summary>
        /// <param name="section">The section to remove.</param>
        /// <param name="subsection">Optional subsection to remove.</param>
        public abstract void RemoveSection(string section, string? subsection = null);

        /// <summary>
        /// Renames a section and optional subsection.
        /// </summary>
        /// <param name="oldSection">The old section name to rename.</param>
        /// <param name="oldSubsection">The optional old subsection to rename.</param>
        /// <param name="newSection">The new section name to use.</param>
        /// <param name="newSubsection">The optional new subsection to use.</param>
        public abstract void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection);

        /// <summary>
        /// Gets the configuration entries in the current configuration.
        /// </summary>
        protected abstract IEnumerable<ConfigEntry> GetEntries();

        IEnumerator<ConfigEntry> IEnumerable<ConfigEntry>.GetEnumerator() => GetEntries().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEntries().GetEnumerator();
    }
}
