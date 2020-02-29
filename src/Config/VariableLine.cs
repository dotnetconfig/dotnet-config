namespace Microsoft.DotNet
{
    public class VariableLine : Line
    {
        public VariableLine(string name, string? value, string? comment, string? text = null)
            : base(text)
        {
            Name = name;
            Value = value;
            Comment = comment;
        }

        public string Name { get; }

        public string? Value { get; }

        public string? Comment { get; }
    }
}
