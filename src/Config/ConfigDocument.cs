using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetConfig
{
    internal record ConfigDocument : IEnumerable<ConfigEntry>
    {
        readonly string filePath;

        ConfigDocument(string filePath, ConfigLevel? level = null)
        {
            this.filePath = new FileInfo(filePath).FullName;
            if (File.Exists(this.filePath))
            {
                using var reader = new ConfigReader(filePath);
                Lines = reader.ReadAllLines().ToImmutableList();
            }

            if (level == null)
            {
                if (this.filePath == Config.GlobalLocation)
                    level = ConfigLevel.Global;
                else if (this.filePath == Config.SystemLocation)
                    level = ConfigLevel.System;
                else if (this.filePath.EndsWith(Config.UserExtension, StringComparison.Ordinal))
                    level = ConfigLevel.Local;
            }

            Level = level;
        }

        public static ConfigDocument FromFile(string filePath, ConfigLevel? level = null) => new ConfigDocument(filePath, level);

        public ConfigLevel? Level { get; }

        internal ImmutableList<Line> Lines { get; init; } = ImmutableList<Line>.Empty;

        public ConfigDocument Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using var writer = new StreamWriter(filePath, false);
            foreach (var line in Lines)
            {
                writer.WriteLine(line.LineText);
            }

            return this;
        }

        public IEnumerable<ConfigEntry> GetAll(string nameRegex, string? valueRegex = null)
        {
            var nameMatches = Matches(nameRegex);
            var valueMatches = Matches(valueRegex);

            return Entries.Where(x => nameMatches(x.Key) && valueMatches(x.RawValue));
        }

        public IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string name, ValueMatcher? valueMatcher = null)
        {
            var matcher = valueMatcher ?? ValueMatcher.All;
            return Variables.Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name && matcher.Matches(line.Value))
                .Select(ToEntry);
        }

        public ConfigDocument Add(string section, string? subsection, string name, string? value)
        {
            var sectionLine = Lines.Where(line => line.Kind == LineKind.Section).FirstOrDefault(SectionEquals(section, subsection));
            var lines = Lines;

            if (sectionLine == null)
            {
                sectionLine = Line.CreateSection(filePath, Lines.Count, section, subsection);
                lines = Lines
                    .Add(sectionLine)
                    .Add(Line.CreateVariable(filePath, Lines.Count, sectionLine.Section, sectionLine.Subsection, name, value));
            }
            else
            {
                var index = Lines.IndexOf(sectionLine);
                // Move to the last variable
                while (++index < Lines.Count && Lines[index].Kind == LineKind.Variable)
                    ;

                lines = lines.Insert(index, Line.CreateVariable(filePath, index, sectionLine.Section, sectionLine.Subsection, name, value));
            }

            return this with { Lines = lines };
        }

        public ConfigDocument Set(string section, string? subsection, string name, string? value = null, ValueMatcher? valueMatcher = null)
        {
            var variables = Variables
                .Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name);

            // Cannot modify multiple with this method. Use SetAll instead.
            if (variables.Skip(1).Any())
                throw new NotSupportedException($"Multi-valued property '{name}' found. Use {nameof(SetAll)} instead.");

            var matcher = valueMatcher ?? ValueMatcher.All;
            var variable = variables
                .Where(line => matcher.Matches(line.Value))
                .FirstOrDefault();

            if (variable != null)
            {
                return this with { Lines = Lines.Replace(variable, variable.WithValue(value)) };
            }
            else
            {
                return Add(section, subsection, name, value);
            }
        }

        public ConfigDocument Unset(string section, string? subsection, string name)
        {
            // Forces validation
            Line.CreateSection(filePath, 0, section, subsection);

            var variables = Variables
                .Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name);

            // Cannot modify multiple with this method. Use SetAll instead.
            if (variables.Skip(1).Any())
                throw new NotSupportedException($"Multi-valued property '{name}' found. Use {nameof(SetAll)} instead.");

            var variable = variables.FirstOrDefault();
            if (variable != null)
            {
                return (this with { Lines = Lines.Remove(variable) }).CleanupSection(section, subsection);
            }

            return this;
        }

        public ConfigDocument SetAll(string section, string? subsection, string name, string? value, ValueMatcher? valueMatcher = null)
        {
            // Forces validation
            var sl = Line.CreateSection(filePath, 0, section, subsection);
            Line.CreateVariable(filePath, 0, sl.Section, sl.Subsection, name, value);

            var matcher = valueMatcher ?? ValueMatcher.All;
            var lines = Lines;
            foreach (var variable in Variables.Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name && matcher.Matches(line.Value))
                .ToArray())
            {
                lines = lines.Replace(variable, variable.WithValue(value));
            }

            return this with { Lines = lines };
        }

        public ConfigDocument UnsetAll(string section, string? subsection, string name, ValueMatcher? valueMatcher = null)
        {
            // Forces validation
            var sl = Line.CreateSection(filePath, 0, section, subsection);
            Line.CreateVariable(filePath, 0, sl.Section, sl.Subsection, name, null);

            var matcher = valueMatcher ?? ValueMatcher.All;
            var variables = Variables.Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name && matcher.Matches(line.Value))
                .ToArray();

            var lines = Lines;

            foreach (var variable in variables)
            {
                lines = lines.Remove(variable);
            }

            return (this with { Lines = lines }).CleanupSection(section, subsection);
        }

        public ConfigDocument RemoveSection(string section, string? subsection = null)
        {
            // Forces validation
            Line.CreateSection(filePath, 0, section, subsection);

            Line line;
            var lines = Lines;

            while ((line = lines
                .Where(line => line.Kind == LineKind.Section)
                .Where(SectionEquals(section, subsection))
                .FirstOrDefault()) != null)
            {
                var start = lines.IndexOf(line);
                var end = start;
                // Delete until next section
                while (++end < lines.Count && lines[end].Kind != LineKind.Section)
                    ;

                lines = lines.RemoveRange(start, end - start);
            }

            while (lines.Count > 0 && lines[0].Kind == LineKind.None)
                lines = lines.RemoveAt(0);

            while (lines.Count > 0 && lines[^1].Kind == LineKind.None)
                lines = lines.RemoveAt(lines.Count - 1);

            return this with { Lines = lines };
        }

        public ConfigDocument RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
        {
            // Forces validation
            Line.CreateSection(filePath, 0, oldSection, oldSubsection);
            Line.CreateSection(filePath, 0, newSection, newSubsection);

            Line line;
            var lines = Lines;

            while ((line = lines
                .Where(line => line.Kind == LineKind.Section)
                .Where(SectionEquals(oldSection, oldSubsection))
                .FirstOrDefault()) != null)
            {
                var os = line.Section;
                var oss = line.Subsection;
                var nl = line.WithSection(newSection, newSubsection);
                lines = lines.Replace(line, nl);

                foreach (var variable in lines
                    .Where(line => line.Kind == LineKind.Variable)
                    .Where(x => x.Section == os && oss == x.Subsection))
                {
                    lines = lines.Replace(variable, variable.WithSection(nl.Section!, nl.Subsection));
                }
            }

            return this with { Lines = lines };
        }

        /// <summary>
        /// Removes a section containing no more variables.
        /// </summary>
        ConfigDocument CleanupSection(string section, string? subsection)
        {
            if (!Variables.Where(SectionEquals(section, subsection)).Any())
            {
                var line = Lines.Where(SectionEquals(section, subsection)).FirstOrDefault();
                if (line.Kind == LineKind.Section)
                {
                    var index = Lines.IndexOf(line);
                    var lines = Lines.RemoveAt(index);

                    // Remove empty section
                    while (index < lines.Count && lines[index].Kind == LineKind.None)
                        lines = lines.RemoveAt(index);

                    return this with { Lines = lines };
                }
            }

            return this;
        }

        Func<Line, bool> SectionEquals(string section, string? subsection) =>
            x => string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) &&
                 (subsection == null && x.Subsection == null || string.Equals(x.Subsection, subsection));

        Func<string?, bool> Matches(string? regex)
            => regex == null ? _ => true :
                regex[0] == '!' ?
                    new Func<string?, bool>(v => v != null && !Regex.IsMatch(v, regex.Substring(1))) :
                    new Func<string?, bool>(v => v != null && Regex.IsMatch(v, regex));

        public IEnumerator<ConfigEntry> GetEnumerator() => Entries.GetEnumerator();

        public IEnumerable<Line> Comments => Lines.Where(line => line.Kind == LineKind.Comment);

        public IEnumerable<Line> Sections => Lines.Where(line => line.Kind == LineKind.Section);

        public IEnumerable<Line> Variables => Lines.Where(line => line.Kind == LineKind.Variable);

        public IEnumerable<ConfigEntry> Entries => Variables.Select(ToEntry);

        ConfigEntry ToEntry(Line line) => new ConfigEntry(line.Section!, line.Subsection, line.Variable!, line.Value, Level);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
