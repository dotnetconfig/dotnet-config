using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DotNetConfig
{
    class DotNetConfigProvider : ConfigurationProvider
    {
        Config configuration = Config.Build();

        public override void Load()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in configuration)
            {
                // NOTES:
                // 1. Microsoft.Extensions.Configuration does not support multi-valued variables
                // so last value wins.
                // 2. a null/empty value is equal to "true" in dotnet-config/git-config.
                // 3. The hierarchical keys are separated by : instead of .

                var key = entry.Section;
                if (entry.Subsection != null)
                    key += ConfigurationPath.KeyDelimiter + entry.Subsection;

                key += ConfigurationPath.KeyDelimiter + entry.Variable;

                data[key] = string.IsNullOrWhiteSpace(entry.RawValue) ? "true" : entry.RawValue!.Trim('"');
            }

            Data = data;
        }

        public override void Set(string key, string value)
        {
            var first = key.IndexOf(ConfigurationPath.KeyDelimiter);
            if (first == -1)
                throw new ArgumentException(key, nameof(key));

            var section = key.Substring(0, first);
            string? subsection = default;
            string variable;

            var last = key.LastIndexOf(ConfigurationPath.KeyDelimiter);

            if (first == last)
            {
                variable = key.Substring(first + 1);
            }
            else
            {
                subsection = key.Substring(first + 1, last - first - 1);
                variable = key.Substring(last + 1);
            }

            configuration = configuration.SetString(section, subsection, variable, value);
            base.Set(key, value);
        }
    }
}
