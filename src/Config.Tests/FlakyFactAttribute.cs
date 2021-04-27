using System;
using Xunit;

namespace DotNetConfig
{
    public class FlakyFactAttribute : FactAttribute
    {
        public FlakyFactAttribute()
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var ci) && ci)
                Skip = "Flaky test that is not run on CI";
        }
    }
}
