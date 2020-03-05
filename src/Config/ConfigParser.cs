using System;
using System.Linq;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Parses configuration lines with good error reporting.
    /// </summary>
    internal static class ConfigParser
    {
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

        internal static TokenListParser<ConfigToken, object> Section { get; } =
            Token.EqualTo(ConfigToken.Identifier)
            .Or(Token.EqualTo(ConfigToken.DottedIdentifier)).Named("alphanumeric, '-' or '.'")
            .Apply(ConfigTextParsers.String);

        internal static TokenListParser<ConfigToken, object> Subsection { get; } =
            Token.EqualTo(ConfigToken.Identifier)
            .Or(Token.EqualTo(ConfigToken.DottedIdentifier))
            .Or(Token.EqualTo(ConfigToken.String))
            .Apply(ConfigTextParsers.String)
            .OptionalOrDefault(NullValue);

        internal static TokenListParser<ConfigToken, object> Variable { get; } =
            Token.EqualTo(ConfigToken.Identifier).Apply(ConfigTextParsers.String);

        static readonly object NullValue = new object();
        static readonly string NullString = Guid.NewGuid().ToString();

        static TokenListParser<ConfigToken, Line> SectionLine { get; } =
            from begin in Token.EqualTo(ConfigToken.LBracket)
            from section in Section
            from subsection in Subsection
            from end in Token.EqualTo(ConfigToken.RBracket)
            from comment in Comment.OptionalOrDefault(NullString).Try()
            select (Line)new SectionLine((string)section, ReferenceEquals(NullValue, subsection) ? null : (string)subsection, ReferenceEquals(NullString, comment) ? null : comment);

        static TokenListParser<ConfigToken, Line> ShortcutTrueVariableLine { get; } =
            from name in Variable
            from comment in Comment.OptionalOrDefault(NullString).Try()
            select (Line)new VariableLine((string)name, "true", ReferenceEquals(NullString, comment) ? null : comment);

        static TokenListParser<ConfigToken, Line> FullVariableLine { get; } =
            from name in Variable
            from equal in Token.EqualTo(ConfigToken.Equal)
            from value in True.Or(False).Or(Number).Or(String)
                // The tokenizer may match a value with spaces as multiple identifiers
                .Or(Identifier.AtLeastOnce().Select(v => (object)string.Join(" ", v)))
            from comment in Comment.OptionalOrDefault(NullString).Try()
            select (Line)new VariableLine((string)name, value == null ? null : value.ToString(), ReferenceEquals(NullString, comment) ? null : comment);

        // Try allows us to backtrack and attempt the shortcut version
        static TokenListParser<ConfigToken, Line> VariableLine { get; } = FullVariableLine.Try().Or(ShortcutTrueVariableLine).AtEnd();

        static TokenListParser<ConfigToken, Line> CommentLine { get; } =
            from comment in Comment
            select (Line)new CommentLine(comment);

        static TokenListParser<ConfigToken, Line> Line { get; } =
            SectionLine.AtEnd()
            .Or(VariableLine.AtEnd())
            .Or(CommentLine.AtEnd());

        public static bool TryParse(string line, out Line? result, out string? error, out Position errorPosition)
            => TryParse(Line, line, out result, out error, out errorPosition);

        static bool TryParse(TokenListParser<ConfigToken, Line> parser, string line, out Line? result, out string? error, out Position errorPosition)
        {
            var tokens = ConfigTokenizer.Line.TryTokenize(line);
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

        internal static bool TryParseSectionLine(string line, out SectionLine? section, out string? error, out Position errorPosition)
        {
            section = null;
            var success = TryParse(SectionLine, line, out var result, out error, out errorPosition);
            if (success && result != null)
            {
                section = (SectionLine)result;
            }

            return success;
        }

        internal static bool TryParseVariableLine(string line, out VariableLine? variable, out string? error, out Position errorPosition)
        {
            variable = null;
            var success = TryParse(VariableLine, line, out var result, out error, out errorPosition);
            if (success && result != null)
            {
                variable = (VariableLine)result;
            }

            return success;
        }
         
        internal static bool TryParseCommentLine(string line, out Line? comment, out string? error, out Position errorPosition)
            => TryParse(CommentLine, line, out comment, out error, out errorPosition);

        internal static bool TryParseKey(string key, out string? section, out string? subsection, out string? variable)
        {
            section = null;
            subsection = null;
            variable = null;

            var tokenizer = new TokenizerBuilder<ConfigToken>()
                .Ignore(Span.WhiteSpace)
                .Match(ConfigTokenizer.IdentifierToken, ConfigToken.Identifier, requireDelimiters: true)
                .Match(ConfigTokenizer.IdentifierToken.AtLeastOnceDelimitedBy(Character.EqualTo('.')), ConfigToken.DottedIdentifier, requireDelimiters: true)
                .Ignore(Character.EqualTo('.'))
                .Match(ConfigTokenizer.StringToken, ConfigToken.String, requireDelimiters: true)
                .Match(ConfigTokenizer.QuotedStringToken, ConfigToken.String, requireDelimiters: true)
                .Build();

            var tokens = tokenizer.TryTokenize(key);
            if (!tokens.HasValue)
            {
                return false;
            }

            var parsed = (from sec in Identifier.AtLeastOnce()
                          from subsec in String.OptionalOrDefault(NullString)
                          from name in Identifier.OptionalOrDefault(NullString)
                          select (sec, subsec, name))
                         .TryParse(tokens.Value);

            if (!parsed.HasValue)
            {
                return false;
            }

            var parsedSection = parsed.Value.sec.Cast<string>().ToArray();
            var parsedSubsection = (string)parsed.Value.subsec == NullString ? null : (string)parsed.Value.subsec;
            var parsedName = (string)parsed.Value.name == NullString ? null : (string)parsed.Value.name;

            if (parsedName == null && parsedSection.Length > 1)
            {
                parsedName = parsedSection[^1];
                parsedSection = parsedSection[0..^1];
            }

            if (parsedSubsection == null && parsedSection.Length > 1)
            {
                parsedSubsection = parsedSection[^1];
                parsedSection = parsedSection[0..^1];
            }

            section = string.Join(".", parsedSection);
            subsection = parsedSubsection;
            variable = parsedName;

            return true;
        }

        internal static bool TryParseSection(string key, out string? section, out string? subsection)
        {
            section = null;
            subsection = null;
            var tokens = ConfigTokenizer.Key.TryTokenize(key);
            if (!tokens.HasValue)
            {
                return false;
            }

            var parsed = (from sec in Identifier.AtLeastOnce()
                          from subsec in String.OptionalOrDefault(NullString)
                          select (sec, subsec))
                         .TryParse(tokens.Value);

            if (!parsed.HasValue)
            {
                return false;
            }

            var parsedSection = parsed.Value.sec.Cast<string>().ToArray();
            subsection = (string)parsed.Value.subsec == NullString ? null : (string)parsed.Value.subsec;

            if (subsection == null && parsedSection.Length > 1)
            {
                subsection = parsedSection[^1];
                parsedSection = parsedSection[0..^1];
            }

            section = string.Join(".", parsedSection);

            return true;
        }
    }
}
