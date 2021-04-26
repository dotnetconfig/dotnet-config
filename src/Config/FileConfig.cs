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
                return new FileConfig(FilePath,
                    document.Add(section, subsection, variable, null)
                            .Save());
            }
            else
            {
                return new FileConfig(FilePath,
                    document.Add(section, subsection, variable, "false")
                            .Save());
            }
        }

        public override Config AddDateTime(string section, string? subsection, string variable, DateTime value)
            => new FileConfig(FilePath, document
                .Add(section, subsection, variable, value.ToString("O"))
                .Save());

        public override Config AddNumber(string section, string? subsection, string variable, long value)
            => new FileConfig(FilePath, document
                .Add(section, subsection, variable, value.ToString())
                .Save());

        public override Config AddString(string section, string? subsection, string variable, string value)
            => new FileConfig(FilePath, document
                .Add(section, subsection, variable, value)
                .Save());

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
            => new FileConfig(FilePath, document
                .RemoveSection(section, subsection)
                .Save());

        public override Config RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
            => new FileConfig(FilePath, document
                .RenameSection(oldSection, oldSubsection, newSection, newSubsection)
                .Save());

        public override Config SetAllBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
        {
            if (value)
            {
                // Shortcut notation.
                return new FileConfig(FilePath, document
                    .SetAll(section, subsection, variable, null, valueRegex)
                    .Save());
            }
            else
            {
                return new FileConfig(FilePath, document
                    .SetAll(section, subsection, variable, "false", valueRegex)
                    .Save());
            }
        }

        public override Config SetAllDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
            => new FileConfig(FilePath, document
                .SetAll(section, subsection, variable, value.ToString("O"), valueRegex)
                .Save());

        public override Config SetAllNumber(string section, string? subsection, string variable, long value, string? valueRegex)
            => new FileConfig(FilePath, document
                .SetAll(section, subsection, variable, value.ToString(), valueRegex)
                .Save());

        public override Config SetAllString(string section, string? subsection, string variable, string value, string? valueRegex)
            => new FileConfig(FilePath, document
                .SetAll(section, subsection, variable, value, valueRegex)
                .Save());

        public override Config SetBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
        {
            if (value)
            {
                // Shortcut notation.
                return new FileConfig(FilePath, document
                    .Set(section, subsection, variable, null, valueRegex)
                    .Save());
            }
            else
            {
                return new FileConfig(FilePath, document
                    .Set(section, subsection, variable, "false", valueRegex)
                    .Save());
            }
        }

        public override Config SetDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
            => new FileConfig(FilePath, document
                .Set(section, subsection, variable, value.ToString("O"), valueRegex)
                .Save());

        public override Config SetNumber(string section, string? subsection, string variable, long value, string? valueRegex)
            => new FileConfig(FilePath, document
                .Set(section, subsection, variable, value.ToString(), valueRegex)
                .Save());

        public override Config SetString(string section, string? subsection, string variable, string value, string? valueRegex)
            => new FileConfig(FilePath, document
                .Set(section, subsection, variable, value, valueRegex)
                .Save());

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
            => new FileConfig(FilePath, document
                .Unset(section, subsection, variable)
                .Save());

        public override Config UnsetAll(string section, string? subsection, string variable, string? valueMatcher)
            => new FileConfig(FilePath, document
                .UnsetAll(section, subsection, variable, valueMatcher)
                .Save());

        protected override IEnumerable<ConfigEntry> GetEntries() => document;

        public override string ToString() => FilePath;
    }
}
