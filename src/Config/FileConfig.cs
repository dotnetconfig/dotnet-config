using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet
{
    internal class FileConfig : Config
    {
        static readonly ValueSerializer serializer = new ValueSerializer();
        ConfigDocument doc;

        public FileConfig(string filePath) : base(filePath)
        {
            doc = ConfigDocument.FromFile(filePath);
        }

        public ConfigLevel Level => doc.Level;

        public override void Add<T>(string section, string? subsection, string variable, T value)
        {
            if (value is bool b && b == true)
            {
                // Shortcut notation.
                doc.Add(section, subsection, variable, null);
                doc.Save();
            }
            else
            {
                doc.Add(section, subsection, variable, serializer.Serialize(value));
                doc.Save();
            }
        }

        public override IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, string? valueRegex = null)
            => doc.GetAll(section, subsection, variable, valueRegex);

        public override IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null)
            => doc.GetAll(nameRegex, valueRegex);

        public override void Set<T>(string section, string? subsection, string variable, T value, string? valueRegex = null)
        {
            if (value is bool b && b == true)
            {
                // Shortcut notation.
                doc.Set(section, subsection, variable, null, valueRegex);
                doc.Save();
            }
            else
            {
                doc.Set(section, subsection, variable, serializer.Serialize(value), valueRegex);
                doc.Save();
            }
        }

        public override void SetAll<T>(string section, string? subsection, string variable, T value, string? valueRegex = null)
        {
            if (value is bool b && b == true)
            {
                // Shortcut notation.
                doc.SetAll(section, subsection, variable, null, valueRegex);
                doc.Save();
            }
            else
            {
                doc.SetAll(section, subsection, variable, serializer.Serialize(value), valueRegex);
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

        public override void Unset(string section, string? subsection, string variable)
        {
            doc.Unset(section, subsection, variable);
            doc.Save();
        }

        public override void UnsetAll(string section, string? subsection, string variable, string? valueRegex = null)
        {
            doc.UnsetAll(section, subsection, variable, valueRegex);
            doc.Save();
        }

        protected override IEnumerable<ConfigEntry> GetEntries() => doc;
    }
}
