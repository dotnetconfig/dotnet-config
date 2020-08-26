using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Superpower;

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
            }

            Level = level;
        }

        public static ConfigDocument FromFile(string filePath, ConfigLevel? level = null) => new ConfigDocument(filePath, level);

        public ConfigLevel? Level { get; }

        public List<Line> Lines { get; } = new List<Line>();

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using var writer = new StreamWriter(filePath, false);
            foreach (var line in Lines)
                writer.WriteLine(line.Text);
        }

        public IEnumerable<ConfigEntry> GetAll(string nameRegex, string? valueRegex = null)
        {
            var nameMatches = Matches(nameRegex);
            var valueMatches = Matches(valueRegex);

            return GetEntries().Where(x => nameMatches(x.Key) && valueMatches(x.RawValue));
        }

        public IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string name, ValueMatcher? valueMatcher = null)
        {
            var matcher = valueMatcher ?? ValueMatcher.All;
            return FindVariables(section, subsection, name)
                .SelectMany(x => x.Variables
                    .Where(v => v != VariableLine.Null && matcher.Matches(v.Value))
                    .Select(v => new ConfigEntry(section, subsection, v.Name, v.Value, Level)));
        }

        public void Add(string section, string? subsection, string name, string? value)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(section));
            ConfigParser.Variable.Parse(ConfigTokenizer.Line.Tokenize(name));

            var sl = Lines.OfType<SectionLine>().FirstOrDefault(Equal(section, subsection));
            int index;
            if (sl == null)
            {
                index = Lines.Count;
                sl = new SectionLine(section, subsection);
                Lines.Add(sl);
            }
            else
            {
                index = Lines.IndexOf(sl);
            }

            void FindEnd()
            {
                while (++index < Lines.Count)
                {
                    var next = Lines[index];
                    switch (next)
                    {
                        case EmptyLine _:
                            return;
                        case SectionLine _:
                            return;
                        default:
                            break;
                    }
                }
            };

            FindEnd();
            Lines.Insert(index, new VariableLine(name, value));
        }

        public ConfigDocument Set(string section, string? subsection, string name, string? value = null, ValueMatcher? valueMatcher = null)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(section));
            ConfigParser.Variable.Parse(ConfigTokenizer.Line.Tokenize(name));

            // Cannot modify multiple with this method. Use SetAll instead.
            if (FindVariables(section, subsection, name).SelectMany(s => s.Variables.Where(v => v != VariableLine.Null).Select(v => (s, v))).Skip(1).Any())
                throw new NotSupportedException($"Multi-valued property '{new SectionLine(section, subsection)} {name}' found. Use {nameof(SetAll)} instead.");

            var matcher = valueMatcher ?? ValueMatcher.All;

            (SectionLine sl, VariableLine vl) = FindVariables(section, subsection, name)
                .SelectMany(s => s.Variables.Select(v => (section: s.Section, variable: v)))
                .Where(x => matcher.Matches(x.variable.Value)).FirstOrDefault();

            if (vl != VariableLine.Null && vl != null)
            {
                vl.Value = value;
                return this;
            }

            int index;

            // We didn't find an existing section
            if (sl == null)
            {
                index = Lines.Count;
                sl = new SectionLine(section, subsection);
                Lines.Add(sl);
            }
            else
            {
                index = Lines.IndexOf(sl);
            }

            var count = 0;
            void FindEnd()
            {
                while (index + ++count < Lines.Count)
                {
                    var next = Lines[index + count];
                    switch (next)
                    {
                        case EmptyLine _:
                            return;
                        case SectionLine _:
                            return;
                        default:
                            break;
                    }
                }
            };

            FindEnd();
            Lines.Insert(index + count, new VariableLine(name, value));
            return this;
        }

        public void Unset(string section, string? subsection, string name)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(section));
            ConfigParser.Variable.Parse(ConfigTokenizer.Line.Tokenize(name));

            // Cannot modify multiple with this method. Use SetAll instead.
            if (FindVariables(section, subsection, name).SelectMany(s => s.Variables.Where(v => v != VariableLine.Null).Select(v => (s, v))).Skip(1).Any())
                throw new NotSupportedException($"Multi-valued property '{new SectionLine(section, subsection)} {name}' found. Use {nameof(UnsetAll)} instead.");

            (SectionLine sl, VariableLine vl) = FindVariables(section, subsection, name)
                .SelectMany(s => s.Variables.Where(v => v != VariableLine.Null).Select(v => (section: s.Section, variable: v)))
                .FirstOrDefault();

            if (vl != null)
            {
                Lines.RemoveAt(Lines.IndexOf(vl));

                var index = Lines.IndexOf(sl);
                // If it's the last section on the file, we can safely remove it.
                if (Lines.Count == index + 1)
                {
                    Lines.Remove(sl);
                }
                else
                {
                    // remove empty section
                    while (index++ < Lines.Count)
                    {
                        var next = Lines[index];
                        switch (next)
                        {
                            case VariableLine _:
                                return;
                            case CommentLine _:
                                return;
                            case SectionLine _:
                                Lines.Remove(sl);
                                return;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public void SetAll(string section, string? subsection, string name, string? value, ValueMatcher? valueMatcher = null)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(section));
            ConfigParser.Variable.Parse(ConfigTokenizer.Line.Tokenize(name));

            var matcher = valueMatcher ?? ValueMatcher.All;
            foreach (var variable in FindVariables(section, subsection, name)
                .SelectMany(s => s.Variables.Select(v => (section: s.Section, variable: v)))
                .Where(x => matcher.Matches(x.variable.Value)))
            {
                variable.variable.Value = value;
            }
        }

        public void UnsetAll(string section, string? subsection, string name, ValueMatcher? valueMatcher = null)
        {
            var matcher = valueMatcher ?? ValueMatcher.All;
            var lines = FindVariables(section, subsection, name)
                .SelectMany(s => s.Variables.Select(v => (section: s.Section, variable: v)))
                .Where(x => matcher.Matches(x.variable.Value)).ToArray();

            foreach (var variable in lines)
            {
                Lines.Remove(variable.variable);
            }

            var sections = lines.Select(x => x.section).Distinct();
            foreach (var sl in sections)
            {
                var index = Lines.IndexOf(sl);
                // If it's the last section on the file, we can safely remove it.
                if (Lines.Count == index + 1)
                {
                    Lines.Remove(sl);
                }
                else
                {
                    void RemoveEmpty(int index)
                    {
                        while (index++ < Lines.Count)
                        {
                            var next = Lines[index];
                            switch (next)
                            {
                                case VariableLine _:
                                    return;
                                case CommentLine _:
                                    return;
                                case SectionLine _:
                                    Lines.Remove(sl);
                                    return;
                                default:
                                    break;
                            }
                        }
                    };
                    RemoveEmpty(index);
                }
            }
        }

        public void RemoveSection(string section, string? subsection = null)
        {
            SectionLine sl;
            while ((sl = Lines.OfType<SectionLine>().FirstOrDefault(Equal(section, subsection))) != null)
            {
                var index = Lines.IndexOf(sl);
                var count = 0;
                void FindEnd()
                {
                    while (index + ++count < Lines.Count)
                    {
                        var next = Lines[index + count];
                        switch (next)
                        {
                            case EmptyLine _:
                                return;
                            case SectionLine _:
                                return;
                            default:
                                break;
                        }
                    }
                };

                FindEnd();
                Lines.RemoveRange(index, count);
            }

            while (Lines.Count > 0 && Lines[0] is EmptyLine)
                Lines.RemoveAt(0);

            while (Lines.Count > 0 && Lines[^1] is EmptyLine)
                Lines.RemoveAt(Lines.Count - 1);
        }

        public void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(oldSection));
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(newSection));

            SectionLine sl;
            while ((sl = Lines.OfType<SectionLine>().FirstOrDefault(Equal(oldSection, oldSubsection))) != null)
            {
                sl.Section = newSection;
                sl.Subsection = newSubsection;
            }
        }

        void Load()
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            string? line = default;
            var index = -1;
            while (!reader.EndOfStream && (line = reader.ReadLine()) != null)
            {
                index++;
                if (line.Length == 0)
                {
                    Lines.Add(new EmptyLine());
                    continue;
                }

                if (ConfigParser.TryParse(line, out var result, out var error, out var errorPosition) && result != null)
                {
                    Lines.Add(result);
                    continue;
                }

                throw new ArgumentException($"{filePath}({index},{errorPosition.Column}): {error}");
            }
        }

        IEnumerable<(SectionLine Section, IEnumerable<VariableLine> Variables)> FindVariables(string section, string? subsection, string? name)
        {
            SectionLine? currentSection = null;
            List<VariableLine>? variables = default;
            foreach (var line in Lines)
            {
                switch (line)
                {
                    case SectionLine sl:
                        if (string.Equals(section, sl.Section, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(sl.Subsection, subsection))
                        {
                            currentSection = sl;
                            variables = new List<VariableLine>();
                            break;
                        }
                        // we changed section and populated variables, return both
                        if (currentSection != null && variables != null)
                        {
                            yield return (currentSection, variables.Count == 0 ? new[] { VariableLine.Null } : (IEnumerable<VariableLine>)variables);
                        }
                        // reset
                        currentSection = null;
                        variables = null;
                        break;
                    case VariableLine vl:
                        if (currentSection != null && 
                            string.Equals(section, currentSection.Section, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(currentSection.Subsection, subsection) &&
                            (name == null || string.Equals(vl.Name, name)))
                        {
                            variables!.Add(vl);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (currentSection != null)
                yield return (currentSection, variables == null || variables.Count == 0 ? new[] { VariableLine.Null } : (IEnumerable<VariableLine>)variables);
        }

        Func<SectionLine, bool> Equal(string section, string? subsection) =>
            x => string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Subsection, subsection);

        Func<string?, bool> Matches(string? regex)
            => regex == null ? _ => true :
                regex[0] == '!' ?
                    new Func<string?, bool>(v => v != null && !Regex.IsMatch(v, regex.Substring(1))) :
                    new Func<string?, bool>(v => v != null && Regex.IsMatch(v, regex));

        public IEnumerator<ConfigEntry> GetEnumerator() => GetEntries().GetEnumerator();

        IEnumerable<ConfigEntry> GetEntries()
        {
            SectionLine? section = null;
            foreach (var line in Lines)
            {
                if (line is SectionLine sl)
                    section = sl;
                else if (line is VariableLine variable && section != null)
                    yield return new ConfigEntry(section.Section, section.Subsection, variable.Name, variable.Value ?? null, Level);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
