using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml;
using Superpower;

namespace Microsoft.DotNet
{
    internal class ConfigDocument : IEnumerable<ConfigEntry>
    {
        string filePath;

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
                else
                    level = ConfigLevel.Local;
            }

            Level = level.Value;
        }

        public static ConfigDocument FromFile(string filePath) => new ConfigDocument(filePath);

        public static ConfigDocument FromFile(string filePath, ConfigLevel level) => new ConfigDocument(filePath, level);

        public ConfigLevel Level { get; }

        public List<Line> Lines { get; } = new List<Line>();

        public void Save()
        {
            using (var writer = new StreamWriter(filePath, false))
            {
                foreach (var line in Lines)
                {
                    writer.WriteLine(line.Text);
                }
            }
        }

        public IEnumerable<ConfigEntry> GetAll(string nameRegex, string? valueRegex = null)
        {
            var nameMatches = Matches(nameRegex);
            var valueMatches = Matches(valueRegex);

            return GetEntries().Where(x => nameMatches(x.Key) && valueMatches(x.Value));
        }

        public IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string name, string? valueRegex = null)
        {
            var matches = valueRegex == null ? _ => true :
                valueRegex[0] == '!' ?
                    new Func<string?, bool>(v => !Regex.IsMatch(v, valueRegex.Substring(1))) :
                    new Func<string?, bool>(v => Regex.IsMatch(v, valueRegex));

            return FindVariables(section, subsection, name)
                .Where(x => x.Item2 != null && matches(x.Item2.Value))
                .Select(x => new ConfigEntry(section, subsection, x.Item2!.Name, x.Item2.Value, Level));
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

        public void Set(string section, string? subsection, string name, string? value, string? valueRegex = null)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(section));
            ConfigParser.Variable.Parse(ConfigTokenizer.Line.Tokenize(name));

            // Cannot modify multiple with this method. Use SetAll instead.
            if (FindVariables(section, subsection, name).Skip(1).Any())
                throw new NotSupportedException($"Multi-valued property '{new SectionLine(section, subsection)} {name}' found. Use {nameof(SetAll)} instead.");

            var matches = Matches(valueRegex);
            (SectionLine? sl, VariableLine? vl) = FindVariables(section, subsection, name).Where(x => x.Item2 != null && matches(x.Item2.Value)).FirstOrDefault();

            if (vl != null)
            {

                vl.Value = value;
                return;
            }

            int sectionIndex;

            // We didn't find an existing variable
            if (sl == null)
            {
                sectionIndex = Lines.Count;
                sl = new SectionLine(section, subsection);
                Lines.Add(sl);
            }
            else
            {
                sectionIndex = Lines.IndexOf(sl);
            }

            var varIndex = sectionIndex + 1;
            var lastSectionLine = Lines.Skip(sectionIndex + 1).Where(l => !(l is SectionLine)).FirstOrDefault();
            if (lastSectionLine != null)
            {
                varIndex = Lines.IndexOf(lastSectionLine) + 1;
            }

            vl = new VariableLine(name, value);
            Lines.Insert(varIndex, vl);
        }

        public void Unset(string section, string? subsection, string name)
        {
            (SectionLine? sl, VariableLine? vl) = FindVariables(section, subsection, name).FirstOrDefault();

            if (vl != null)
            {
                // Cannot modify multiple with this method. Use SetAll instead.
                if (FindVariables(section, subsection, name).Skip(1).Any())
                    throw new NotSupportedException($"Multi-valued property '{new SectionLine(section, subsection)} {name}' found. Use {nameof(UnsetAll)} instead.");

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

        public void SetAll(string section, string? subsection, string name, string? value, string? valueRegex = null)
        {
            ConfigParser.Section.Parse(ConfigTokenizer.Line.Tokenize(section));
            ConfigParser.Variable.Parse(ConfigTokenizer.Line.Tokenize(name));

            var matches = Matches(valueRegex);
            foreach (var variable in FindVariables(section, subsection, name).Where(x => x.Item2 != null && matches(x.Item2.Value)))
            {
                variable.Item2!.Value = value;
            }
        }

        public void UnsetAll(string section, string? subsection, string name, string? valueRegex = null)
        {
            var matches = Matches(valueRegex);
            var lines = FindVariables(section, subsection, name).Where(x => x.Item2 != null && matches(x.Item2.Value)).ToArray();

            foreach (var variable in lines)
            {
                Lines.Remove(variable.Item2!);
            }

            var sections = lines.Select(x => x.Item1).Distinct();
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
            {
                Lines.RemoveAt(0);
            }

            while (Lines.Count > 0 && Lines[Lines.Count - 1] is EmptyLine)
            {
                Lines.RemoveAt(Lines.Count - 1);
            }
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
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string? line = default;
                int index = -1;
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

                    throw new ArgumentException($"{filePath}({line},{errorPosition.Column}): {error}");
                }
            }
        }

        IEnumerable<(SectionLine, VariableLine?)> FindVariables(string section, string? subsection, string? name)
        {
            SectionLine? currentSection = null;
            VariableLine? currentVariable = null;
            foreach (var line in Lines)
            {
                switch (line)
                {
                    case SectionLine sl:
                        if (string.Equals(section, sl.Section, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(sl.Subsection, subsection))
                        {
                            currentSection = sl;
                        }
                        break;
                    case VariableLine vl:
                        if (currentSection != null && 
                            string.Equals(section, currentSection.Section, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(currentSection.Subsection, subsection) &&
                            (name == null || string.Equals(vl.Name, name)))
                        {
                            currentVariable = vl;
                            yield return (currentSection, vl);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (currentSection != null && currentVariable == null)
                yield return (currentSection, currentVariable);
        }

        Func<SectionLine, bool> Equal(string section, string? subsection) =>
            x => string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Subsection, subsection);

        Func<string?, bool> Matches(string? regex)
            => regex == null ? _ => true :
                regex[0] == '!' ?
                    new Func<string?, bool>(v => !Regex.IsMatch(v, regex.Substring(1))) :
                    new Func<string?, bool>(v => Regex.IsMatch(v, regex));

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
