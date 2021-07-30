using Microsoft.Extensions.Configuration;

namespace DotNetConfig
{
    class DotNetConfigSource : IConfigurationSource
    {
        readonly string? path;

        public DotNetConfigSource(string? path = null) => this.path = path;

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new DotNetConfigProvider(path);
    }
}
