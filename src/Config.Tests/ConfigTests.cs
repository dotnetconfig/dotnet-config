using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Moq;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
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
    [InlineData("[core-option]", "core-option", null)]
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
    [InlineData("1key=value", null, null, false)]
    [InlineData("key=value", "key", "value")]
    [InlineData("    key-bar= value has spaces  ", "key-bar", "value has spaces")]
    [InlineData("key-bar = \"value has spaces  \" ", "key-bar", "value has spaces  ")]
    public void can_parse_variable(string input, string key, string expected, bool matched = true)
    {
        var result = Config.VariableParser.TryParse(input);

        Assert.Equal(matched, result.HasValue);

        if (result.HasValue)
        {
            Assert.Equal(key, result.Value.name);
            Assert.Equal(expected, result.Value.value);
        }
    }

    [Fact]
    public void can_read_value_from_file()
    {
        var config = Config.Read(Path.Combine(Directory.GetCurrentDirectory(), "Content", "local", ".netconfig"));

        Assert.True(config.Get<bool>("core", "filemode"));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("yes", true)]
    [InlineData("Yes", true)]
    [InlineData("YES", true)]
    [InlineData("on", true)]
    [InlineData("On", true)]
    [InlineData("ON", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    [InlineData("no", false)]
    [InlineData("No", false)]
    [InlineData("NO", false)]
    [InlineData("off", false)]
    [InlineData("Off", false)]
    [InlineData("OFF", false)]
    public void can_read_bool(string input, bool value)
    {
        var config = Mock.Of<Config>();

        Assert.Equal(value, config.ConvertTo<bool>(input));
        Assert.Equal(value, config.ConvertTo<bool?>(input));
    }

    [Fact]
    public void throws_invalid_bool() => Assert.Throws<ArgumentException>(() => Mock.Of<Config>().ConvertTo<bool>("foo"));

    [Theory]
    [InlineData("10", 10)]
    [InlineData("2k", 2048)]
    [InlineData("2kb", 2048)]
    [InlineData("5M", 5242880)]
    [InlineData("5MB", 5242880)]
    [InlineData("1G", 1073741824)]
    [InlineData("1GB", 1073741824)]
    public void can_read_int(string input, int value)
    {
        var config = Mock.Of<Config>();

        Assert.Equal(value, config.ConvertTo<int>(input));
        Assert.Equal(value, config.ConvertTo<int?>(input));
    }

    [Theory]
    [InlineData("500M", 524288000)]
    [InlineData("5G", 5368709120)]
    [InlineData("2T", 2199023255552)]
    [InlineData("2Tb", 2199023255552)]
    public void can_read_long(string input, long value)
    {
        var config = Mock.Of<Config>();

        Assert.Equal(value, config.ConvertTo<long>(input));
        Assert.Equal(value, config.ConvertTo<long?>(input));
    }

    [Fact]
    public void can_read_datetime()
    {
        var config = Mock.Of<Config>();

        Assert.Equal(DateTime.Parse("2008-09-22T14:01:54.9571247Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), config.ConvertTo<DateTime>("2008 -09-22T14:01:54.9571247Z"));
        Assert.Equal(DateTime.Parse("2008-09-22T14:01:54.9571247Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), config.ConvertTo<DateTime?>("2008 -09-22T14:01:54.9571247Z"));
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
    public void can_match_identifier()
    {
        var identifier = Identifier.CStyle(new Superpower.Model.TextSpan("asdf")).Value.ToStringValue();

        Assert.Equal("asdf", identifier);

        var parser = Span.MatchedBy(Character.Letter.IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('-')).IgnoreMany()));
        
        Assert.False(parser(new TextSpan("1foo")).HasValue);
        Assert.False(parser(new TextSpan("_foo")).HasValue);
        Assert.True(parser(new TextSpan("f1-oo")).HasValue);
        Assert.Equal("foo-bar-2", parser(new TextSpan("foo-bar-2")).Value.ToStringValue());
    }
}
