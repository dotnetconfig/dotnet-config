using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotNetConfig
{
    class ConfigReader : IDisposable
    {
        readonly string? filePath;
        TextReader? reader;
        int lineNumber;
        TextSpan? section;
        TextSpan? subsection;

        public static Line ParseSection(string? filePath, string lineText, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(lineText))
                throw new ArgumentException($"{filePath}({lineNumber},0): Expected section");

            var current = AdvanceTo(lineText, 0, c => !char.IsWhiteSpace(c));
            var c = lineText[current];

            if (c == '[')
            {
                var line = ReadSection(filePath, lineText, lineNumber, ++current, c);
                if (line.Kind == LineKind.Error)
                    throw new ArgumentException($"{filePath}({lineNumber},{line.ErrorPosition!.Column}): {line.Error}");
                else if (line.Kind == LineKind.Section)
                    return line;
            }

            throw new ArgumentException($"{filePath}({lineNumber},0): Expected section");
        }

        public static Line ParseVariable(string? filePath, string lineText, int lineNumber, TextSpan? section, TextSpan? subsection)
        {
            if (string.IsNullOrWhiteSpace(lineText))
                throw new ArgumentException($"{filePath}({lineNumber},0): Expected variable");

            var current = AdvanceTo(lineText, 0, c => !char.IsWhiteSpace(c));
            var c = lineText[current];

            var line = ReadVariable(filePath, lineText, lineNumber, section, subsection, current, c);
            if (line.Kind == LineKind.Error)
                throw new ArgumentException($"{filePath}({lineNumber},{line.ErrorPosition!.Column}): {line.Error}");
            else if (line.Kind != LineKind.Variable)
                throw new ArgumentException($"{filePath}({lineNumber},0): Expected variable");

            return line;
        }

        public ConfigReader(string filePath)
            : this(new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, options: FileOptions.SequentialScan)))
            => this.filePath = filePath;

        internal ConfigReader(TextReader reader) => this.reader = reader;

        public Line? ReadLine()
        {
            if (reader == null || reader.Peek() == -1)
                return null;

            var lineText = reader.ReadLine();
            var lineLength = lineText.Length;
            lineNumber++;

            if (string.IsNullOrWhiteSpace(lineText))
                return new Line(filePath, LineKind.None, lineNumber, lineText);

            var current = AdvanceTo(lineText, 0, c => !char.IsWhiteSpace(c));
            var c = lineText[current];

            if (c == '[')
            {
                section = null;
                subsection = null;
                var line = ReadSection(filePath, lineText, lineNumber, ++current, c);
                if (line.Kind == LineKind.Section)
                {
                    section = line.Section;
                    subsection = line.Subsection;
                }
                return line;
            }

            if (c == '#' || c == ';')
                return new Line(filePath, LineKind.Comment, lineNumber, lineText, comment: ReadComment(lineText, lineNumber, current));

            return ReadVariable(filePath, lineText, lineNumber, section, subsection, current, c);
        }

        public IEnumerable<Line> ReadAllLines()
        {
            var line = ReadLine();
            while (line != null)
            {
                yield return line;
                line = ReadLine();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        static Line ReadSection(string? filePath, string lineText, int lineNumber, int current, char c)
        {
            if (current >= lineText.Length)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Expected section name.",
                    errorPosition: new Position(lineNumber, lineText.Length));
            }

            var start = current;
            var end = current;
            c = lineText[current];

            if (!char.IsLetter(c))
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Section name must start with a letter.",
                    errorPosition: new Position(lineNumber, current));
            }

            end = current = AdvanceWhile(lineText, current, c => char.IsLetterOrDigit(c) || c == '-');

            // Advance to next char past the end of the section name
            if (current == -1 || ++current >= lineText.Length)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Expected end of section ']' or whitespace followed by quoted subsection name.",
                    errorPosition: new Position(lineNumber, current == -1 ? lineText.Length : current));
            }

            c = lineText[current];

            if (c != ']' && !char.IsWhiteSpace(c))
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Section name can only contain letters, digits or '-'.",
                    errorPosition: new Position(lineNumber, current));
            }

            var section = new TextSpan(lineText, new Position(lineNumber, start), end - start + 1);

            if (c == ']')
            {
                return new Line(filePath, LineKind.Section, lineNumber, lineText, section,
                    comment: ReadComment(lineText, lineNumber, current + 1));
            }

            // If we didn't find the end of the section, advance past the whitespace 
            // to read the subsection
            if (char.IsWhiteSpace(c) &&
                (++current >= lineText.Length || (c = lineText[current]) != '"'))
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Expected quoted subsection name.",
                    errorPosition: new Position(lineNumber, current >= lineText.Length ? lineText.Length : current));
            }

            return ReadSubsection(filePath, lineText, lineNumber, section, current);
        }

        static Line ReadSubsection(string? filePath, string lineText, int lineNumber, TextSpan section, int current)
        {
            var lineLength = lineText.Length;
            var start = current;
            char c;

            // Subsection has special escape rules, so we build its text once
            var builder = new StringBuilder(lineText.Length - start);
            // We are at the starting " char, skip it to begin subsection reading.
            var i = ++current;
            while (i < lineLength)
            {
                c = lineText[i];
                var escaped = false;
                if (c == '\\')
                {
                    escaped = true;
                    if (++i >= lineLength)
                    {
                        return new Line(filePath, LineKind.Error, lineNumber, lineText,
                            error: "Expected closing quote.",
                            errorPosition: new Position(lineNumber, lineLength));
                    }
                }

                if ((c = lineText[i]) == '"' && !escaped)
                {
                    if (++i >= lineLength || lineText[i] != ']')
                    {
                        return new Line(filePath, LineKind.Error, lineNumber, lineText,
                            error: "Expected end of section ']'.",
                            errorPosition: new Position(lineNumber, i));
                    }

                    return new Line(filePath, LineKind.Section, lineNumber, lineText, section,
                        new TextSpan(lineText, new Position(lineNumber, start), i - start, builder.ToString()),
                        comment: ReadComment(lineText, lineNumber, i + 1));
                }

                if (c == '"' && !escaped)
                {
                    return new Line(filePath, LineKind.Error, lineNumber, lineText,
                        error: "Double quote must be escaped.",
                        errorPosition: new Position(lineNumber, i));
                }

                builder.Append(c);
                i++;
            }

            if (builder.Length > 0)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Expected closing quote and end of section ']'.",
                    errorPosition: new Position(lineNumber, lineLength));
            }

            return new Line(filePath, LineKind.Error, lineNumber, lineText,
                error: "Expected end of section ']' or whitespace followed by quoted subsection name.",
                errorPosition: new Position(lineNumber, section.Position.Column + section.Length));
        }

        static Line ReadVariable(string? filePath, string lineText, int lineNumber, TextSpan? section, TextSpan? subsection, int current, char c)
        {
            // Variable lines require a section
            if (section == null)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Variables must be declared within a section.",
                    errorPosition: new Position(lineNumber, lineText.Length));
            }

            if (!char.IsLetter(c))
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Variable name must start with a letter.",
                    errorPosition: new Position(lineNumber, current));
            }

            // We are already at the first non-whitespace character from ReadLine
            var start = current;
            var end = current = AdvanceWhile(lineText, start, c => char.IsLetterOrDigit(c) || c == '-');
            // We read to the end and it was all letters, digits or '-'
            if (end == -1)
            {
                return new Line(filePath, LineKind.Variable, lineNumber, lineText, section, subsection,
                    new TextSpan(lineText, new Position(lineNumber, start), lineText.Length - start));
            }

            // The next non-whitespace char must be =, # or ;
            current = AdvanceTo(lineText, ++current, c => !char.IsWhiteSpace(c));
            if (current == -1)
            {
                return new Line(filePath, LineKind.Variable, lineNumber, lineText, section, subsection,
                    new TextSpan(lineText, new Position(lineNumber, start), end - start + 1));
            }

            c = lineText[current];
            if (c != '=' && c != '#' && c != ';')
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Variable name can only contain letters, digits or '-'.",
                    errorPosition: new Position(lineNumber, end + 1));
            }

            var variable = new TextSpan(lineText, new Position(lineNumber, start), end - start + 1);
            if (c == '#' || c == ';')
            {
                return new Line(filePath, LineKind.Variable, lineNumber, lineText, section, subsection, variable,
                    comment: ReadComment(lineText, lineNumber, current));
            }

            return ReadValue(filePath, lineText, lineNumber, section, subsection, variable, current, c);
        }

        static Line ReadValue(string? filePath, string lineText, int lineNumber, TextSpan section, TextSpan? subsection, TextSpan variable, int current, char c)
        {
            var lineLength = lineText.Length;
            if (c == '=')
                current++;

            // Start at the first non-whitespace char or "
            var start = current = AdvanceTo(lineText, current, c => !char.IsWhiteSpace(c) || c == '"');

            if (start == -1)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Expected variable value after '='.",
                    errorPosition: new Position(lineNumber, lineLength));
            }

            // Variable has special escape rules, so we build its text once
            var builder = new StringBuilder(lineLength - start);
            int? quote = null;
            var escaped = false;

            // We are at the starting " char, skip it to begin subsection reading.
            while (current < lineLength)
            {
                c = lineText[current];
                if (c == '\\' && !escaped)
                {
                    escaped = true;
                    current++;
                    // To make the error message very specific, check for the specific allowed 
                    // escape sequences.
                    if (current < lineLength)
                    {
                        c = lineText[current];
                        switch (lineText[current])
                        {
                            case '"':
                            case '\\':
                                continue;
                            case 'n':
                                builder.Append(Environment.NewLine);
                                current++;
                                continue;
                            case 't':
                                builder.Append('\t');
                                current++;
                                continue;
                            default:
                                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                                    error: $"Invalid escape sequence '\\{c}'.",
                                    errorPosition: new Position(lineNumber, current));
                        }
                    }

                    continue;
                }

                if (c == '"' && !escaped)
                {
                    if (quote == null)
                        quote = current;
                    else
                        quote = null;

                    current++;
                    continue;
                }

                if (quote == null && (c == '#' || c == ';'))
                {
                    // Comment started
                    break;
                }

                if (char.IsWhiteSpace(c))
                {
                    // Detect inner vs outer spaces
                    var next = AdvanceWhile(lineText, current, char.IsWhiteSpace);
                    // If we reach EOL, drop the trailing whitespaces.
                    if (next == -1 || next + 1 >= lineLength)
                        break;

                    var nc = lineText[++next];
                    // Or if comment start is next instead of a closing quote
                    if (nc == '#' || nc == ';')
                        break;

                    for (var i = current; i < next; i++)
                        builder.Append(' ');

                    current = next;
                }
                else
                {
                    builder.Append(c);
                    current++;
                }

                escaped = false;
            }

            if (escaped)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Incomplete character escape.",
                    errorPosition: new Position(lineNumber, lineLength));
            }

            if (quote != null)
            {
                return new Line(filePath, LineKind.Error, lineNumber, lineText,
                    error: "Double quotes must be properly balanced or escaped with a backslash.",
                    errorPosition: new Position(lineNumber, quote.Value));
            }

            var text = builder.ToString();

            return new Line(filePath, LineKind.Variable, lineNumber, lineText, section, subsection, variable,
                new TextSpan(lineText, new Position(lineNumber, start), current, text),
                ReadComment(lineText, lineNumber, current));
        }

        ~ConfigReader() => Dispose(false);

        void Dispose(bool disposing)
        {
            reader?.Dispose();
            reader = null;
        }

        static int AdvanceTo(string lineText, int start, Func<char, bool> predicate)
        {
            for (var i = start; i < lineText.Length; i++)
            {
                if (predicate(lineText[i]))
                    return i;
            }
            return -1;
        }

        static int AdvanceWhile(string lineText, int start, Func<char, bool> predicate)
        {
            for (var i = start; i < lineText.Length; i++)
            {
                if (!predicate(lineText[i]))
                    return Math.Max(start, i - 1);
            }
            return -1;
        }

        static TextSpan? ReadComment(string lineText, int lineNumber, int start)
        {
            if (start >= lineText.Length)
                return null;

            start = AdvanceTo(lineText, start, c => c == '#' || c == ';');
            if (start >= lineText.Length || start == -1)
                return null;

            return new TextSpan(lineText, new Position(lineNumber, start), lineText.Length - start);
        }
    }
}
