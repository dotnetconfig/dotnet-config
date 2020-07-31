using System;
using System.Text.RegularExpressions;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Allows matching values by using a regular expression.
    /// </summary>
    public class ValueMatcher
    {
        readonly Func<string?, bool> matcher;

        /// <summary>
        /// A <see cref="ValueMatcher"/> that always matches regardless of the 
        /// specified value.
        /// </summary>
        public static ValueMatcher All { get; } = new ValueMatcher(_ => true);

        /// <summary>
        /// An optional regular expression to use 
        /// </summary>
        /// <param name="expression">Regular expression, optionally starting with <c>!</c> to negate the match expression.</param>
        public static ValueMatcher From(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return All;

            if (expression![0] == '!')
                return new ValueMatcher(v => v != null && !Regex.IsMatch(v, expression.Substring(1)));

            return new ValueMatcher(v => v != null && Regex.IsMatch(v, expression));

        }

        ValueMatcher(Func<string?, bool> matcher) => this.matcher = matcher;

        /// <summary>
        /// Checks whether the given <paramref name="value"/> matches the expression 
        /// specified for the <see cref="ValueMatcher"/> when constructed.
        /// </summary>
        /// <returns><see langword="true"/> if the value is not null and matches the expression.</returns>
        public bool Matches(string? value) => matcher(value);
    }
}
