using System;
using System.Globalization;
using System.IO;
using Moq;
using Superpower;
using Xunit;
using Microsoft.DotNet;

namespace Microsoft.DotNet.Tests
{
    public class ConfigTests
    {
        public ConfigTests()
        {
            Config.GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "global.netconfig");
            Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "system.netconfig");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [Theory]
        [InlineData("[core.option]", "core.option", null)]
        [InlineData("[core.option \"sub section\"] # this is a comment", "core.option", "sub section")]
        [InlineData("[core-option]", "core-option", null)]
        public void can_parse_section(string input, string name, string subsection)
        {
            var result = ConfigParser.SectionParser.TryParse(input);

            if (!result.HasValue)
                Assert.Equal("", result.FormatErrorMessageFragment());

            Assert.True(result.HasValue);
            Assert.Equal(name, result.Value.section);
            Assert.Equal(subsection, result.Value.subsection);
        }

        [Fact]
        public void can_deserialize_datetime()
        {
            var serializer = new ValueSerializer();

            Assert.Equal(DateTime.Parse("2008-09-22T14:01:54.9571247Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), serializer.Deserialize<DateTime>("2008 -09-22T14:01:54.9571247Z"));
            Assert.Equal(DateTime.Parse("2008-09-22T14:01:54.9571247Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), serializer.Deserialize<DateTime?>("2008 -09-22T14:01:54.9571247Z"));
        }

        [Fact]
        public void can_read_hierarchical()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));

            Assert.True(config.Get<bool>("core", "local"));
            Assert.True(config.Get<bool>("core", "parent"));
            Assert.True(config.Get<bool>("core", "global"));
            Assert.True(config.Get<bool>("core", "system"));
        }

        [Fact]
        public void can_read_system()
        {
            var config = Config.Read(ConfigLevel.System);

            Assert.False(config.Get<bool>("core", "local"));
            Assert.False(config.Get<bool>("core", "parent"));
            Assert.False(config.Get<bool>("core", "global"));
            Assert.True(config.Get<bool>("core", "system"));
        }

        [Fact]
        public void can_read_global()
        {
            var config = Config.Read(ConfigLevel.Global);

            Assert.False(config.Get<bool>("core", "local"));
            Assert.False(config.Get<bool>("core", "parent"));
            Assert.True(config.Get<bool>("core", "global"));
            Assert.False(config.Get<bool>("core", "system"));
        }

        [Fact]
        public void can_read_local()
        {
            var dir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));
                var config = Config.Read(ConfigLevel.Local);

                Assert.True(config.Get<bool>("core", "local"));
                Assert.True(config.Get<bool>("core", "parent"));
                Assert.False(config.Get<bool>("core", "global"));
                Assert.False(config.Get<bool>("core", "system"));
            }
            finally
            {
                Directory.SetCurrentDirectory(dir);
            }
        }

        [Fact]
        public void can_read_file()
        {
            var config = Config.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.Get<bool>("core", "local"));
            Assert.False(config.Get<bool>("core", "parent"));
            Assert.False(config.Get<bool>("core", "global"));
            Assert.False(config.Get<bool>("core", "system"));
        }

        [Fact]
        public void can_read_non_existent_file()
        {
            var config = Config.FromFile(Path.GetTempFileName());

            Assert.False(config.Get<bool>("core", "bool"));
            Assert.Null(config.Get<bool?>("core", "bool"));
            Assert.Null(config.Get<string>("core", "string"));
            Assert.Equal(0, config.Get<int>("core", "int"));
            Assert.Null(config.Get<int?>("core", "int"));
        }

        [Fact]
        public void can_write_new_file()
        {
            var file = Path.GetTempFileName();
            var config = Config.FromFile(file);

            config.Set("section", "subsection", "bool", true);

            Assert.True(config.Get<bool>("section", "subsection", "bool"));
        }
    }
}
