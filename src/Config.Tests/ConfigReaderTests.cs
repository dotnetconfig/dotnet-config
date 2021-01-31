using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotNetConfig
{
    public class ConfigReaderTests
    {
        [Theory]
        [InlineData("[foo]", "foo", null)]
        [InlineData("[foo-bar]", "foo-bar", null)]
        [InlineData("[foo \"bar\"]", "foo", "bar")]
        [InlineData("[foo] # some comment", "foo", null, "# some comment")]
        [InlineData("[foo \"bar\"] # some comment", "foo", "bar", "# some comment")]
        [InlineData("[foo \"bar\\baz\"]", "foo", "barbaz")]
        [InlineData("# comment", null, null, "# comment")]
        [InlineData("    # comment", null, null, "# comment")]
        public void can_read_section(string input, string section, string subsection, string? comment = null)
        {
            using var reader = new ConfigReader(new StringReader(input));
            var line = reader.ReadLine();

            Assert.NotNull(line);

            if (section != null)
            {
                Assert.Equal(LineKind.Section, line!.Kind);
                Assert.Equal(section, line.Section);
            }

            if (subsection != null)
                Assert.Equal(subsection, line!.Subsection);

            if (comment != null)
                Assert.Equal(comment, line!.Comment);
        }

        [Theory]
        [InlineData("[foo]", "bar", null, "[bar]")]
        [InlineData("[foo \"bar\"]", "bar", "baz", "[bar \"baz\"]")]
        [InlineData("[foo] # some comment", "bar", null, "[bar] # some comment")]
        [InlineData("[foo \"bar\"] # some comment", "bar", "baz", "[bar \"baz\"] # some comment")]
        public void can_replace_section(string input, string section, string subsection, string expected)
        {
            using var reader = new ConfigReader(new StringReader(input));
            var line = reader.ReadLine();

            Assert.NotNull(line);

            var updated = line!.WithSection(section, subsection);

            Assert.Equal(expected, updated.LineText);
        }

        [Theory]
        [InlineData("[", 2, "Expected section name.")]
        [InlineData("[1foo]", 2, "Section name must start with a letter.")]
        [InlineData("[foo_bar]", 5, "Section name can only contain letters, digits or '-'.")]
        [InlineData("[foo;", 5, "Section name can only contain letters, digits or '-'.")]
        [InlineData("[foo", 5, "Expected end of section ']' or whitespace followed by quoted subsection name.")]
        [InlineData("[foo ", 6, "Expected quoted subsection name.")]
        [InlineData("[foo \"bar\\", 11, "Expected closing quote.")]
        [InlineData("[foo \"bar", 10, "Expected closing quote and end of section ']'.")]
        [InlineData("[foo \"bar\"", 11, "Expected end of section ']'.")]
        [InlineData("[foo \"bar\" ", 11, "Expected end of section ']'.")]
        public void reports_section_error(string input, int column, string error, params object[] args)
        {
            using var reader = new ConfigReader(new StringReader(input));
            var line = reader.ReadLine();

            Assert.NotNull(line);
            Assert.Equal(LineKind.Error, line!.Kind);
            Assert.Equal(string.Format(error, args), line!.Error);
            Assert.Equal(column, line!.ErrorPosition?.Column);
        }

        [Theory]
        [InlineData("foo", "foo", null)]
        [InlineData("foo   ", "foo", null)]
        [InlineData("       foo   ", "foo", null)]
        [InlineData("foo # comment", "foo", null, "# comment")]
        [InlineData("foo=bar", "foo", "bar")]
        [InlineData("foo = bar", "foo", "bar")]
        [InlineData("foo-bar=baz", "foo-bar", "baz")]
        [InlineData("  foo    =    bar", "foo", "bar")]
        [InlineData("\tfoo=bar", "foo", "bar")]
        [InlineData("  foo=ba\\tr", "foo", "ba\tr")]
        [InlineData("  foo=bar\\nbaz", "foo", @"bar
baz")]
        [InlineData("foo = \"with spaces\"", "foo", "with spaces")]
        [InlineData("foo= value   has spaces  ", "foo", "value   has spaces")]
        [InlineData("key= \"value has spaces  \" ", "key", "value has spaces  ")]
        [InlineData("foo= value: has colon", "foo", "value: has colon")]
        [InlineData("foo=\"+A;-B\"", "foo", "+A;-B")]
        [InlineData("foo=\"A#B\"", "foo", "A#B")]
        [InlineData("glob = targets\\\\*\\\\*.xml", "glob", "targets\\*\\*.xml")]
        [InlineData("file=a.xml", "file", "a.xml")]
        [InlineData("foo= .txt=text/plain", "foo", ".txt=text/plain")]
        [InlineData("quoted=\"this is \\\"great\\\".\"", "quoted", "this is \"great\".")]
        [InlineData("  key = value  # this is a comment", "key", "value", "# this is a comment")]
        [InlineData("connection=DefaultEndpointsProtocol=https;AccountName=", "connection", "DefaultEndpointsProtocol=https")]
        [InlineData("connection=\"DefaultEndpointsProtocol=https;AccountName=;\"", "connection", "DefaultEndpointsProtocol=https;AccountName=;")]

        //[InlineData("foo", "foo", "true")]
        //[InlineData("enabled=1", "enabled", "true")]
        //[InlineData("enabled=true", "enabled", "true")]
        //[InlineData("enabled=True", "enabled", "true")]
        //[InlineData("enabled=TRUE", "enabled", "true")]
        //[InlineData("enabled=yes", "enabled", "true")]
        //[InlineData("enabled=Yes", "enabled", "true")]
        //[InlineData("enabled=YES", "enabled", "true")]
        //[InlineData("enabled=on", "enabled", "true")]
        //[InlineData("enabled=On", "enabled", "true")]
        //[InlineData("enabled=ON", "enabled", "true")]
        //[InlineData("enabled=0", "enabled", "false")]
        //[InlineData("enabled=false", "enabled", "false")]
        //[InlineData("enabled=False", "enabled", "false")]
        //[InlineData("enabled=FALSE", "enabled", "false")]
        //[InlineData("enabled=no", "enabled", "false")]
        //[InlineData("enabled=No", "enabled", "false")]
        //[InlineData("enabled=NO", "enabled", "false")]
        //[InlineData("enabled=off", "enabled", "false")]
        //[InlineData("enabled=Off", "enabled", "false")]
        //[InlineData("enabled=OFF", "enabled", "false")]
        //[InlineData("size=10", "size", "10")]
        //[InlineData("size=2k", "size", "2048")]
        //[InlineData("size=2kb", "size", "2048")]
        //[InlineData("size=2K", "size", "2048")]
        //[InlineData("size=2KB", "size", "2048")]
        //[InlineData("size=5m", "size", "5242880")]
        //[InlineData("size=5mb", "size", "5242880")]
        //[InlineData("size=5M", "size", "5242880")]
        //[InlineData("size=5MB", "size", "5242880")]
        //[InlineData("size=500m", "size", "524288000")]
        //[InlineData("size=1g", "size", "1073741824")]
        //[InlineData("size=1gb", "size", "1073741824")]
        //[InlineData("size=1G", "size", "1073741824")]
        //[InlineData("size=1GB", "size", "1073741824")]
        //[InlineData("size=5G", "size", "5368709120")]
        //[InlineData("size=2T", "size", "2199023255552")]
        //[InlineData("size=2Tb", "size", "2199023255552")]
        //[InlineData("size=2t", "size", "2199023255552")]
        //[InlineData("size=2tb", "size", "2199023255552")]
        public void can_read_variable(string input, string name, string value, string? comment = null)
        {
            using var reader = new ConfigReader(new StringReader("[section]" + Environment.NewLine + input));
            Assert.NotNull(reader.ReadLine());

            var line = reader.ReadLine();

            Assert.NotNull(line);
            Assert.Equal(LineKind.Variable, line!.Kind);
            Assert.Equal(name, line.Variable);

            if (value == null)
                Assert.Null(line.Value);
            else
                Assert.Equal(value, line.Value);

            if (comment != null)
                Assert.Equal(comment, line.Comment);
        }

        [Theory]
        [InlineData("\tfoo", "bar", "\tfoo = bar")]
        [InlineData("\tfoo=bar", null, "\tfoo")]
        [InlineData("\tfoo # comment", "bar", "\tfoo = bar # comment")]
        [InlineData("\tfoo=bar", "baz", "\tfoo = baz")]
        [InlineData("\tfoo = bar", "baz", "\tfoo = baz")]
        [InlineData("\tfoo-bar=baz", "foo", "\tfoo-bar = foo")]
        [InlineData("\tfoo    =    bar", "baz", "\tfoo = baz")]
        [InlineData("\tfoo=b\\tar", "ba\tr", "\tfoo = ba\tr")]
        public void can_replace_variable(string input, string value, string expected)
        {
            using var reader = new ConfigReader(new StringReader("[section]" + Environment.NewLine + input));
            Assert.NotNull(reader.ReadLine());

            var line = reader.ReadLine();

            Assert.NotNull(line);

            var updated = line!.WithValue(value);

            Assert.Equal(expected, updated.LineText);
        }

        [Fact]
        public void reports_required_section()
        {
            using var reader = new ConfigReader(new StringReader("foo = bar"));
            var line = reader.ReadLine();

            Assert.NotNull(line);
            Assert.Equal(LineKind.Error, line!.Kind);
            Assert.Equal("Variables must be declared within a section.", line.Error);
        }

        [Theory]
        [InlineData("1foo", 1, "Variable name must start with a letter.")]
        [InlineData("foo_bar", 4, "Variable name can only contain letters, digits or '-'.")]
        [InlineData("foo =", 6, "Expected variable value after '='.")]
        [InlineData("foo=\"", 5, "Double quotes must be properly balanced or escaped with a backslash.")]
        [InlineData("foo=\\", 6, "Incomplete character escape.")]
        [InlineData("foo=\"bar\\", 10, "Incomplete character escape.")]
        [InlineData("foo = bar \" baz", 11, "Double quotes must be properly balanced or escaped with a backslash.")]
        [InlineData("v.1 = 1", 2, "Variable name can only contain letters, digits or '-'.")]
        [InlineData("variable with spaces = 1", 9, "Variable name can only contain letters, digits or '-'.")]
        [InlineData("variable_with_underscore = 1", 9, "Variable name can only contain letters, digits or '-'.")]
        [InlineData("foo=\"bar\\baz\"", 10, "Invalid escape sequence '\\b'.")]
        public void reports_variable_error(string input, int column, string error, params object[] args)
        {
            using var reader = new ConfigReader(new StringReader("[section]" + Environment.NewLine + input));
            Assert.NotNull(reader.ReadLine());

            var line = reader.ReadLine();

            Assert.NotNull(line);
            Assert.Equal(LineKind.Error, line!.Kind);
            Assert.Equal(string.Format(error, args), line.Error);
            Assert.Equal(column, line.ErrorPosition?.Column);
        }

        [Fact]
        public void reading_from_multiple_threads_succeeds()
        {
            ThreadPool.GetMaxThreads(out var threads, out _);

            Config.GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "global.netconfig");
            Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "system.netconfig");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Content", ".netconfig");

            Task.WaitAll(Enumerable
                .Range(0, threads * 2)
                .Select(_ => Task.Run(() => Config.Build(path)))
                .ToArray());
        }
    }
}
