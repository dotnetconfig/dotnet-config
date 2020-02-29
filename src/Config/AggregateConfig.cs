using System;
using System.Collections.Generic;

namespace Microsoft.DotNet
{
    internal class AggregateConfig : Config
    {
        readonly List<Config> configs;

        public AggregateConfig(List<Config> configs)
        {
            this.configs = configs;
        }

        public override void Set<T>(string section, string? subsection, string variable, T value)
        {
            throw new NotSupportedException();
        }

        public override bool TryGet<T>(string section, string? subsection, string variable, out T value)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            foreach (var config in configs)
            {
                if (config.TryGet<T>(section, subsection, variable, out value))
                    return true;
            }

            return false;
        }
    }
}
