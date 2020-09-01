namespace DotNetConfig
{
    /// <summary>
    /// A position within a line of configuration.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// The position corresponding to the zero index.
        /// </summary>
        public static Position Zero { get; } = new Position(0, 0, 1);

        /// <summary>
        /// Creates a new position.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="absolute">The absolute position (zero-based).</param>
        /// <param name="column">The column number (one-based).</param>
        public Position(int line, int absolute, int? column = default)
        {
            Line = line;
            Absolute = absolute;
            Column = column ?? absolute + 1;
        }

        /// <summary>
        /// The zero-based absolute index of the position.
        /// </summary>
        public int Absolute { get; }

        /// <summary>
        /// Gets the one-based line number.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the one-based column number.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// <see langword="true"/> if the position has a value.
        /// </summary>
        public bool HasValue => Line > 0;

        /// <summary>
        /// Gets the <see cref="Absolute"/> position.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Absolute.ToString();
    }
}
