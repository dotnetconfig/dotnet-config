using System;
using System.Linq;

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

            var entry = doc.FirstOrDefault(x =>
                string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Subsection, subsection) &&
                variable.Equals(x.Name));

            if (entry != null)
            {
                value = ConvertTo<T>(entry.Value);
                return true;
            }
            
            return false;
        }
    }
}
