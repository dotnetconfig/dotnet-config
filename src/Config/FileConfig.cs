using System;

namespace Microsoft.DotNet
{
    internal class FileConfig : Config
    {
        ConfigDocument doc;

        public FileConfig(string filePath) => doc = ConfigDocument.FromFile(filePath);

        public override void Set<T>(string section, string? subsection, string variable, T value)
        {
        }

        public override bool TryGet<T>(string section, string? subsection, string variable, out T value)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.

            SectionLine? currentSection = default;
            foreach (var line in doc)
            {
                if (line is SectionLine sectionLine)
                {
                    currentSection = sectionLine;
                    continue;
                }
                else if (line is VariableLine variableLine)
                {
                    if (string.Equals(section, currentSection.Section, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(currentSection.Subsection, subsection) &&
                        variable.Equals(variableLine.Name))
                    {
                        value = ConvertTo<T>(variableLine.Value);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
