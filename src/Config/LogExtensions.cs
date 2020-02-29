using System.Diagnostics;
using Superpower;
using Superpower.Model;

namespace Microsoft.DotNet
{
    internal static class LogExtensions
    {
        public static TokenListParser<TKind, T> Log<TKind, T>(this TokenListParser<TKind, T> parser, string name)
        {
            if (!Debugger.IsAttached)
                return parser;

            return delegate (TokenList<TKind> input)
            {
                var result = parser(input);
                Debug.WriteLine($"{name}: {result.HasValue.ToString().ToLowerInvariant()} => {(result.HasValue ? result.Value?.ToString() : "")}");
                return result;
            };
        }

        public static TextParser<T> Log<T>(this TextParser<T> parser, string name)
        {
            if (!Debugger.IsAttached)
                return parser;

            return delegate (TextSpan input)
            {
                var result = parser(input);
                Debug.WriteLine($"{name}: {result.HasValue.ToString().ToLowerInvariant()} => {(result.HasValue ? result.Value?.ToString() : "")}");
                return result;
            };
        }
    }
}
