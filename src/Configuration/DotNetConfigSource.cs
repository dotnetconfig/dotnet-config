using Microsoft.Extensions.Configuration;

namespace DotNetConfig
{
    class DotNetConfigSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new DotNetConfigProvider();
    }
}
