using System.Text;

namespace Microsoft.DotNet
{
    public class VariableLine : Line
    {
        string? value;

        public VariableLine(string name, string? value, string? comment = null, string? text = null)
            : base(text)
        {
            Name = name;
            this.value = value;
            Comment = comment;
        }

        public string Name { get; }

        public string? Value 
        {
            get => value;
            internal set
            {
                if (value != this.value)
                {
                    // If previous value was null, we need to write out entire line
                    if (this.value == null || 
                        // If it wasn't null, it will depend
                        (Text.IndexOf(this.value) is int index && 
                            // On whether we can't find the value again (that would be an anomaly anyway)
                                            // Or if we find more than one match
                            (index == -1 || Text.IndexOf(this.value, index + this.value.Length) != -1)))
                    {
                        this.value = value;
                        Text = ToString();
                    }
                    else
                    {
                        // Replace in Text, preserves existing line formatting in the majority 
                        // of cases.
                        Text = Text.Replace(this.value, Serialize(value!));
                        this.value = value;
                    }
                }
            } 
        }

        public string? Comment { get; }

        public override string ToString()
        {
            var sb = new StringBuilder("\t").Append(Name);
            if (Value != null)
                sb = sb.Append(" ").Append("=").Append(" ").Append(Serialize(Value));

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
