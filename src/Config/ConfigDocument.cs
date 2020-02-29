using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Superpower;

namespace Microsoft.DotNet
{
    public class ConfigDocument : IEnumerable<Line>
    {
        string filePath;
        List<Line> lines = new List<Line>();

        ConfigDocument(string filePath)
        {
            this.filePath = filePath;
            if (File.Exists(filePath))
                Load();
        }

        public static ConfigDocument FromFile(string filePath)
        {
            return new ConfigDocument(filePath);
        }

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
                        lines.Add(new Line(""));
                        continue;
                    }

                    if (ConfigParser.TryParse(line, out var result, out var error, out var errorPosition) && result != null)
                    {
                        lines.Add(result);
                        continue;
                    }

                    throw new ArgumentException($"{filePath}({line},{errorPosition.Column}): {error}");
                }
            }
        }

        public IEnumerator<Line> GetEnumerator() => lines.AsReadOnly().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
