using System.Text;
using Superpower.Parsers;

namespace Microsoft.DotNet
{
    public class SectionLine : Line
    {
        string section;
        string? subsection;

        public SectionLine(string section, string? subsection, string? comment = null, string? text = null)
            : base(text)
        {
            this.section = section;
            this.subsection = subsection;
            Comment = comment;
        }

        public string Section
        {
            get => section;
            internal set
            {
                if (value != section)
                {
                    // If previous value is found on the existing text, replace it in-place
                    if (Text.IndexOf(section) is int index &&
                        // On whether we can't find the value again (that would be an anomaly anyway)
                        // Or if we find more than one match
                        (index == -1 || Text.IndexOf(section, index + section.Length) != -1))
                    {
                        section = value;
                        Text = ToString();
                    }
                    else
                    {
                        // Replace in Text, preserves existing line formatting in the majority of cases.
                        Text = Text.Replace(section, value);
                        section = value;
                    }
                }
            }
        }

        public string? Subsection
        {
            get => subsection;
            internal set
            {
                if (value != subsection)
                {
                    // If previous value was null, we need to write out entire line
                    if (subsection == null ||
                        // If it wasn't null, it will depend
                        (Text.IndexOf(subsection) is int index &&
                            // On whether we can't find the value again (that would be an anomaly anyway)
                            // Or if we find more than one match
                            (index == -1 || Text.IndexOf(subsection, index + subsection.Length) != -1)))
                    {
                        subsection = value;
                        Text = ToString();
                    }
                    else
                    {
                        // Replace in Text, preserves existing line formatting in the majority of cases.
                        Text = Text.Replace(subsection, Serialize(value!));
                        subsection = value;
                    }
                }
            }
        }

        public string? Comment { get; }

        public override string ToString()
        {
            var sb = new StringBuilder("[" + Section);
            if (Subsection != null)
                sb = sb.Append(" ").Append(Serialize(Subsection));

            sb = sb.Append("]");

            if (Comment != null)
                sb = sb.Append(" # " + Comment);

            return sb.ToString();
        }

        string Serialize(string value)
        {
            if (value.IndexOfAny(new[] { ' ', '\\', '"', '.' }) == -1)
                return value;

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
