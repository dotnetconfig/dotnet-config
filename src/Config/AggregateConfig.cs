using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetConfig
{
    internal class AggregateConfig : Config
    {
        public AggregateConfig(params Config[] configs)
            : base(configs.SkipWhile(c => c.Level == ConfigLevel.Local).FirstOrDefault()?.FilePath ?? throw new ArgumentException())
            => Files = configs.ToList();

        public List<Config> Files { get; }

        public override void AddBoolean(string section, string? subsection, string variable, bool value)
            => GetConfig().AddBoolean(section, subsection, variable, value);

        public override void AddDateTime(string section, string? subsection, string variable, DateTime value)
            => GetConfig().AddDateTime(section, subsection, variable, value);

        public override void AddNumber(string section, string? subsection, string variable, long value)
            => GetConfig().AddNumber(section, subsection, variable, value);

        public override void AddString(string section, string? subsection, string variable, string value)
            => GetConfig().AddString(section, subsection, variable, value);

        public override IEnumerable<ConfigEntry> GetAll(string section, string? subsection, string variable, string? valueRegex)
            => Files.SelectMany(x => x.GetAll(section, subsection, variable, valueRegex));

        public override IEnumerable<ConfigEntry> GetRegex(string nameRegex, string? valueRegex = null)
            => Files.SelectMany(x => x.GetRegex(nameRegex, valueRegex));

        public override string? GetNormalizedPath(string section, string? subsection, string variable)
            => Files.Select(config => config.GetNormalizedPath(section, subsection, variable)).Where(x => x != null).FirstOrDefault();

        public override void RemoveSection(string section, string? subsection = null)
            => GetConfig().RemoveSection(section, subsection);

        public override void RenameSection(string oldSection, string? oldSubsection, string newSection, string? newSubsection)
            => GetConfig().RenameSection(oldSection, oldSubsection, newSection, newSubsection);

        public override void SetAllBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
            => GetConfig().SetAllBoolean(section, subsection, variable, value, valueRegex);

        public override void SetAllDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
            => GetConfig().SetAllDateTime(section, subsection, variable, value, valueRegex);

        public override void SetAllNumber(string section, string? subsection, string variable, long value, string? valueRegex)
            => GetConfig().SetAllNumber(section, subsection, variable, value, valueRegex);

        public override void SetAllString(string section, string? subsection, string variable, string value, string? valueRegex)
            => GetConfig().SetAllString(section, subsection, variable, value, valueRegex);

        public override void SetBoolean(string section, string? subsection, string variable, bool value, string? valueRegex)
            => GetConfig().SetBoolean(section, subsection, variable, value, valueRegex);

        public override void SetDateTime(string section, string? subsection, string variable, DateTime value, string? valueRegex)
            => GetConfig().SetDateTime(section, subsection, variable, value, valueRegex);

        public override void SetNumber(string section, string? subsection, string variable, long value, string? valueRegex)
            => GetConfig().SetNumber(section, subsection, variable, value, valueRegex);

        public override void SetString(string section, string? subsection, string variable, string value, string? valueRegex)
            => GetConfig().SetString(section, subsection, variable, value, valueRegex);

        public override bool TryGetBoolean(string section, string? subsection, string variable, out bool value)
        {
            foreach (var config in Files)
            {
                if (config.TryGetBoolean(section, subsection, variable, out value))
                    return true;
            }

            value = false;
            return false;
        }

        public override bool TryGetDateTime(string section, string? subsection, string variable, out DateTime value)
        {
            foreach (var config in Files)
            {
                if (config.TryGetDateTime(section, subsection, variable, out value))
                    return true;
            }

            value = DateTime.MinValue;
            return false;
        }

        public override bool TryGetNumber(string section, string? subsection, string variable, out long value)
        {
            foreach (var config in Files)
            {
                if (config.TryGetNumber(section, subsection, variable, out value))
                    return true;
            }

            value = 0;
            return false;
        }

        public override bool TryGetString(string section, string? subsection, string variable, out string value)
        {
            foreach (var config in Files)
            {
                if (config.TryGetString(section, subsection, variable, out value))
                    return true;
            }

            value = "";
            return false;
        }

        public override void Unset(string section, string? subsection, string variable)
            => GetConfig().Unset(section, subsection, variable);

        public override void UnsetAll(string section, string? subsection, string variable, string? valueRegex)
            => GetConfig().UnsetAll(section, subsection, variable, valueRegex);

        // When writing via the aggregate config without specifying a ConfigLevel, we first try the default 
        // .netconfig location, followed by a non-local one (i.e. global/system if that's the first we find), 
        // followed by whatever is first last.
        Config GetConfig()
            => Files.FirstOrDefault(x => x.Level == null) ??
               Files.FirstOrDefault(x => x.Level != ConfigLevel.Local) ??
               Files.First();

        protected override IEnumerable<ConfigEntry> GetEntries() => Files.SelectMany(x => x);

        public override string ToString() => string.Join(", ", Files.Select(x => x.FilePath));
    }
}
