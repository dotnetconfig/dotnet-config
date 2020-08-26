using System.Linq;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace DotNetConfig
{
    static class ConfigTokenizer
    {
        internal static readonly TextParser<Unit> IdentifierToken =
            Span.MatchedBy(Character.Letter.IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('-')).IgnoreMany())).Value(Unit.Value);

        internal static TextParser<Unit> StringToken { get; } =
            from content in Span
                .EqualTo("\\\\").Value(Unit.Value)
                .Or(Character.ExceptIn('"', '\\', '\r', '\n', '#', ';').AtLeastOnce().Value(Unit.Value))
            select Unit.Value;

        internal static TextParser<Unit> QuotedStringToken { get; } =
            from open in Character.EqualTo('"')
            from content in Span
                .EqualTo("\\\"").Value(Unit.Value).Try()
                .Or(Span.EqualTo("\\\\").Value(Unit.Value).Try())
                .Or(Character.ExceptIn('"', '\\', '\r', '\n').Value(Unit.Value))
                .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;

        // Like the string parser, the number version is permissive - it's just looking 
        // for a chunk of input that looks something like a JSON number, and not
        // necessarily a valid one.
        internal static TextParser<Unit> NumberToken { get; } =
            from first in Character.Digit
            from rest in Character.Digit.IgnoreMany()
            from suffix in Character.Letter.Optional()
            from b in Character.Letter.Optional()
            select Unit.Value;

        public static Tokenizer<ConfigToken> Key { get; } =
            new TokenizerBuilder<ConfigToken>()
                .Ignore(Span.WhiteSpace)
                .Match(IdentifierToken, ConfigToken.Identifier, requireDelimiters: true)
                .Match(IdentifierToken.AtLeastOnceDelimitedBy(Character.EqualTo('.')), ConfigToken.DottedIdentifier, requireDelimiters: true)
                .Ignore(Character.EqualTo('.'))
                .Match(StringToken, ConfigToken.String, requireDelimiters: true)
                .Match(QuotedStringToken, ConfigToken.String, requireDelimiters: true)
                .Build();

        public static Tokenizer<ConfigToken> Line { get; } =
             new TokenizerBuilder<ConfigToken>()
                 .Ignore(Span.WhiteSpace)
                 .Match(Character.EqualTo('['), ConfigToken.LBracket)
                 .Match(Character.EqualTo(']'), ConfigToken.RBracket)
                 .Match(Character.EqualTo('#'), ConfigToken.Hash)
                 .Match(Character.EqualTo(';'), ConfigToken.Semicolon)
                 .Match(Character.EqualTo('='), ConfigToken.Equal)
                 .Match(NumberToken, ConfigToken.Number, requireDelimiters: true)
                 .Match(IdentifierToken, ConfigToken.Identifier, requireDelimiters: true)
                 .Match(IdentifierToken.AtLeastOnceDelimitedBy(Character.EqualTo('.')), ConfigToken.DottedIdentifier, requireDelimiters: true)
                 .Match(StringToken, ConfigToken.String, requireDelimiters: true)
                 .Match(StringToken.AtLeastOnce(), ConfigToken.String, requireDelimiters: true)
                 .Match(QuotedStringToken, ConfigToken.QuotedString, requireDelimiters: true)
                 .Match(Character.EqualTo('"'), ConfigToken.Quote)
                 .Match(Character.EqualTo('\\'), ConfigToken.Backslash)
                 .Match(Character.AnyChar.AtLeastOnce(), ConfigToken.AnyString, requireDelimiters: true)
                 .Build();
    }
}
