using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetConfig
{
    internal class ConfigDocument : IEnumerable<ConfigEntry>
    {
        readonly string filePath;

        ConfigDocument(string filePath, ConfigLevel? level = null)
        {
            this.filePath = filePath;
            if (File.Exists(filePath))
                Load();

            if (level == null)
            {
                if (filePath == Config.GlobalLocation)
                    level = ConfigLevel.Global;
                else if (filePath == Config.SystemLocation)
                    level = ConfigLevel.System;
                else if (filePath.EndsWith(Config.UserExtension, StringComparison.Ordinal))
                    level = ConfigLevel.Local;
            }

            Level = level;
        }

        public static ConfigDocument FromFile(string filePath, ConfigLevel? level = null) => new ConfigDocument(filePath, level);

        public ConfigLevel? Level { get; }

        public List<Line> Lines { get; private set; } = new List<Line>();

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using var writer = new StreamWriter(filePath, false);
            foreach (var line in Lines)
            {
                writer.WriteLine(line.LineText);
            }
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

        public void Add(string section, string? subsection, string name, string? value)
        {
            var sectionLine = Lines.Where(line => line.Kind == LineKind.Section).FirstOrDefault(SectionEquals(section, subsection));

            if (sectionLine == null)
            {
                sectionLine = Line.CreateSection(filePath, Lines.Count, section, subsection);
                Lines.Add(sectionLine);
                Lines.Add(Line.CreateVariable(filePath, Lines.Count, sectionLine.Section, sectionLine.Subsection, name, value));
            }
            else
            {
                var index = Lines.IndexOf(sectionLine);
                // Move to the last variable
                while (++index < Lines.Count && Lines[index].Kind == LineKind.Variable)
                    ;

                Lines.Insert(index, Line.CreateVariable(filePath, index, sectionLine.Section, sectionLine.Subsection, name, value));
            }
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
                variable.WithValue(value);
            }
            else
            {
                Add(section, subsection, name, value);
            }

            return this;
        }

        public void Unset(string section, string? subsection, string name)
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
                Lines.Remove(variable);
                CleanupSection(section, subsection);
            }
        }

        public void SetAll(string section, string? subsection, string name, string? value, ValueMatcher? valueMatcher = null)
        {
            // Forces validation
            var sl = Line.CreateSection(filePath, 0, section, subsection);
            Line.CreateVariable(filePath, 0, sl.Section, sl.Subsection, name, value);

            var matcher = valueMatcher ?? ValueMatcher.All;

            foreach (var variable in Variables.Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name && matcher.Matches(line.Value))
                .ToArray())
            {
                variable.WithValue(value);
            }
        }

        public void UnsetAll(string section, string? subsection, string name, ValueMatcher? valueMatcher = null)
        {
            // Forces validation
            var sl = Line.CreateSection(filePath, 0, section, subsection);
            Line.CreateVariable(filePath, 0, sl.Section, sl.Subsection, name, null);

            var matcher = valueMatcher ?? ValueMatcher.All;
            var lines = Variables.Where(SectionEquals(section, subsection))
                .Where(line => line.Variable == name && matcher.Matches(line.Value))
                .ToArray();

            foreach (var variable in lines)
            {
                Lines.Remove(variable);
            }

            CleanupSection(section, subsection);
        }

        public void RemoveSection(string section, string? subsection = null)
        {
            // Forces validation
            Line.CreateSection(filePath, 0, section, subsection);

            Line line;
            while ((line = Lines
                .Where(line => line.Kind == LineKind.Section)
                .Where(SectionEquals(section, subsection))
                .FirstOrDefault()) != null)
            {
                var start = Lines.IndexOf(line);
                var end = start;
                // Delete until next section
                while (++end < Lines.Count && Lines[end].Kind != LineKind.Section)
                    ;

                Lines.RemoveRange(start, end - start);
            }

            while (Lines.Count > 0 && Lines[0].Kind == LineKind.None)
                Lines.RemoveAt(0);

            while (Lines.Count > 0 && Lines[^1].Kind == LineKind.None)
                Lines.RemoveAt(Lines.Count - 1);
        }

        public void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
        {
            // Forces validation
            Line.CreateSection(filePath, 0, oldSection, oldSubsection);

            Line line;
            while ((line = Lines
                .Where(line => line.Kind == LineKind.Section)
                .Where(SectionEquals(oldSection, oldSubsection))
                .FirstOrDefault()) != null)
            {
                var os = line.Section;
                var oss = line.Subsection;
                var nl = line.WithSection(newSection, newSubsection);

                foreach (var variable in Variables.Where(x => x.Section == os && oss == x.Subsection))
                {
                    variable.WithSection(nl.Section!, nl.Subsection);
                }
            }
        }

        /// <summary>
        /// Removes a section containing no more variables.
        /// </summary>
        void CleanupSection(string section, string? subsection)
        {
            if (!Variables.Where(SectionEquals(section, subsection)).Any())
            {
                var line = Lines.Where(SectionEquals(section, subsection)).FirstOrDefault();
                if (line.Kind == LineKind.Section)
                {
                    var index = Lines.IndexOf(line);
                    Lines.RemoveAt(index);
                    // Remove empty section
                    while (index < Lines.Count && Lines[index].Kind == LineKind.None)
                        Lines.RemoveAt(index);
                }
            }
        }

        void Load()
        {
            using var reader = new ConfigReader(filePath);
            Lines = reader.ReadAllLines().ToList();

            // throw for lines with errors?
            //throw new ArgumentException($"{filePath}({index},{errorPosition.Column}): {error}");
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
