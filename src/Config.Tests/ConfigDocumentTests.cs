using System;
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
            Assert.Equal(2, doc.Lines.OfType<CommentLine>().Count());
        }

        [Fact]
        public void can_set_new_variable_new_section()
        {
            var path = Path.GetTempFileName();
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", "bar", "baz", "true");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved);
            Assert.Equal("foo", saved.Single().Section);
            Assert.Equal("bar", saved.Single().Subsection);
            Assert.Equal("baz", saved.Single().Name);
            Assert.Equal("true", saved.Single().Value);
        }

        [Fact]
        public void can_set_new_variable_existing_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true");
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", null, "baz", "false");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved.Skip(1));
            Assert.Equal("foo", saved.Skip(1).Single().Section);
            Assert.Equal("baz", saved.Skip(1).Single().Name);
            Assert.Equal("false", saved.Skip(1).Single().Value);
        }

        [Fact]
        public void can_set_existing_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true");
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", null, "bar", "false");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved);
            Assert.Equal("foo", saved.Single().Section);
            Assert.Equal("bar", saved.Single().Name);
            Assert.Equal("false", saved.Single().Value);
        }

        [Fact]
        public void can_replace_existing_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true # with a comment ; some # stuff """);
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", null, "bar", "false");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved);
            Assert.Equal("foo", saved.Single().Section);
            Assert.Equal("bar", saved.Single().Name);
            Assert.Equal("false", saved.Single().Value);
        }

        [Fact]
        public void throws_when_set_multivalued_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hello
    bar = world");
            var doc = ConfigDocument.FromFile(path);

            Assert.Throws<NotSupportedException>(() => doc.Set("foo", null, "bar", "bye"));
        }

        [Fact]
        public void can_unset_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true
    baz = false");
            var doc = ConfigDocument.FromFile(path);

            doc.UnSet("foo", null, "bar");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Empty(saved.Lines.OfType<VariableLine>().Where(v => v.Name == "bar"));
        }

        [Fact]
        public void throws_when_unset_multivalued_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hello
    bar = world");
            var doc = ConfigDocument.FromFile(path);

            Assert.Throws<NotSupportedException>(() => doc.UnSet("foo", null, "bar"));
        }

        [Fact]
        public void unset_variable_removes_empty_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true");
            var doc = ConfigDocument.FromFile(path);

            doc.UnSet("foo", null, "bar");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Empty(saved.Lines);
        }

        [Fact]
        public void can_unset_non_existent_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true");
            var doc = ConfigDocument.FromFile(path);

            doc.UnSet("foo", null, "baz");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved.Lines.OfType<VariableLine>());
        }

        [Fact]
        public void can_add_multi_valued_variable()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hello
    # should come after this

    [other]
    # but definitely before this");
            var doc = ConfigDocument.FromFile(path);

            doc.Add("foo", default, "bar", "bye");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Equal(2, saved.Lines.OfType<VariableLine>().Where(x => x.Name == "bar").Count());
            Assert.Equal("hello", saved.Lines.OfType<VariableLine>().First().Value);
            Assert.Equal("bye", saved.Lines.OfType<VariableLine>().Skip(1).First().Value);
        }

        [Fact]
        public void can_set_all_multi_valued_variables()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hello
    bar = bye");
            var doc = ConfigDocument.FromFile(path);

            doc.SetAll("foo", default, "bar", "empty");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Equal(2, saved.Lines.OfType<VariableLine>().Where(x => x.Name == "bar").Count());
            Assert.All(saved.Lines.OfType<VariableLine>(), line => Assert.Equal("empty", line.Value));
        }

        [Fact]
        public void can_unset_all_multi_valued_variables()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hello
    # comment
    bar = bye
    # comment");
            var doc = ConfigDocument.FromFile(path);

            doc.UnSetAll("foo", default, "bar");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Empty(saved.Lines.OfType<VariableLine>());
            Assert.Equal(2, saved.Lines.OfType<CommentLine>().Count());
        }

        [Fact]
        public void unset_all_removes_empty_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hello
    bar = bye

    [other]
    yet = another");
            var doc = ConfigDocument.FromFile(path);

            doc.UnSetAll("foo", default, "bar");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved.Lines.OfType<SectionLine>());
        }
    }
}
