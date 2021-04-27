using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace DotNetConfig
{
    public class ConfigurationTests
    {
        readonly ITestOutputHelper output;

        public ConfigurationTests(ITestOutputHelper output)
        {
            Config.GlobalLocation = Path.Combine(Constants.CurrentDirectory, "Content", "global.netconfig");
            Config.SystemLocation = Path.Combine(Constants.CurrentDirectory, "Content", "system.netconfig");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Directory.SetCurrentDirectory(Path.Combine(Constants.CurrentDirectory, "Content", "web"));
            this.output = output;
        }

        [FlakyFact]
        public void LoadHierarchicalValues()
        {
            var config = new ConfigurationBuilder().AddDotNetConfig().Build();
            Assert.Equal("on", config["core:parent"]);
            Assert.Equal("true", config["http:sslVerify"]);
            Assert.Equal("false", config["http:https://weak.example.com:sslVerify"]);
            Assert.Equal("yay", config["foo:bar:baz"]);
        }

        [FlakyFact]
        public void SaveValues()
        {
            var config = new ConfigurationBuilder().AddDotNetConfig().Build();
            config["foo:enabled"] = "true";
            config["foo:bar:baz"] = "bye";
            config["http:https://weaker.example.com:sslVerify"] = "false";

            var dotnet = Config.Build();

            Assert.Equal("true", dotnet.GetString("foo", "enabled"));
            Assert.Equal("bye", dotnet.GetString("foo", "bar", "baz"));
            Assert.Equal("false", dotnet.GetString("http", "https://weaker.example.com", "sslVerify"));
        }
    }
}