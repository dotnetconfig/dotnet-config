using System;
using System.Text;

namespace DotNetConfig
{
    internal record Line
    {
        public static Line CreateSection(string? filePath, int lineNumber, string section, string? subsection)
        {
            var lineText = subsection == null ?
                "[" + section + "]" :
                "[" + section + " \"" + TextRules.SerializeSubsection(subsection) + "\"]";

            return ConfigReader.ParseSection(filePath, lineText, lineNumber);
        }

        public static Line CreateVariable(string? filePath, int lineNumber, TextSpan? section, TextSpan? subsection, string name, string? value)
        {
            var lineText = value == null ?
                "\t" + name :
                "\t" + name + " = " + TextRules.SerializeValue(value);

            return ConfigReader.ParseVariable(filePath, lineText, lineNumber, section, subsection);
        }

        public Line(
            string? filePath, LineKind kind,
            int lineNumber, string lineText,
            TextSpan? section = null, TextSpan? subsection = null,
            TextSpan? name = null, TextSpan? value = null,
            TextSpan? comment = null,
            string? error = null,
            Position? errorPosition = null)
        {
            FilePath = filePath;
            Kind = kind;
            LineNumber = lineNumber;
            LineText = lineText;
            Section = section;
            Subsection = subsection;
            Variable = name;
            Value = value;
            Comment = comment;
            Error = error;
            ErrorPosition = errorPosition;
        }

        public string? FilePath { get; }

        public LineKind Kind { get; }

        public int LineNumber { get; }

        public string LineText { get; private init; }

        public TextSpan? Section { get; private init; }

        public TextSpan? Subsection { get; private init; }

        public TextSpan? Variable { get; private init; }

        public TextSpan? Value { get; private init; }

        public TextSpan? Comment { get; private init; }

        public string? Error { get; }

        public Position? ErrorPosition { get; }

        public override string ToString() => LineText;

        internal Line WithSection(string section, string? subsection)
        {
            var original = LineText;
            var builder = new StringBuilder(original.Length + section.Length + (subsection == null ? 0 : subsection.Length));

            if (Section == null)
                throw new InvalidOperationException("Expected non-null Section");

            builder.Append(original.Substring(0, Section.Position.Absolute));
            builder.Append(section);

            if (Subsection != null)
            {
                var start = Section.Position.Absolute + Section.Length;
                var length = Subsection.Position.Absolute - start;

                builder.Append(original.Substring(start, length));

                if (subsection != null)
                    builder.Append('\"').Append(TextRules.SerializeSubsection(subsection)).Append('\"');

                builder.Append(original.Substring(Subsection.Position.Absolute + Subsection.Length));
            }
            else
            {
                if (subsection != null)
                    builder.Append(" \"").Append(TextRules.SerializeSubsection(subsection)).Append('\"');

                builder.Append(original.Substring(Section.Position.Absolute + Section.Length));
            }

            var lineText = builder.ToString();
            var parsed = ConfigReader.ParseSection(FilePath, lineText, LineNumber);

            return this with
            {
                LineText = lineText,
                Section = parsed.Section,
                Subsection = parsed.Subsection,
                Comment = parsed.Comment,
            };
        }

        internal Line WithSection(TextSpan section, TextSpan? subsection)
            => this with
            {
                Section = section,
                Subsection = subsection,
            };

        internal Line WithValue(string? value)
        {
            if (Kind != LineKind.Variable)
                throw new NotSupportedException();

            var original = LineText;
            var builder = new StringBuilder(original.Length + (value == null ? 0 : value.Length));

            builder.Append(original.Substring(0, Variable!.Position.Absolute + Variable!.Length));

            if (value != null)
            {
                builder.Append(" = ");
                builder.Append(TextRules.SerializeValue(value));
            }

            if (Comment != null)
                builder.Append(" ").Append(Comment.Text);

            var lineText = builder.ToString();
            var parsed = ConfigReader.ParseVariable(FilePath, lineText, LineNumber, Section, Subsection);

            return this with
            {
                LineText = lineText,
                Variable = parsed.Variable,
                Value = parsed.Value,
                Comment = parsed.Comment,
            };
        }
    }
}
