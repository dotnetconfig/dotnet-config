using Superpower.Display;
using Superpower.Parsers;

namespace DotNetConfig
{
    enum ConfigToken
    {
        [Token(Example = "[")]
        LBracket,

        [Token(Example = "]")]
        RBracket,

        [Token(Example = "#")]
        Hash,

        [Token(Example = ";")]
        Semicolon,

        [Token(Example = "=")]
        Equal,

        [Token(Category = "character", Example = "\"")]
        Quote,

        [Token(Category = "character", Example = "\\")]
        Backslash,

        [Token(Category = "identifier", Description = "alphanumeric characters and `-`, starting with an alphabetic character", Example = "foo")]
        Identifier,

        [Token(Category = "identifier", Description = "dot-separated identifiers, consisting of alphanumeric characters and `-`, starting with an alphabetic character", Example = "foo.bar")]
        DottedIdentifier,

        [Token(Category = "value", Description = "string enclosed in double quotes", Example = "\"foo bar\"")]
        QuotedString,

        [Token(Category = "value", Description = "string value", Example = "\"foo bar\"")]
        String,

        AnyString,

        Number,
    }
}
