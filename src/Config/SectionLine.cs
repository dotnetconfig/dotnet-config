using System.Text;
using Superpower.Parsers;

namespace Microsoft.DotNet
{
    public class SectionLine : Line
    {
        public SectionLine(string section, string? subsection, string? comment = null, string? text = null)
            : base(text)
        {
            Section = section;
            Subsection = subsection;
            Comment = comment;
        }

        public string Section { get; }

        public string? Subsection { get; }

        public string? Comment { get; }

        public override string ToString()
        {
            var sb = new StringBuilder("[" + Section);
            if (Subsection != null)
            {
                if (Subsection.IndexOf(' ') == -1)
                    sb = sb.Append(" " + Subsection);
                else
                    sb = sb.Append(" \"" + Subsection + "\"");
            }

            sb = sb.Append("]");

            if (Comment != null)
                sb = sb.Append(" # " + Comment);

            return sb.ToString();
        }
    }
}
