using System.Linq;
using Superpower;
using Superpower.Parsers;

namespace Microsoft.DotNet
{
    static class ConfigTextParsers
    {
        const long KB = 1024;

        static TextParser<object> QuotedString { get; } =
            from open in Character.EqualTo('"')
            from chars in 
                Span.EqualTo("\\\"").Value('"').Try()
                .Or(Span.EqualTo("\\\\").Value('\\').Try())
                .Or(Character.ExceptIn('"', '\\'))
                .Many()
            from close in Character.EqualTo('"')
            select (object)new string(chars);

        static TextParser<object> PlainString { get; } =
            from chars in
                // Unescape \\, which is valid in a plain string
                Character.EqualTo('\\').Repeat(2).Select(_ => '\\')
                .Or(Character.ExceptIn('"', '\\', '#', ';'))
                .Many()
            select (object)new string(chars);

        public static TextParser<object> String { get; } = QuotedString.Try().Or(PlainString);

        public static TextParser<object> Number { get; } =
            from numeric in Numerics.Natural.Select(n => long.Parse(n.ToStringValue()))
            from multiplier in Character.Letter.Optional().Select(suffix => suffix switch
            {
                null => 1,
                'k' => KB,
                'K' => KB,
                'm' => KB * KB,
                'M' => KB * KB,
                'g' => KB * KB * KB,
                'G' => KB * KB * KB,
                't' => KB * KB * KB * KB,
                'T' => KB * KB * KB * KB,
                _ => 1
            })
            from b in Character.In('b', 'B').Optional()
            select (object)(numeric * multiplier);
    }
}
