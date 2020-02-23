using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Moq;
using Superpower;
using Xunit;

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
    public void can_parse_section(string input, string name, string subsection)
    {
        var result = Config.SectionParser.TryParse(input);

        if (!result.HasValue)
            Assert.Equal("", result.FormatErrorMessageFragment());

        Assert.True(result.HasValue);
        Assert.Equal(name, result.Value.section);
        Assert.Equal(subsection, result.Value.subsection);
    }

    [Theory]
    [InlineData("switch", "switch", null)]
    [InlineData("switch=true", "switch", "true")]
    [InlineData("quoted=\"this is an \\\"example\\\" of a nested quote\"", "quoted", "this is an \"example\" of a nested quote")]
    [InlineData("switch # should just set the switch to true if no value", "switch", null)]
    [InlineData("  key = value  # this is a comment", "key", "value")]
    [InlineData("\tkey=value;this is a comment", "key", "value")]
    public void can_parse_variable(string input, string key, string expected)
    {
        var result = Config.VariableParser.TryParse(input);

        if (!result.HasValue)
            Assert.Equal("", result.FormatErrorMessageFragment());

        Assert.True(result.HasValue);
        Assert.Equal(key, result.Value.name);
        Assert.Equal(expected, result.Value.value);
    }

    [Fact]
    public void can_read_value_from_file()
    {
        var config = Config.Read(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

        Assert.True(config.Get<bool>("core", "filemode"));
    }

    [Fact]
    public void can_read_bool()
    {
        var config = Mock.Of<Config>();

        Assert.True(config.ConvertTo<bool>(null));
        Assert.True(config.ConvertTo<bool>("true"));
        Assert.True(config.ConvertTo<bool>("yes"));
        Assert.True(config.ConvertTo<bool>("on"));
        Assert.True(config.ConvertTo<bool>("1"));

        Assert.False(config.ConvertTo<bool>("false"));
        Assert.False(config.ConvertTo<bool>("no"));
        Assert.False(config.ConvertTo<bool>("off"));
        Assert.False(config.ConvertTo<bool>("0"));

        Assert.Throws<ArgumentException>(() => config.ConvertTo<bool>("foo"));
    }

    [Fact]
    public void can_read_datetime()
    {
        var config = Mock.Of<Config>();

        Assert.Equal(DateTime.Parse("2008-09-22T14:01:54.9571247Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), config.ConvertTo<DateTime>("2008 -09-22T14:01:54.9571247Z"));
        Assert.Equal(DateTime.Parse("2008-09-22T14:01:54.9571247Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), config.ConvertTo<DateTime?>("2008 -09-22T14:01:54.9571247Z"));
    }

    [Fact]
    public void can_read_int()
    {
        var config = Mock.Of<Config>();

        Assert.Equal(10, config.ConvertTo<int>("10"));
        Assert.Equal(10, config.ConvertTo<int?>("10"));
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
}
