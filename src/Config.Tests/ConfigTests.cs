using System;
using System.Globalization;
using System.IO;
using Moq;
using Superpower;
using Xunit;
using Microsoft.DotNet;

namespace Microsoft.DotNet.Tests
{
    public class ConfigTests : IDisposable
    {
        string originalDir;

        public ConfigTests()
        {
            Config.GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "global.netconfig");
            Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "system.netconfig");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            originalDir = Directory.GetCurrentDirectory();
        }

        public void Dispose()
        {
            if (originalDir != Directory.GetCurrentDirectory())
                Directory.SetCurrentDirectory(originalDir);
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
        public void can_build_no_config()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = Config.Build(dir);

            Assert.Equal(Path.Combine(dir, Config.FileName), config.FilePath);
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
        public void when_build_single_file_does_not_return_aggregate()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Path.Combine(
                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName,
                ".netconfig");
            File.WriteAllText(path, "");

            var config = Config.Build(path);

            Assert.IsType<FileConfig>(config);
        }

        [Fact]
        public void when_read_local_single_file_does_not_return_aggregate()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            File.WriteAllText(Path.Combine(path, ".netconfig"), "");

            Directory.SetCurrentDirectory(path);

            var config = Config.Read(ConfigLevel.Local);

            Assert.IsType<FileConfig>(config);
        }

        [Fact]
        public void when_build_hierarchical_filepath_is_first_local()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));

            Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", Config.FileName), config.FilePath);
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
