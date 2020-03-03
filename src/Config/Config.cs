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
        /// <see cref="DefaultFileName"/>.
        /// </summary>
        public static string GlobalLocation { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), FileName);

        /// <summary>
        /// Default system location, equal to <see cref="Environment.SpecialFolder.System"/> plus 
        /// <see cref="DefaultFileName"/>.
        /// </summary>
        public static string SystemLocation { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), FileName);

        /// <summary>
        /// Access the hierarchical configuration from the local, global and system locations.
        /// </summary>
        /// <param name="directory">Optional directory to use to read the local configuration from. Defaults to the current directory.</param>
        public static Config Build(string? directory = null)
        {
            var configs = new List<Config>();
            var dir = new DirectoryInfo(directory ?? Directory.GetCurrentDirectory());

            while (dir != null && dir.Exists)
            {
                var file = Path.Combine(dir.FullName, FileName);
                if (File.Exists(file))
                    configs.Add(FromFile(file));

                dir = dir.Parent;
            }

            if (configs.Count == 0)
                configs.Add(FromFile(Path.Combine(directory ?? Directory.GetCurrentDirectory(), FileName)));

            if (File.Exists(GlobalLocation))
                configs.Add(FromFile(GlobalLocation));
            if (File.Exists(SystemLocation))
                configs.Add(FromFile(SystemLocation));

            if (configs.Count == 1)
                return configs[0];

            return new AggregateConfig(configs);
        }

        /// <summary>
        /// Access configuration from a specific file.
        /// </summary>
        /// <remarks>
        /// If the file does not exist, it is considered empty. When writing 
        /// variables, it will be created automatically in that case.
        /// </remarks>
        public static Config FromFile(string filePath) => new FileConfig(filePath);

        /// <summary>
        /// Access the configuration from a specific store.
        /// </summary>
        public static Config Read(ConfigLevel store)
        {
            if (store == ConfigLevel.Global)
                return FromFile(GlobalLocation);
            else if (store == ConfigLevel.System)
                return FromFile(SystemLocation);

            var configs = new List<Config>();
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (dir != null && dir.Exists)
            {
                var file = Path.Combine(dir.FullName, FileName);
                if (File.Exists(file))
                    configs.Add(FromFile(file));

                dir = dir.Parent;
            }

            if (configs.Count == 1)
                return configs[0];

            return new AggregateConfig(configs);
        }

        protected Config(string filePath) => FilePath = filePath;

        /// <summary>
        /// Path to the file that will be used to save values when writing 
        /// changes to disk.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Adds a value to a multi-valued variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value add to the variable.</param>
        public abstract void Add<T>(string section, string? subsection, string variable, T value);

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
        public T Get<T>(string section, string? subsection, string variable, T defaultValue = default)
            => TryGet<T>(section, subsection, variable, out var value) ? value : defaultValue;

        /// <summary>
        /// Gets all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Optional regular expression to match against the variable values.</param>
        public abstract IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, string? valueRegex = null);

        /// <summary>
        /// Gets all values where the key (section plus subsection and variable name) match 
        /// the <paramref name="nameRegex"/> and optionally also the <paramref name="valueRegex"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="nameRegex">Regular expression to match against the key (section plus subsection and variable name).</param>
        /// <param name="valueRegex">Optional regular expression to match against the variable values.</param>
        public abstract IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex);

        /// <summary>
        /// Sets the value of a variable in the given section and optional subsection.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <param name="valueRegex">Optional regular expression to match against the variable value.</param>
        public abstract void Set<T>(string section, string? subsection, string variable, T value, string? valueRegex = null);

        /// <summary>
        /// Sets the value of all matching variables in the given section and optional subsection,
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to assign.</param>
        /// <param name="value">Value to assign to the matching variables.</param>
        /// <param name="valueRegex">Optional regular expression to match against the variable value.</param>
        public abstract void SetAll<T>(string section, string? subsection, string variable, T value, string? valueRegex = null);

        /// <summary>
        /// Tries to retrieve a variable value from configuration.
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve.</typeparam>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to retrieve.</param>
        /// <param name="value">The variable value if found.</param>
        /// <returns><see langword="true"/> if the value was found, <see langword="false"/> otherwise.</returns>
        public abstract bool TryGet<T>(string section, string? subsection, string variable, out T value);

        /// <summary>
        /// Removes a variable from the given section and optional subsection.
        /// </summary>
        /// <typeparam name="T">The type of value to remove.</typeparam>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        public abstract void Unset(string section, string? subsection, string variable);

        /// <summary>
        /// Removes all values from a multi-valued variable from the given section and optional subsection, 
        /// which optionally match the given value regular expression.
        /// </summary>
        /// <typeparam name="T">The type of value to remove.</typeparam>
        /// <param name="section">The section containing the variable.</param>
        /// <param name="subsection">Optional subsection containing the variable.</param>
        /// <param name="variable">The variable to remove.</param>
        /// <param name="valueRegex">Optional regular expression to match against the variable values.</param>
        public abstract void UnsetAll(string section, string? subsection, string variable, string? valueRegex = null);

        /// <summary>
        /// Remove the given section from the configuration file.
        /// </summary>
        /// <param name="section">The section to remove.</param>
        /// <param name="subsection">Optional subsection to remove.</param>
        public abstract void RemoveSection(string section, string? subsection = null);

        /// <summary>
        /// Renames a section and optional subsection.
        /// </summary>
        public abstract void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection);

        /// <summary>
        /// Gets the configuration entries in the current configuration.
        /// </summary>
        protected abstract IEnumerable<ConfigEntry> GetEntries();

        IEnumerator<ConfigEntry> IEnumerable<ConfigEntry>.GetEnumerator() => GetEntries().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEntries().GetEnumerator();
    }
}
