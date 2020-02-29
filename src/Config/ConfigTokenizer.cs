using System.Linq;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Microsoft.DotNet
{
    static class ConfigTokenizer
    {
        static readonly TextParser<Unit> IdentifierToken =
            Span.MatchedBy(Character.Letter.IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('-')).IgnoreMany())).Value(Unit.Value);

        static readonly TextParser<Unit> StringContentToken =
            Span.EqualTo("\\\"").Value(Unit.Value).Try()
                .Or(Span.EqualTo("\\\\").Value(Unit.Value).Try())
                .Or(Character.ExceptIn('"', '\\', '\r', '\n').Value(Unit.Value));

        static TextParser<Unit> QuotedStringToken { get; } =
            from open in Character.EqualTo('"')
            from content in StringContentToken.IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;

        // Like the string parser, the number version is permissive - it's just looking 
        // for a chunk of input that looks something like a JSON number, and not
        // necessarily a valid one.
        static TextParser<Unit> NumberToken { get; } =
            from first in Character.Digit
            from rest in Character.Digit.IgnoreMany()
            from suffix in Character.Letter.Optional()
            from b in Character.Letter.Optional()
            select Unit.Value;

        public static Tokenizer<ConfigToken> Instance { get; } =
             new TokenizerBuilder<ConfigToken>()
                 .Ignore(Span.WhiteSpace)
                 .Match(Character.EqualTo('['), ConfigToken.LBracket)
                 .Match(Character.EqualTo(']'), ConfigToken.RBracket)
                 .Match(Character.EqualTo('#'), ConfigToken.Hash)
                 .Match(Character.EqualTo(';'), ConfigToken.Semicolon)
                 .Match(Character.EqualTo('='), ConfigToken.Equal)
                 .Match(IdentifierToken, ConfigToken.Identifier, requireDelimiters: true)
                 .Match(IdentifierToken.AtLeastOnceDelimitedBy(Character.EqualTo('.')), ConfigToken.DottedIdentifier, requireDelimiters: true)
                 .Match(NumberToken, ConfigToken.Number, requireDelimiters: true)
                 .Match(StringContentToken, ConfigToken.String, requireDelimiters: true)
                 .Match(QuotedStringToken, ConfigToken.String, requireDelimiters: true)
                 .Match(Character.AnyChar.AtLeastOnce(), ConfigToken.AnyString, requireDelimiters: true)
                 .Build();
    }
}
