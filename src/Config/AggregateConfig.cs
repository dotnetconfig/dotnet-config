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

        public override void Add<T>(string section, string? subsection, string variable, T value)
            => configs.First().Add(section, subsection, variable, value);

        public override IEnumerable<ConfigEntry> GetAll<T>(string section, string? subsection, string variable, string? valueRegex = null)
            => configs.SelectMany(x => x.GetAll<T>(section, subsection, variable, valueRegex));

        public override void Set<T>(string section, string? subsection, string variable, T value, string? valueRegex = null)
            => configs.First().Set(section, subsection, variable, value, valueRegex);

        public override void SetAll<T>(string section, string? subsection, string variable, T value, string? valueRegex = null)
            => configs.First().SetAll(section, subsection, variable, value, valueRegex);

        public override bool TryGet<T>(string section, string? subsection, string variable, out T value)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            foreach (var config in configs)
            {
                if (config.TryGet(section, subsection, variable, out value))
                    return true;
            }

            return false;
        }

        public override void Unset(string section, string? subsection, string variable)
            => configs.First().Unset(section, subsection, variable);

        public override void UnsetAll(string section, string? subsection, string variable, string? valueRegex)
            => configs.First().UnsetAll(section, subsection, variable, valueRegex);

        protected override IEnumerable<ConfigEntry> GetEntries() => configs.SelectMany(x => x);
    }
}
