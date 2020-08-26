using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace DotNetConfig.Tests
{
    public class ConfigParserTests
    {
        ITestOutputHelper output;

        public ConfigParserTests(ITestOutputHelper output) => this.output = output;

        [Theory]
        [InlineData("[foo\\bar]")]
        [InlineData("[foo\"]")]
        [InlineData("[foo bar")]
        public void cannot_parse_invalid_section(string input)
        {
            Assert.False(ConfigParser.TryParseSectionLine(input, out var _, out var __, out var ___));
        }

        [Theory]
        [InlineData("[foo]", "foo", null)]
        [InlineData("[foo.bar]", "foo.bar", null)]
        [InlineData("[foo \"bar\"]", "foo", "bar")]
        [InlineData("[foo-bar]", "foo-bar", null)]
        [InlineData("[foo \"bar-baz\"]", "foo", "bar-baz")]
        [InlineData("[foo.bar \"baz\"]", "foo.bar", "baz")]
        [InlineData("[foo.bar \"baz \\\"quoted\\\"\"]", "foo.bar", "baz \"quoted\"")]
        public void can_parse_section(string input, string section, string subsection)
        {
            if (ConfigParser.TryParseSectionLine(input, out var result, out var error, out var position))
            {
                Assert.Equal(section, result.Section);
                Assert.Equal(subsection, result.Subsection);
            }
            else
            {
                Assert.True(false, $"at {position.Column}: {error}");
            }
        }

        [Theory]
        [InlineData("foo", "foo", "true")]
        [InlineData("foo=bar", "foo", "bar")]
        [InlineData("foo-bar=baz", "foo-bar", "baz")]
        [InlineData("foo = bar", "foo", "bar")]
        [InlineData("foo = \"with spaces\"", "foo", "with spaces")]
        [InlineData("  foo    =    bar", "foo", "bar")]
        [InlineData("\tfoo=bar", "foo", "bar")]
        [InlineData("foo= value has spaces  ", "foo", "value has spaces")]
        [InlineData("foo= value: has colon", "foo", "value: has colon")]
        [InlineData("foo=\"+A;-B\"", "foo", "+A;-B")]
        [InlineData("foo=\"A#B\"", "foo", "A#B")]
        [InlineData("glob = targets\\\\*\\\\*.xml", "glob", "targets\\*\\*.xml")]
        [InlineData("file=a.xml", "file", "a.xml")]
        [InlineData("foo= .txt=text/plain", "foo", ".txt=text/plain")]
        [InlineData("quoted=\"this is an \\\"example\\\" of a nested quote\"", "quoted", "this is an \"example\" of a nested quote")]
        [InlineData("  key = value  # this is a comment", "key", "value")]
        [InlineData("\tkey=value;this is a comment", "key", "value")]
        [InlineData("key= \"value has spaces  \" ", "key", "value has spaces  ")]
        [InlineData("enabled=1", "enabled", "true")]
        [InlineData("enabled=true", "enabled", "true")]
        [InlineData("enabled=True", "enabled", "true")]
        [InlineData("enabled=TRUE", "enabled", "true")]
        [InlineData("enabled=yes", "enabled", "true")]
        [InlineData("enabled=Yes", "enabled", "true")]
        [InlineData("enabled=YES", "enabled", "true")]
        [InlineData("enabled=on", "enabled", "true")]
        [InlineData("enabled=On", "enabled", "true")]
        [InlineData("enabled=ON", "enabled", "true")]
        [InlineData("enabled=0", "enabled", "false")]
        [InlineData("enabled=false", "enabled", "false")]
        [InlineData("enabled=False", "enabled", "false")]
        [InlineData("enabled=FALSE", "enabled", "false")]
        [InlineData("enabled=no", "enabled", "false")]
        [InlineData("enabled=No", "enabled", "false")]
        [InlineData("enabled=NO", "enabled", "false")]
        [InlineData("enabled=off", "enabled", "false")]
        [InlineData("enabled=Off", "enabled", "false")]
        [InlineData("enabled=OFF", "enabled", "false")]
        [InlineData("size=10", "size", "10")]
        [InlineData("size=2k", "size", "2048")]
        [InlineData("size=2kb", "size", "2048")]
        [InlineData("size=2K", "size", "2048")]
        [InlineData("size=2KB", "size", "2048")]
        [InlineData("size=5m", "size", "5242880")]
        [InlineData("size=5mb", "size", "5242880")]
        [InlineData("size=5M", "size", "5242880")]
        [InlineData("size=5MB", "size", "5242880")]
        [InlineData("size=500m", "size", "524288000")]
        [InlineData("size=1g", "size", "1073741824")]
        [InlineData("size=1gb", "size", "1073741824")]
        [InlineData("size=1G", "size", "1073741824")]
        [InlineData("size=1GB", "size", "1073741824")]
        [InlineData("size=5G", "size", "5368709120")]
        [InlineData("size=2T", "size", "2199023255552")]
        [InlineData("size=2Tb", "size", "2199023255552")]
        [InlineData("size=2t", "size", "2199023255552")]
        [InlineData("size=2tb", "size", "2199023255552")]
        public void can_parse_variable(string input, string name, string value)
        {
            if (ConfigParser.TryParseVariableLine(input, out var result, out var error, out var position))
            {
                Assert.Equal(name, result.Name);
                Assert.Equal(value, result.Value);
            }
            else
            {
                Assert.True(false, $"at {position.Column}: {error}");
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("variable with spaces = 1")]
        [InlineData("variable_with_underscore = 1")]
        [InlineData("missing-end-quote = \"")]
        [InlineData("must-quote-backslash = \\")]
        [InlineData("missing-value = ")]
        [InlineData("1variable=1")]
        [InlineData("foo= value \"has single quote  ")]
        [InlineData("foo= value \\has backslash")]
        public void cannot_parse_invalid_variable(string input)
        {
            Assert.False(ConfigParser.TryParseVariableLine(input, out _, out _, out _));
        }

        [Theory]
        [InlineData("# this is a comment")]
        [InlineData("  # this is a comment")]
        [InlineData("\t\t# this is a comment")]
        [InlineData("; this is a comment")]
        [InlineData("  ; this is a comment")]
        [InlineData("\t\t; this is a comment")]
        public void can_parse_comment(string input)
        {
            if (ConfigParser.TryParseCommentLine(input, out var result, out var error, out var position))
            {
                Assert.Equal(input, result.Text);
            }
            else
            {
                Assert.True(false, $"at {position.Column}: {error}");
            }
        }

        [Fact]
        public void cannot_parse_non_comment()
        {
            Assert.False(ConfigParser.TryParseCommentLine("not a comment", out _, out _, out _));
        }

        [Theory]
        [InlineData("file.app.config.url", "file.app", "config", "url")]
        [InlineData("file.url", "file", null, "url")]
        [InlineData("file.\"app.config\".url", "file", "app.config", "url")]
        [InlineData("file.\"with spaces\".url", "file", "with spaces", "url")]
        [InlineData("file.\"src\\\\app.config\".url", "file", "src\\app.config", "url")]
        public void can_parse_key(string key, string section, string subsection, string variable)
        {
            Assert.True(ConfigParser.TryParseKey(key, out var s, out var ss, out var v, out _));
            Assert.Equal(section, section);
            Assert.Equal(subsection, ss);
            Assert.Equal(variable, v);
        }

        [Theory]
        [InlineData("file.app.config", "file.app", "config")]
        [InlineData("file", "file", null)]
        [InlineData("file.\"app.config\"", "file", "app.config")]
        [InlineData("foo.\"bar or baz\"", "file", "bar or baz")]
        public void can_parse_section_key(string key, string section, string subsection)
        {
            Assert.True(ConfigParser.TryParseSection(key, out var s, out var ss, out _));
            Assert.Equal(section, section);
            Assert.Equal(subsection, ss);
        }
    }
}
