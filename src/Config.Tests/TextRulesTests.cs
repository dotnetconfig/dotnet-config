using System;
using Xunit;

namespace DotNetConfig
{
    public class TextRulesTests
    {
        [Theory]
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
        public void can_parse_boolean(string value, bool expected)
        {
            Assert.Equal(expected, TextRules.ParseBoolean(value));
        }

        [Theory]
        [InlineData("10", "10")]
        [InlineData("2k", "2048")]
        [InlineData("2kb", "2048")]
        [InlineData("2K", "2048")]
        [InlineData("2KB", "2048")]
        [InlineData("5m", "5242880")]
        [InlineData("5mb", "5242880")]
        [InlineData("5M", "5242880")]
        [InlineData("5MB", "5242880")]
        [InlineData("500m", "524288000")]
        [InlineData("1g", "1073741824")]
        [InlineData("1gb", "1073741824")]
        [InlineData("1G", "1073741824")]
        [InlineData("1GB", "1073741824")]
        [InlineData("5G", "5368709120")]
        [InlineData("2T", "2199023255552")]
        [InlineData("2Tb", "2199023255552")]
        [InlineData("2t", "2199023255552")]
        [InlineData("2tb", "2199023255552")]
        public void can_parse_number(string value, string expected)
        {
            Assert.Equal(long.Parse(expected), TextRules.ParseNumber(value));
        }
    }
}
