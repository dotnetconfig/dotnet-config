namespace Microsoft.DotNet
{
    public class SectionLine : Line
    {
        public SectionLine(string section, string? subsection, string? comment, string? text = null)
            : base(text)
        {
            Section = section;
            Subsection = subsection;
            Comment = comment;
        }

        public string Section { get; }

        public string? Subsection { get; }

        public string? Comment { get; }
    }
}
