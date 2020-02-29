using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Superpower;

namespace Microsoft.DotNet
{
    internal class ConfigDocument : IEnumerable<ConfigEntry>
    {
        string filePath;
        ConfigLevel level;

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

            this.level = level.Value;
        }

        public static ConfigDocument FromFile(string filePath) => new ConfigDocument(filePath);

        public static ConfigDocument FromFile(string filePath, ConfigLevel level) => new ConfigDocument(filePath, level);

        public List<Line> Lines { get; } = new List<Line>();

        public void Save()
        {
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
                        Lines.Add(new Line(""));
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

        public IEnumerator<ConfigEntry> GetEnumerator() => GetEntries().GetEnumerator();

        IEnumerable<ConfigEntry> GetEntries()
        {
            SectionLine? section = null;
            foreach (var line in Lines)
            {
                if (line is SectionLine sl)
                    section = sl;
                else if (line is VariableLine vl && section != null)
                    yield return new ConfigEntry(section.Section, section.Subsection, vl.Name, vl.Value ?? null, level);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
