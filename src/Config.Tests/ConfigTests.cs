using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace DotNetConfig.Tests
{
    public class ConfigTests
    {
        public ConfigTests()
        {
            Config.GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "global.netconfig");
            Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "system.netconfig");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
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
        public void can_read_hierarchical_with_config_entry_level()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));

            foreach (var entry in config.GetRegex("core"))
            {
                switch (entry.Variable)
                {
                    case "local":
                    case "parent":
                        Assert.Null(entry.Level);
                        break;
                    case "global":
                        Assert.Equal(ConfigLevel.Global, entry.Level);
                        break;
                    case "system":
                        Assert.Equal(ConfigLevel.System, entry.Level);
                        break;
                    default:
                        break;
                }
            }
        }

        [Fact]
        public void can_read_system()
        {
            var config = Config.Build(ConfigLevel.System);
            
            Assert.Null(config.GetBoolean("core", "local"));
            Assert.Null(config.GetBoolean("core", "parent"));
            Assert.Null(config.GetBoolean("core", "global"));
            Assert.True(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void can_read_global()
        {
            var config = Config.Build(ConfigLevel.Global);

            Assert.Null(config.GetBoolean("core", "local"));
            Assert.Null(config.GetBoolean("core", "parent"));
            Assert.True(config.GetBoolean("core", "global"));
            Assert.True(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void can_read_local_file()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.GetBoolean("core", "local"));
        }

        [Fact]
        public void when_reading_local_file_as_root_then_parent_variable_is_null()
        {
            var path = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Content", Guid.NewGuid().ToString()));
            Directory.CreateDirectory(path);
            var config = Config.Build(path);
            config.SetBoolean("config", "root", true);

            // Reload after the value was set.
            config = Config.Build(path);

            Assert.Null(config.GetBoolean("config", "parent"));
        }

        [Fact]
        public void when_reading_local_file_with_global_false_then_system_variable_is_null()
        {
            var path = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Content", Guid.NewGuid().ToString()));
            Directory.CreateDirectory(path);
            var config = Config.Build(path);
            config.SetBoolean("config", "global", false);

            // Reload after the value was set.
            config = Config.Build(path);

            Assert.Null(config.GetBoolean("core", "global"));
        }

        [Fact]
        public void when_reading_local_file_with_system_false_then_system_variable_is_null()
        {
            var path = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Content", Guid.NewGuid().ToString()));
            Directory.CreateDirectory(path);
            var config = Config.Build(path);
            config.SetBoolean("config", "system", false);

            // Reload after the value was set.
            config = Config.Build(path);

            Assert.Null(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void when_reading_local_file_parent_variable_is_set()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.GetBoolean("core", "parent"));
        }

        [Fact]
        public void when_reading_local_file_global_variable_is_set()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.GetBoolean("core", "global"));
        }

        [Fact]
        public void when_reading_local_file_system_variable_is_set()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.GetBoolean("core", "system"));
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
            var config = Config.Build(Path.GetTempFileName());

            Assert.Null(config.GetBoolean("core", "bool"));
            Assert.Null(config.GetBoolean("core", "bool"));
            Assert.Null(config.GetString("core", "string"));
            Assert.Null(config.GetNumber("core", "int"));
            Assert.Null(config.GetNumber("core", "int"));
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
            var config = Config.Build(file);

            config.SetBoolean("section", "subsection", "bool", true);

            Assert.True(config.GetBoolean("section", "subsection", "bool"));
        }

        [Fact]
        public void can_roundtrip()
        {
            var file = Path.GetTempFileName();
            var config = Config.Build(file);

            config.SetString("section", "subsection", "foo", "bar");

            var value = Config.Build(file).GetString("section", "subsection", "foo");

            Assert.Equal("bar", value);
        }

        [Fact]
        public void when_setting_global_variable_then_writes_global_file()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            File.WriteAllText(Path.Combine(path, ".netconfig"), "");

            var config = Config.Build(path);

            config.AddBoolean("test", "var", true, ConfigLevel.Global);

            var global = Config.Build(ConfigLevel.Global);

            Assert.True(global.GetBoolean("test", "var"));
        }

        [Fact]
        public void when_setting_system_variable_then_writes_global_file()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            File.WriteAllText(Path.Combine(path, ".netconfig"), "");

            var config = Config.Build(path);

            config.AddBoolean("test", "var", true, ConfigLevel.System);

            var global = Config.Build(ConfigLevel.System);

            Assert.True(global.GetBoolean("test", "var"));
        }
    }
}
