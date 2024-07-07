using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetConfig
{
    class FileConfig : Config
    {
        ConfigDocument document;

        public FileConfig(string filePath, ConfigLevel? level = null)
            : base(filePath)
            => document = ConfigDocument.FromFile(filePath, level);

        FileConfig(string filePath, ConfigDocument document) : base(filePath) => this.document = document;

        public override Config AddBoolean(string section, string? subsection, string variable, bool value)
        {
            if (value)
            {
                // Shortcut notation.
                document.Add(section, subsection, variable, null).Save();
            }
            else
            {
                document.Add(section, subsection, variable, "false").Save();
            }

            return this;
        }

        public override Config AddDateTime(string section, string? subsection, string variable, DateTime value)
        {
            document.Add(section, subsection, variable, value.ToString("O")).Save();
            return this;
        }

        public override Config AddNumber(string section, string? subsection, string variable, long value)
        {
            document.Add(section, subsection, variable, value.ToString()).Save();
            return this;
        }

        public override Config AddString(string section, string? subsection, string variable, string value)
        {
            document.Add(section, subsection, variable, value).Save();
            return this;
        }

        public override IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, string? valueRegex)
            => document.GetAll(section, subsection, variable, valueRegex);

        public override IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null)
            => document.GetAll(nameRegex, valueRegex);

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

        public override Config RemoveSection(string section, string? subsection)
        {
            document.RemoveSection(section, subsection).Save();
            return this;
        }

        public override Config RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
        {
            document.RenameSection(oldSection, oldSubsection, newSection, newSubsection).Save();
            return this;
        }

        public override Config SetAllBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
        {
            if (value)
            {
                // Shortcut notation.
                document.SetAll(section, subsection, variable, null, valueRegex).Save();
            }
            else
            {
                document.SetAll(section, subsection, variable, "false", valueRegex).Save();
            }

            return this;
        }

        public override Config SetAllDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
        {
            document.SetAll(section, subsection, variable, value.ToString("O"), valueRegex).Save();
            return this;
        }

        public override Config SetAllNumber(string section, string? subsection, string variable, long value, string? valueRegex)
        {
            document.SetAll(section, subsection, variable, value.ToString(), valueRegex).Save();
            return this;
        }

        public override Config SetAllString(string section, string? subsection, string variable, string value, string? valueRegex)
        {
            document.SetAll(section, subsection, variable, value, valueRegex).Save();
            return this;
        }

        public override Config SetBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
        {
            if (value)
            {
                // Shortcut notation.
                document.Set(section, subsection, variable, null, valueRegex).Save();
            }
            else
            {
                document.Set(section, subsection, variable, "false", valueRegex).Save();
            }

            return this;
        }

        public override Config SetDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
        {
            document.Set(section, subsection, variable, value.ToString("O"), valueRegex).Save();
            return this;
        }

        public override Config SetNumber(string section, string? subsection, string variable, long value, string? valueRegex)
        {
            document.Set(section, subsection, variable, value.ToString(), valueRegex).Save();
            return this;
        }

        public override Config SetString(string section, string? subsection, string variable, string value, string? valueRegex)
        {
            document.Set(section, subsection, variable, value, valueRegex).Save();
            return this;
        }

        public override bool TryGetBoolean(string section, string? subsection, string variable, out bool value)
        {
            var entry = document.FirstOrDefault(x =>
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
            var entry = document.FirstOrDefault(x =>
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
            var entry = document.FirstOrDefault(x =>
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
            var entry = document.FirstOrDefault(x =>
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

        public override Config Unset(string section, string? subsection, string variable)
        {
            document.Unset(section, subsection, variable).Save();
            return this;
        }

        public override Config UnsetAll(string section, string? subsection, string variable, string? valueMatcher)
        {
            document.UnsetAll(section, subsection, variable, valueMatcher).Save();
            return this;
        }

        protected override IEnumerable<ConfigEntry> GetEntries() => document;

        public override string ToString() => FilePath;
    }
}
