namespace Microsoft.DotNet
{
    public class Line
    {
        public Line(string? text) => Text = text ?? "";

        public string Text { get; internal set; }
    }
}
