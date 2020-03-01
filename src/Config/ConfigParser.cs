using System;
using System.Linq;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Parses configuration lines with good error reporting.
    /// </summary>
    internal static class ConfigParser
    {
        internal static TextParser<TextSpan> IdentifierParser { get; } =
            Span.MatchedBy(Character.Letter.IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('-')).IgnoreMany()));

        internal static TextParser<(string section, string? subsection)> SectionParser { get; } =
            from section in Character.EqualTo('[').IgnoreThen(Character.Matching(c => char.IsLetterOrDigit(c) || c == '-' || c == '.', "alphanumeric, '-' or '.'").Many())
            from subsection in Character.WhiteSpace.IgnoreThen(QuotedString.CStyle).OptionalOrDefault()
            select (new string(section), subsection);

        internal static TextParser<(string name, string? value)> VariableParser { get; } =
            from name in Character.WhiteSpace.IgnoreMany().IgnoreThen(IdentifierParser)
            from value in
                    Character.WhiteSpace.IgnoreMany().IgnoreThen(
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                    // Couldn't figure out how to fix the proper nullability constraints here
                    Character.EqualTo('=').IgnoreThen(
                    Character.WhiteSpace.IgnoreMany().IgnoreThen(
                    QuotedString.CStyle.Or(
                        Character.ExceptIn('#', ';').Many().Select(x => new string(x).Trim())))).OptionalOrDefault())
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            select (name.ToStringValue(), value);

        static TokenListParser<ConfigToken, object> Identifier { get; } = Token
            .EqualTo(ConfigToken.Identifier)
            .Apply(ConfigTextParsers.String);

        static TokenListParser<ConfigToken, object> String { get; } = Token
            .EqualTo(ConfigToken.String)
            .Apply(ConfigTextParsers.String);

        static TokenListParser<ConfigToken, object> Number { get; } = Token
            .EqualTo(ConfigToken.Number)
            .Apply(ConfigTextParsers.Number);

        static TokenListParser<ConfigToken, object> True { get; } =
            Token.EqualToValueIgnoreCase(ConfigToken.Identifier, "true")
                 .Or(Token.EqualToValueIgnoreCase(ConfigToken.Identifier, "yes"))
                 .Or(Token.EqualToValueIgnoreCase(ConfigToken.Identifier, "on"))
                 .Or(Token.EqualToValueIgnoreCase(ConfigToken.Number, "1"))
                 .Value((object)"true");

        static TokenListParser<ConfigToken, object> False { get; } =
            Token.EqualToValueIgnoreCase(ConfigToken.Identifier, "false")
                 .Or(Token.EqualToValueIgnoreCase(ConfigToken.Identifier, "no"))
                 .Or(Token.EqualToValueIgnoreCase(ConfigToken.Identifier, "off"))
                 .Or(Token.EqualToValueIgnoreCase(ConfigToken.Number, "0"))
                 .Value((object)"false");

        static TokenListParser<ConfigToken, string> Comment { get; } =
            from begin in Token.EqualTo(ConfigToken.Hash).Or(Token.EqualTo(ConfigToken.Semicolon))
            // Basically anything is accepted after the comment start
            from comment in new TokenListParser<ConfigToken, Token<ConfigToken>>(input => input.ConsumeToken()).Many()
            select string.Join(" ", comment.Select(c => c.ToStringValue()));

        static readonly object NullValue = new object();
        static readonly string NullString = Guid.NewGuid().ToString();

        static TokenListParser<ConfigToken, Line> Section { get; } =
            from begin in Token.EqualTo(ConfigToken.LBracket)
            from section in
                Token.EqualTo(ConfigToken.Identifier)
                .Or(Token.EqualTo(ConfigToken.DottedIdentifier)).Named("alphanumeric, '-' or '.'")
                .Apply(ConfigTextParsers.String)
            from subsection in
                Token.EqualTo(ConfigToken.Identifier)
                .Or(Token.EqualTo(ConfigToken.DottedIdentifier))
                .Or(Token.EqualTo(ConfigToken.String))
                .Apply(ConfigTextParsers.String)
                .OptionalOrDefault(NullValue)
            from end in Token.EqualTo(ConfigToken.RBracket)
            from comment in Comment.OptionalOrDefault(NullString).Try()
            select (Line)new SectionLine((string)section, ReferenceEquals(NullValue, subsection) ? null : (string)subsection, ReferenceEquals(NullString, comment) ? null : comment);

        static TokenListParser<ConfigToken, Line> ShortcutTrueVariable { get; } =
            from name in Token.EqualTo(ConfigToken.Identifier).Apply(ConfigTextParsers.String)
            from comment in Comment.OptionalOrDefault(NullString).Try()
            select (Line)new VariableLine((string)name, "true", ReferenceEquals(NullString, comment) ? null : comment);

        static TokenListParser<ConfigToken, Line> FullVariable { get; } =
            from name in Token.EqualTo(ConfigToken.Identifier).Apply(ConfigTextParsers.String)
            from equal in Token.EqualTo(ConfigToken.Equal)
            from value in True.Or(False).Or(Number).Or(String)
                // The tokenizer may match a value with spaces as multiple identifiers
                .Or(Identifier.AtLeastOnce().Select(v => (object)string.Join(" ", v)))
            from comment in Comment.OptionalOrDefault(NullString).Try()
            select (Line)new VariableLine((string)name, value == null ? null : value.ToString(), ReferenceEquals(NullString, comment) ? null : comment);

        // Try allows us to backtrack and attempt the shortcut version
        static TokenListParser<ConfigToken, Line> Variable { get; } = FullVariable.Try().Or(ShortcutTrueVariable).AtEnd();

        static TokenListParser<ConfigToken, Line> CommentLine { get; } =
            from comment in Comment
            select (Line)new CommentLine(comment);

        static TokenListParser<ConfigToken, Line> Line { get; } =
            Section.AtEnd()
            .Or(Variable.AtEnd())
            .Or(CommentLine.AtEnd());

        public static bool TryParse(string line, out Line? result, out string? error, out Position errorPosition)
            => TryParse(Line, line, out result, out error, out errorPosition);

        static bool TryParse(TokenListParser<ConfigToken, Line> parser, string line, out Line? result, out string? error, out Position errorPosition)
        {
            var tokens = ConfigTokenizer.Instance.TryTokenize(line);
            if (!tokens.HasValue)
            {
                result = null;
                error = tokens.ToString();
                errorPosition = tokens.ErrorPosition;
                return false;
            }

            var parsed = parser.TryParse(tokens.Value);
            if (!parsed.HasValue)
            {
                result = null;
                error = parsed.ToString();
                errorPosition = parsed.ErrorPosition;
                return false;
            }

            result = parsed.Value;
            // Preserve the line verbatim to maintain all formatting when writing out unchanged lines.
            result.Text = line;
            error = null;
            errorPosition = Position.Empty;
            return true;
        }

        internal static bool TryParseSection(string line, out SectionLine? section, out string? error, out Position errorPosition)
        {
            section = null;
            var success = TryParse(Section, line, out var result, out error, out errorPosition);
            if (success && result != null)
            {
                section = (SectionLine)result;
            }

            return success;
        }

        internal static bool TryParseVariable(string line, out VariableLine? variable, out string? error, out Position errorPosition)
        {
            variable = null;
            var success = TryParse(Variable, line, out var result, out error, out errorPosition);
            if (success && result != null)
            {
                variable = (VariableLine)result;
            }

            return success;
        }

        internal static bool TryParseComment(string line, out Line? comment, out string? error, out Position errorPosition)
            => TryParse(CommentLine, line, out comment, out error, out errorPosition);
    }
}
