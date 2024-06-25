using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace DotNetConfig
{
    public class ConfigTests
    {
        readonly ITestOutputHelper output;

        public ConfigTests(ITestOutputHelper output)
        {
            Directory.SetCurrentDirectory(Constants.CurrentDirectory);
            Config.GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "global.netconfig");
            Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "system.netconfig");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.output = output;
        }

        [Fact]
        public void can_read_hierarchical()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));

            Assert.True(config.GetBoolean("core", "local"));
            Assert.True(config.GetBoolean("core", "parent"));
            Assert.True(config.GetBoolean("core", "global"));
            Assert.True(config.GetBoolean("core", "system"));

            var section = config.GetSection("core");

            Assert.True(section.GetBoolean("local"));
            Assert.True(section.GetBoolean("parent"));
            Assert.True(section.GetBoolean("global"));
            Assert.True(section.GetBoolean("system"));
        }

        [Fact]
        public void when_reading_hierarchical_from_file_reads_system_once()
        {
            Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", ".netconfig");

            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

            Assert.True(config.GetBoolean("core", "local"));
            Assert.True(config.GetBoolean("core", "parent"));

            var section = config.GetSection("core");

            Assert.True(section.GetBoolean("local"));
            Assert.True(section.GetBoolean("parent"));
            Assert.True(section.GetBoolean("global"));

            // Reads the system.netconfig only once, even if the file is in a subfolder
            Assert.Single(config.GetAll("core", "parent"));
        }

        [Fact]
        public void can_read_hierarchical_from_root_file()
        {
            var config = Config.Build("C:\\.netconfig");

            Assert.True(config.GetBoolean("core", "global"));
            Assert.True(config.GetBoolean("core", "system"));
        }

        [Fact]
        public void can_read_hierarchical_from_relative_path()
        {
            var config = Config.Build(Config.FileName);

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

            Assert.False(File.Exists(config.FilePath));
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
        public void when_build_hierarchical_filepath_is_default()
        {
            var config = Config.Build(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local"));

            Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", Config.FileName), config.FilePath);
        }

        [Fact]
        public void can_write_new_file()
        {
            var file = Path.GetTempFileName();
            var config = Config.Build(file)
                .SetBoolean("section", "subsection", "bool", true);

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
        public void when_setting_variable_then_uses_tab()
        {
            var file = Path.GetTempFileName();
            var config = Config.Build(file);

            config.SetString("section", "subsection", "foo", "bar");

            var line = File.ReadAllLines(file).SkipWhile(x => x[0] == '#' || x[0] == '[').First();

            Assert.Equal('\t', line[0]);
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

        [Fact]
        public void when_setting_local_variable_then_writes_user_file()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            var config = Config.Build(path);

            config.AddString("foo", "bar", "baz", ConfigLevel.Local);

            Assert.True(File.Exists(Path.Combine(path, Config.FileName + ".user")));

            using var reader = new ConfigReader(Path.Combine(path, Config.FileName + ".user"));

            Assert.Single(reader.ReadAllLines().Where(x => x.Kind == LineKind.Variable && x.Variable == "bar" && x.Value == "baz"));
        }

        [Fact]
        public void when_setting_variable_then_writes_default_file()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            var config = Config.Build(path);

            config.AddString("foo", "bar", "baz");

            Assert.False(File.Exists(Path.Combine(path, Config.FileName + ".user")));
            Assert.True(File.Exists(Path.Combine(path, Config.FileName)));

            using var reader = new ConfigReader(Path.Combine(path, Config.FileName));

            Assert.Single(reader.ReadAllLines().Where(x => x.Kind == LineKind.Variable && x.Variable == "bar" && x.Value == "baz"));
        }

        [Fact]
        public void when_setting_local_variable_then_overrides_default_file()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            var path = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            File.WriteAllText(Path.Combine(path, ".netconfig"), @"[foo]
bar = baz");
            File.WriteAllText(Path.Combine(path, ".netconfig.user"), @"[foo]
bar = hey");


            var config = Config.Build(path);

            Assert.Equal("hey", config.GetString("foo", "bar"));
        }

        [Fact]
        public void when_resolving_path_then_resolves_relative_to_config()
        {
            var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var systemDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var parentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var localDir = Path.Combine(parentDir, Guid.NewGuid().ToString());

            Config.GlobalLocation = Path.Combine(globalDir, ".netconfig");
            Config.SystemLocation = Path.Combine(systemDir, ".netconfig");

            Directory.CreateDirectory(globalDir);
            Directory.CreateDirectory(systemDir);
            Directory.CreateDirectory(localDir);

            File.WriteAllText(Config.GlobalLocation, @$"[file]
    global = .\\global\\file.txt
    dir = .\\global
");
            File.WriteAllText(Config.SystemLocation, @$"[file]
    system = ./system/file.txt
");
            File.WriteAllText(Path.Combine(parentDir, Config.FileName), @$"[file]
    parent = file.txt
");
            File.WriteAllText(Path.Combine(localDir, Config.FileName), @$"[file]
    local = ./file.txt
");

            var config = Config.Build(localDir);

            Assert.Equal(Path.Combine(globalDir, "global"), config.GetNormalizedPath("file", "dir"));
            Assert.Equal(Path.Combine(globalDir, "global", "file.txt"), config.GetNormalizedPath("file", "global"));
            Assert.Equal(Path.Combine(systemDir, "system", "file.txt"), config.GetNormalizedPath("file", "system"));
            Assert.Equal(Path.Combine(parentDir, "file.txt"), config.GetNormalizedPath("file", "parent"));
            Assert.Equal(Path.Combine(localDir, "file.txt"), config.GetNormalizedPath("file", "local"));

            var section = config.GetSection("file");

            Assert.Equal(Path.Combine(globalDir, "global"), section.GetNormalizedPath("dir"));
            Assert.Equal(Path.Combine(globalDir, "global", "file.txt"), section.GetNormalizedPath("global"));
            Assert.Equal(Path.Combine(systemDir, "system", "file.txt"), section.GetNormalizedPath("system"));
            Assert.Equal(Path.Combine(parentDir, "file.txt"), section.GetNormalizedPath("parent"));
            Assert.Equal(Path.Combine(localDir, "file.txt"), section.GetNormalizedPath("local"));
        }

        [Fact]
        public void when_setting_variable_in_global_dir_then_writes_global_file()
        {
            Config.GlobalLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");
            Config.SystemLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".netconfig");

            // Simulates opening a cli app from the user's profile dir
            Directory.CreateDirectory(Path.GetDirectoryName(Config.GlobalLocation)!);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Config.GlobalLocation)!);
            var config = Config.Build();

            config.SetString("foo", "bar", "baz");

            var global = Config.Build(ConfigLevel.Global);

            Assert.Equal("baz", global.GetString("foo", "bar"));
        }
    }
}
