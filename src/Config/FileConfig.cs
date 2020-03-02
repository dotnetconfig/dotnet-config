using System;
using System.Linq;

namespace Microsoft.DotNet
{
    internal class FileConfig : Config
    {
        static readonly ValueSerializer serializer = new ValueSerializer();
        ConfigDocument doc;

        public FileConfig(string filePath) : base(filePath) => doc = ConfigDocument.FromFile(filePath);

        public override void Set<T>(string section, string? subsection, string variable, T value)
        {
            if (value is bool b && b == true)
            {
                // Shortcut notation.
                doc.Set(section, subsection, variable, null);
                doc.Save();
            }
            else
            {
                doc.Set(section, subsection, variable, serializer.Serialize(value));
                doc.Save();
            }
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
                value = serializer.Deserialize<T>(entry.Value);
                return true;
            }
            
            return false;
        }
    }
}
