﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.DotNet
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

        public override IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, ValueMatcher valueMatcher)
            => doc.GetAll(section, subsection, variable, valueMatcher);

        public override IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null)
            => doc.GetAll(nameRegex, valueRegex);

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

        public override void SetAllBoolean(string section, string? subsection, string variable, bool value, ValueMatcher valueMatcher)
        {
            if (value)
            {
                // Shortcut notation.
                doc.SetAll(section, subsection, variable, null, valueMatcher);
                doc.Save();
            }
            else
            {
                doc.SetAll(section, subsection, variable, "false", valueMatcher);
                doc.Save();
            }
        }

        public override void SetAllDateTime(string section, string? subsection, string variable, DateTime value, ValueMatcher valueMatcher)
        {
            doc.SetAll(section, subsection, variable, value.ToString("O"), valueMatcher);
            doc.Save();
        }

        public override void SetAllNumber(string section, string? subsection, string variable, long value, ValueMatcher valueMatcher)
        {
            doc.SetAll(section, subsection, variable, value.ToString(), valueMatcher);
            doc.Save();
        }

        public override void SetAllString(string section, string? subsection, string variable, string value, ValueMatcher valueMatcher)
        {
            doc.SetAll(section, subsection, variable, value, valueMatcher);
            doc.Save();
        }

        public override void SetBoolean(string section, string? subsection, string variable, bool value, ValueMatcher valueMatcher)
        {
            if (value)
            {
                // Shortcut notation.
                doc.Set(section, subsection, variable, null, valueMatcher);
                doc.Save();
            }
            else
            {
                doc.Set(section, subsection, variable, "false", valueMatcher);
                doc.Save();
            }
        }

        public override void SetDateTime(string section, string? subsection, string variable, DateTime value, ValueMatcher valueMatcher)
        {
            doc.Set(section, subsection, variable, value.ToString("O"), valueMatcher);
            doc.Save();
        }

        public override void SetNumber(string section, string? subsection, string variable, long value, ValueMatcher valueMatcher)
        {
            doc.Set(section, subsection, variable, value.ToString(), valueMatcher);
            doc.Save();
        }

        public override void SetString(string section, string? subsection, string variable, string value, ValueMatcher valueMatcher)
        {
            doc.Set(section, subsection, variable, value, valueMatcher);
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

        public override void UnsetAll(string section, string? subsection, string variable, ValueMatcher valueMatcher)
        {
            doc.UnsetAll(section, subsection, variable, valueMatcher);
            doc.Save();
        }

        protected override IEnumerable<ConfigEntry> GetEntries() => doc;
    }
}
