using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
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
            this.output = output;
        }

        [Fact]
        public void can_load_hierarchical_values()
        {
            var config = BuildConfiguration();

            Assert.Equal("on", config["core:parent"]);
            Assert.Equal("true", config["http:sslVerify"]);
            Assert.Equal("false", config["http:https://weak.example.com:sslVerify"]);
            Assert.Equal("yay", config["foo:bar:baz"]);
        }

        [Fact]
        public void can_save_values()
        {
            var config = BuildConfiguration();

            config["foo:enabled"] = "true";
            config["foo:bar:baz"] = "bye";
            config["http:https://weaker.example.com:sslVerify"] = "false";

            var dotnet = Config.Build(Path.Combine(Constants.CurrentDirectory, "Content", "web", nameof(can_save_values)));

            Assert.Equal("true", dotnet.GetString("foo", "enabled"));
            Assert.Equal("bye", dotnet.GetString("foo", "bar", "baz"));
            Assert.Equal("false", dotnet.GetString("http", "https://weaker.example.com", "sslVerify"));
        }

        [Fact]
        public void local_values_override_system_values()
        {
            var config = BuildConfiguration();

            Assert.Equal("123", config["local:value"]);
        }

        [Fact]
        public void saves_to_same_level()
        {
            var dir = Path.Combine(Constants.CurrentDirectory, "Content", "web", nameof(saves_to_same_level));
            Directory.CreateDirectory(dir);
            var usr = Path.Combine(dir, Config.FileName + Config.UserExtension);

            Config.Build(usr).SetNumber("local", "value", 999, ConfigLevel.Local);

            var config = BuildConfiguration();

            Assert.Equal("999", config["local:value"]);

            config["local:value"] = "888";

            Assert.Equal("888", config["local:value"]);
            Assert.Contains("888", File.ReadAllText(usr));
        }

        IConfiguration BuildConfiguration([CallerMemberName] string? methodName = null)
        {
            var dir = Path.Combine(Constants.CurrentDirectory, "Content", "web", methodName);
            Directory.CreateDirectory(dir);

            return new ConfigurationBuilder().AddDotNetConfig(dir).Build();
        }
    }
}