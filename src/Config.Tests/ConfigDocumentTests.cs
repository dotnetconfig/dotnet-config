using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.DotNet
{
    public class ConfigDocumentTests
    {
        [Fact]
        public void can_load_document()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"# sample
[foo] # section
  bar = baz # variable
  ; done
");

            var doc = ConfigDocument.FromFile(path);

            Assert.Single(doc.Lines.OfType<SectionLine>());
            Assert.Single(doc.Lines.OfType<VariableLine>());
            Assert.Equal(2, doc.Lines.Where(line => line.GetType() == typeof(Line)).Count());
        }
    }
}
