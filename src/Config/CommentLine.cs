namespace DotNetConfig
{
    class CommentLine : Line
    {
        public CommentLine(string comment, string? text = null)
            : base(text)
        {
            Comment = comment;
        }

        public string Comment { get; }

        public override string ToString() => "#" + Comment;
    }
}
