namespace Microsoft.DotNet
{
    public abstract class Line
    {
        string? text;

        protected Line(string? text) => this.text = text;

        public string Text 
        {
            get => text ?? ToString();
            internal set => text = value; 
        }
    }
}
