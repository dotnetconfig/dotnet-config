namespace DotNetConfig
{
    /// <summary>
    /// A span of configuration text within an entire line.
    /// </summary>
    class TextSpan
    {
        string? text;

        /// <summary>
        /// Construct a span encompassing an entire string.
        /// </summary>
        /// <param name="line">The source line.</param>
        public TextSpan(string line)
            : this(line, Position.Zero, line.Length)
        {
        }

        /// <summary>
        /// Construct a string span for a substring of <paramref name="line"/>.
        /// </summary>
        /// <param name="line">The source line.</param>
        /// <param name="position">The start of the span.</param>
        /// <param name="length">The length of the span.</param>
        /// <param name="text">Optional pre-calculated text from the span.</param>
        public TextSpan(string line, Position position, int length, string? text = null)
        {
            Line = line;
            Position = position;
            Length = length;
            this.text = text;
        }

        /// <summary>
        /// The line of text text containing the span.
        /// </summary>
        public string Line { get; }

        /// <summary>
        /// The position of the start of the span within the string.
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// The length of the span.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The text value of the span.
        /// </summary>
        public string Text
        {
            get
            {
                if (text == null)
                    text = Line == null ? "" : Line.Substring(Position.Absolute, Length);

                return text;
            }
        }

        /// <summary>
        /// Gets the <see cref="Text"/> represented by this span.
        /// </summary>
        public override string ToString() => Text;

        /// <summary>
        /// Gets the text value of the span.
        /// </summary>
        public static implicit operator string?(TextSpan? span) => span?.Text;
    }
}
