using System;
using System.IO;
using Xunit;

namespace Microsoft.DotNet.Tests
{
    public class ConfigParserTests
    {
        [Theory]
        [InlineData("[foo\\bar]")]
        [InlineData("[foo\"]")]
        [InlineData("[foo")]
        public void cannot_parse_invalid_section(string input)
        {
            Assert.False(ConfigParser.TryParseSection(input, out var _, out var __, out var ___));
        }

        [Theory]
        [InlineData("[foo]", "foo", null)]
        [InlineData("[foo.bar]", "foo.bar", null)]
        [InlineData("[foo bar]", "foo", "bar")]
        [InlineData("[foo-bar]", "foo-bar", null)]
        [InlineData("[foo bar-baz]", "foo", "bar-baz")]
        [InlineData("[foo \"bar\"]", "foo", "bar")]
        [InlineData("[foo.bar \"baz\"]", "foo.bar", "baz")]
        [InlineData("[foo.bar \"baz \\\"quoted\\\"\"]", "foo.bar", "baz \"quoted\"")]
        public void can_parse_section(string input, string section, string? subsection)
        {
            if (ConfigParser.TryParseSection(input, out var result, out var error, out var position))
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
        public void can_parse_variable(string input, string name, string? value)
        {
            if (ConfigParser.TryParseVariable(input, out var result, out var error, out var position))
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
        public void cannot_parse_invalid_variable(string input)
        {
            Assert.False(ConfigParser.TryParseVariable(input, out var _, out var __, out var ___));
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
            if (ConfigParser.TryParseComment(input, out var result, out var error, out var position))
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
            Assert.False(ConfigParser.TryParseComment("not a comment", out _, out _, out _));
        }

        [Fact]
        public void can_parse_tokens()
        {
            foreach (var line in File.ReadAllLines($"Content\\ConfigParserTests.txt"))
            {
                {
                    if (ConfigParser.TryParseSection(line, out var section, out var error, out var position))
                    {
                        Console.WriteLine(section.ToString());
                    }
                    else
                    {
                        Console.WriteLine($"{position.Column}: {error}");
                    }
                }
                {
                    if (ConfigParser.TryParseVariable(line, out var variable, out var error, out var position))
                    {
                        Console.WriteLine($"{variable.Name}={variable.Value}");
                    }
                    else
                    {
                        Console.WriteLine($"{position.Column}: {error}");
                    }
                }

                var tokens = ConfigTokenizer.Instance.TryTokenize(line);
                Console.WriteLine(tokens);
            }
        }
    }
}
