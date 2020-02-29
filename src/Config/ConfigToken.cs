using Superpower.Display;
using Superpower.Parsers;

namespace Microsoft.DotNet
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

        Identifier,

        DottedIdentifier,

        String,

        AnyString,

        Number,
    }
}
