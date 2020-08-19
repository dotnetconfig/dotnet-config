using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet
{
    internal class AggregateConfig : Config
    {
        readonly List<Config> configs;

        public AggregateConfig(List<Config> configs)
            : base(configs.First().FilePath)
        {
            this.configs = configs;
        }

        public override void AddBoolean(string section, string? subsection, string variable, bool value)
            => configs.First().AddBoolean(section, subsection, variable, value);

        public override void AddDateTime(string section, string? subsection, string variable, DateTime value)
            => configs.First().AddDateTime(section, subsection, variable, value);

        public override void AddNumber(string section, string? subsection, string variable, long value)
            => configs.First().AddNumber(section, subsection, variable, value);

        public override void AddString(string section, string? subsection, string variable, string value)
            => configs.First().AddString(section, subsection, variable, value);

        public override IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, ValueMatcher valueMatcher)
            => configs.SelectMany(x => x.GetAll(section, subsection, variable, valueMatcher));

        public override IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null)
            => configs.SelectMany(x => x.GetRegex(nameRegex, valueRegex));

        public override void RemoveSection(string section, string? subsection = null)
            => configs.First().RemoveSection(section, subsection);

        public override void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
            => configs.First().RenameSection(oldSection, oldSubsection, newSection, newSubsection);

        public override void SetAllBoolean(string section, string? subsection, string variable, bool value, ValueMatcher valueMatcher)
            => configs.First().SetAllBoolean(section, subsection, variable, value, valueMatcher);

        public override void SetAllDateTime(string section, string? subsection, string variable, DateTime value, ValueMatcher valueMatcher)
            => configs.First().SetAllDateTime(section, subsection, variable, value, valueMatcher);

        public override void SetAllNumber(string section, string? subsection, string variable, long value, ValueMatcher valueMatcher)
            => configs.First().SetAllNumber(section, subsection, variable, value, valueMatcher);

        public override void SetAllString(string section, string? subsection, string variable, string value, ValueMatcher valueMatcher)
            => configs.First().SetAllString(section, subsection, variable, value, valueMatcher);

        public override void SetBoolean(string section, string? subsection, string variable, bool value, ValueMatcher valueMatcher)
            => configs.First().SetBoolean(section, subsection, variable, value, valueMatcher);

        public override void SetDateTime(string section, string? subsection, string variable, DateTime value, ValueMatcher valueMatcher)
            => configs.First().SetDateTime(section, subsection, variable, value, valueMatcher);

        public override void SetNumber(string section, string? subsection, string variable, long value, ValueMatcher valueMatcher)
            => configs.First().SetNumber(section, subsection, variable, value, valueMatcher);

        public override void SetString(string section, string? subsection, string variable, string value, ValueMatcher valueMatcher)
            => configs.First().SetString(section, subsection, variable, value, valueMatcher);

        public override bool TryGetBoolean(string section, string? subsection, string variable, out bool value)
        {
            foreach (var config in configs)
            {
                if (config.TryGetBoolean(section, subsection, variable, out value))
                    return true;
            }

            value = false;
            return false;
        }

        public override bool TryGetDateTime(string section, string? subsection, string variable, out DateTime value)
        {
            foreach (var config in configs)
            {
                if (config.TryGetDateTime(section, subsection, variable, out value))
                    return true;
            }

            value = DateTime.MinValue;
            return false;
        }

        public override bool TryGetNumber(string section, string? subsection, string variable, out long value)
        {
            foreach (var config in configs)
            {
                if (config.TryGetNumber(section, subsection, variable, out value))
                    return true;
            }

            value = 0;
            return false;
        }

        public override bool TryGetString(string section, string? subsection, string variable, out string value)
        {
            foreach (var config in configs)
            {
                if (config.TryGetString(section, subsection, variable, out value))
                    return true;
            }

            value = "";
            return false;
        }

        public override void Unset(string section, string? subsection, string variable)
            => configs.First().Unset(section, subsection, variable);

        public override void UnsetAll(string section, string? subsection, string variable, ValueMatcher valueMatcher)
            => configs.First().UnsetAll(section, subsection, variable, valueMatcher);

        protected override IEnumerable<ConfigEntry> GetEntries() => configs.SelectMany(x => x);
    }
}
