using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetConfig
{
    internal class FileConfig : Config
    {
        ConfigDocument doc;

        public FileConfig(string filePath, ConfigLevel? level = null) 
            : base(filePath) 
            => doc = ConfigDocument.FromFile(filePath, level);

        public override void AddBoolean(string section, string? subsection, string variable, bool value)
        {
            if (value)
            {
                // Shortcut notation.
                doc.Add(section, subsection, variable, null);
                doc.Save();
            }
            else
            {
                doc.Add(section, subsection, variable, "false");
                doc.Save();
            }
        }

        public override void AddDateTime(string section, string? subsection, string variable, DateTime value)
        {
            doc.Add(section, subsection, variable, value.ToString("O"));
            doc.Save();
        }

        public override void AddNumber(string section, string? subsection, string variable, long value)
        {
            doc.Add(section, subsection, variable, value.ToString());
            doc.Save();
        }

        public override void AddString(string section, string? subsection, string variable, string value)
        {
            doc.Add(section, subsection, variable, value);
            doc.Save();
        }

        public override IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, string? valueRegex)
            => doc.GetAll(section, subsection, variable, valueRegex);

        public override IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null)
            => doc.GetAll(nameRegex, valueRegex);

        public override string? GetNormalizedPath(string section, string? subsection, string variable)
        {
            if (!TryGetString(section, subsection, variable, out var value))
                return null;

            // For Windows, FileInfo.FullName takes care of converting \ to / already.
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                value = value.Replace('\\', '/');

            if (Path.IsPathRooted(value))
                return new FileInfo(value).FullName;

            return new FileInfo(Path.Combine(Path.GetDirectoryName(FilePath), value)).FullName;
        }

        public override void RemoveSection(string section, string? subsection)
        {
            doc.RemoveSection(section, subsection);
            doc.Save();
        }

        public override void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
        {
            doc.RenameSection(oldSection, oldSubsection, newSection, newSubsection);
            doc.Save();
        }

        public override void SetAllBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
        {
            if (value)
            {
                // Shortcut notation.
                doc.SetAll(section, subsection, variable, null, valueRegex);
                doc.Save();
            }
            else
            {
                doc.SetAll(section, subsection, variable, "false", valueRegex);
                doc.Save();
            }
        }

        public override void SetAllDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
        {
            doc.SetAll(section, subsection, variable, value.ToString("O"), valueRegex);
            doc.Save();
        }

        public override void SetAllNumber(string section, string? subsection, string variable, long value, string? valueRegex)
        {
            doc.SetAll(section, subsection, variable, value.ToString(), valueRegex);
            doc.Save();
        }

        public override void SetAllString(string section, string? subsection, string variable, string value, string? valueRegex)
        {
            doc.SetAll(section, subsection, variable, value, valueRegex);
            doc.Save();
        }

        public override void SetBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
        {
            if (value)
            {
                // Shortcut notation.
                doc.Set(section, subsection, variable, null, valueRegex);
                doc.Save();
            }
            else
            {
                doc.Set(section, subsection, variable, "false", valueRegex);
                doc.Save();
            }
        }

        public override void SetDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
        {
            doc.Set(section, subsection, variable, value.ToString("O"), valueRegex);
            doc.Save();
        }

        public override void SetNumber(string section, string? subsection, string variable, long value, string? valueRegex)
        {
            doc.Set(section, subsection, variable, value.ToString(), valueRegex);
            doc.Save();
        }

        public override void SetString(string section, string? subsection, string variable, string value, string? valueRegex)
        {
            doc.Set(section, subsection, variable, value, valueRegex);
            doc.Save();
        }

        public override bool TryGetBoolean(string section, string? subsection, string variable, out bool value)
        {
            var entry = doc.FirstOrDefault(x =>
                string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Subsection, subsection) &&
                variable.Equals(x.Variable));

            if (entry == null)
            {
                value = false;
                return false;
            }

            value = entry.GetBoolean();
            return true;
        }

        public override bool TryGetDateTime(string section, string? subsection, string variable, out DateTime value)
        {
            var entry = doc.FirstOrDefault(x =>
                string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Subsection, subsection) &&
                variable.Equals(x.Variable));

            if (entry == null)
            {
                value = DateTime.MinValue;
                return false;
            }

            value = entry.GetDateTime();
            return true;
        }

        public override bool TryGetNumber(string section, string? subsection, string variable, out long value)
        {
            var entry = doc.FirstOrDefault(x =>
                string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Subsection, subsection) &&
                variable.Equals(x.Variable));

            if (entry == null)
            {
                value = 0;
                return false;
            }

            value = entry.GetNumber();
            return true;
        }

        public override bool TryGetString(string section, string? subsection, string variable, out string value)
        {
            var entry = doc.FirstOrDefault(x =>
                string.Equals(section, x.Section, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Subsection, subsection) &&
                variable.Equals(x.Variable));

            if (entry == null)
            {
                value = "";
                return false;
            }

            value = entry.RawValue ?? throw new ArgumentNullException(entry.Key);
            return true;
        }

        public override void Unset(string section, string? subsection, string variable)
        {
            doc.Unset(section, subsection, variable);
            doc.Save();
        }

        public override void UnsetAll(string section, string? subsection, string variable, string? valueMatcher)
        {
            doc.UnsetAll(section, subsection, variable, valueMatcher);
            doc.Save();
        }

        protected override IEnumerable<ConfigEntry> GetEntries() => doc;
    }
}
