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

            Assert.True(config.GetBoolean("core", "local"));
            Assert.True(config.GetBoolean("core", "parent"));
            Assert.True(config.GetBoolean("core", "global"));
            Assert.True(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void can_read_system()
        {
            var config = Config.Read(ConfigLevel.System);

            Assert.Null(config.GetBoolean("core", "local"));
            Assert.Null(config.GetBoolean("core", "parent"));
            Assert.Null(config.GetBoolean("core", "global"));
            Assert.True(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void can_read_global()
        {
            var config = Config.Read(ConfigLevel.Global);

            Assert.Null(config.GetBoolean("core", "local"));
            Assert.Null(config.GetBoolean("core", "parent"));
            Assert.True(config.GetBoolean("core", "global"));
            Assert.Null(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void can_read_local_only()
        {
            var dir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));
                var config = Config.Read(ConfigLevel.Local);

                Assert.True(config.GetBoolean("core", "local"));
                Assert.True(config.GetBoolean("core", "parent"));
                Assert.Null(config.GetBoolean("core", "global"));
                Assert.Null(config.GetBoolean("core", "system"));
            }
            finally
            {
                Directory.SetCurrentDirectory(dir);
            }
        }

        [Fact]
        public void can_read_local_file()
        {
            var config = Config.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.GetBoolean("core", "local"));
        }

        [Fact]
        public void when_reading_local_file_parent_variable_is_null()
        {
            var config = Config.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.Null(config.GetBoolean("core", "parent"));
        }

        [Fact]
        public void when_reading_local_file_global_variable_is_null()
        {
            var config = Config.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.Null(config.GetBoolean("core", "global"));
        }

        [Fact]
        public void when_reading_local_file_system_variable_is_null()
        {
            var config = Config.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.Null(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void when_building_config_then_file_can_be_missing()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = Config.Build(dir);

            Assert.Equal(Path.Combine(dir, Config.FileName), config.FilePath);
        }

        [Fact]
        public void when_reading_non_existent_file_then_values_are_null()
        {
            var config = Config.FromFile(Path.GetTempFileName());

            Assert.Null(config.GetBoolean("core", "bool"));
            Assert.Null(config.GetBoolean("core", "bool"));
            Assert.Null(config.GetString("core", "string"));
            Assert.Null(config.GetNumber("core", "int"));
            Assert.Null(config.GetNumber("core", "int"));
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

            config.SetBoolean("section", "subsection", "bool", true);

            Assert.True(config.GetBoolean("section", "subsection", "bool"));
        }

        [Fact]
        public void can_roundtrip()
        {
            var file = Path.GetTempFileName();
            var config = Config.FromFile(file);

            config.SetString("section", "subsection", "foo", "bar");

            var value = Config.FromFile(file).GetString("section", "subsection", "foo");

            Assert.Equal("bar", value);
        }
    }
}
