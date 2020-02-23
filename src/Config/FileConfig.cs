using System;
using System.IO;
using Superpower;

class FileConfig : Config
{
    string filePath;

    public FileConfig(string filePath)
    {
        this.filePath = filePath;
    }

    public override bool TryGet<T>(string section, string? subsection, string variable, out T value)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
        string? currentSection = default;
        string? currentSubsection = default;
        using (var stream = File.OpenRead(filePath))
        using (var reader = new StreamReader(stream))
        {
            string? line = default;
            while (!reader.EndOfStream && (line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0)
                    continue;

                var sectionResult = SectionParser.TryParse(line);
                var variableResult = VariableParser.TryParse(line);

                if (sectionResult.HasValue)
                {
                    currentSection = sectionResult.Value.section;
                    currentSubsection = sectionResult.Value.subsection;
                }
                else if (variableResult.HasValue)
                {
                    if (string.Equals(section, currentSection, StringComparison.OrdinalIgnoreCase) && 
                        string.Equals(currentSubsection, subsection) &&
                        variable.Equals(variableResult.Value.name))
                    {
                        value = ConvertTo<T>(variableResult.Value.value);
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
