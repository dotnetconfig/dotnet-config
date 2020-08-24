using System;
using System.IO;
using System.Linq;
using Superpower;
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
        public void can_get_variables_by_regex()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
  hello = first
  hell = second
  bar = none
  elle = third
");

            var doc = ConfigDocument.FromFile(path);

            Assert.Equal(3, doc.GetAll("el").Count());
            Assert.Single(doc.GetAll("el", "on"));
            Assert.Equal(2, doc.GetAll("!hel").Count());
            Assert.Single(doc.GetAll("!hel", "!on"));
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
            Assert.Equal("baz", saved.Single().Variable);
            Assert.Equal("true", saved.Single().RawValue);
        }

        [Fact]
        public void set_variable_null_reads_as_boolean_true()
        {
            var path = Path.GetTempFileName();
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", "bar", "baz", "value");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);
            doc.Set("foo", "bar", "baz", null);

            // TODO: if we don't save and reload, there's an issue in updating 
            // the value
            doc.Save();
            saved = ConfigDocument.FromFile(path);

            Assert.Single(saved);
            Assert.Equal("foo", saved.Single().Section);
            Assert.Equal("bar", saved.Single().Subsection);
            Assert.Equal("baz", saved.Single().Variable);
            Assert.Equal("true", saved.Single().RawValue);
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
            Assert.Equal("baz", saved.Skip(1).Single().Variable);
            Assert.Equal("false", saved.Skip(1).Single().RawValue);
        }

        [Fact]
        public void can_set_many_variables_section()
        {
            var path = Path.GetTempFileName();
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", null, "bar", "false");
            doc.Save();
            doc.Set("foo", null, "baz", "true");
            doc.Save();
            doc.Set("foo", null, "weak");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved.Lines.OfType<SectionLine>());
            Assert.Equal(3, saved.Lines.OfType<VariableLine>().Count());
            Assert.Equal("weak", saved.Lines.OfType<VariableLine>().Last().Name);
        }

        [Theory]
        [InlineData("file", null, "[file]")]
        [InlineData("file", "app.config", "[file \"app.config\"]")]
        [InlineData("file", "with spaces", "[file \"with spaces\"]")]
        [InlineData("file", "with \\ slash", "[file \"with \\\\ slash\"]")]
        [InlineData("file", "with \" quote", "[file \"with \\\" quote\"]")]
        public void render_section(string section, string subsection, string expected)
        {
            Assert.Equal(expected, new SectionLine(section, subsection).Text);
        }

        [Fact]
        public void can_set_variable_matching_regex()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hi
    baz = bye");
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", null, "baz", "hi", ValueMatcher.From("y"));
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Equal("hi", saved.Where(x => x.Variable == "baz").First().RawValue);
        }

        [Fact]
        public void can_set_all_variables_matching_regex()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    source = github.com/kzu
    source = microsoft.com/kzu
    source = github.com/vga
    source = microsoft.com/vga");
            var doc = ConfigDocument.FromFile(path);

            doc.SetAll("foo", null, "source", "none", ValueMatcher.From("github\\.com"));
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Equal(2, saved.Where(x => x.RawValue == "none").Count());
        }

        [Fact]
        public void does_not_set_variable_not_matching_regex()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = hi
    baz = bye");
            var doc = ConfigDocument.FromFile(path);

            doc.Set("foo", null, "baz", "hi", ValueMatcher.From("blah"));
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Equal("bye", saved.Where(x => x.Variable == "baz").First().RawValue);
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
            Assert.Equal("bar", saved.Single().Variable);
            Assert.Equal("false", saved.Single().RawValue);
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
            Assert.Equal("bar", saved.Single().Variable);
            Assert.Equal("false", saved.Single().RawValue);
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

            doc.Unset("foo", null, "bar");
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

            Assert.Throws<NotSupportedException>(() => doc.Unset("foo", null, "bar"));
        }

        [Fact]
        public void unset_variable_removes_empty_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = true");
            var doc = ConfigDocument.FromFile(path);

            doc.Unset("foo", null, "bar");
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

            doc.Unset("foo", null, "baz");
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

            doc.UnsetAll("foo", default, "bar");
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

            doc.UnsetAll("foo", default, "bar");
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Single(saved.Lines.OfType<SectionLine>());
        }

        [Fact]
        public void can_unset_all_with_regex_filter()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    source = https://github.com/kzu
    source = https://github.com/xamarin
    source = https://github.com/microsoft
    source = https://microsoft.com/kzu
    source = https://nuget.org/kzu");
            var doc = ConfigDocument.FromFile(path);

            doc.UnsetAll("foo", default, "source", ValueMatcher.From("github\\.com"));
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Empty(saved.Lines.OfType<VariableLine>().Where(x => x.Value.Contains("github")));
        }

        [Fact]
        public void can_set_all_with_regex_filter()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    source = https://github.com/kzu
    source = https://github.com/xamarin
    source = https://github.com/microsoft
    source = https://microsoft.com/kzu
    source = https://nuget.org/kzu");
            var doc = ConfigDocument.FromFile(path);

            doc.SetAll("foo", default, "source", "https://dev.azure.com" , ValueMatcher.From("github\\.com"));
            doc.Save();

            var saved = ConfigDocument.FromFile(path);

            Assert.Empty(saved.Lines.OfType<VariableLine>().Where(x => x.Value.Contains("github")));
            Assert.Equal(3, saved.Lines.OfType<VariableLine>().Where(x => x.Value.Contains("dev.azure.com")).Count());
        }

        [Fact]
        public void find_multivalue_with_regex()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    source = https://github.com/kzu
    source = https://github.com/xamarin
    source = https://github.com/microsoft
    source = https://microsoft.com/kzu
    source = https://nuget.org/kzu");

            var doc = ConfigDocument.FromFile(path);

            Assert.Equal(3, doc.GetAll("foo", null, "source", ValueMatcher.From("github\\.com")).Count());
        }

        [Fact]
        public void can_remove_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = baz
    enabled
    # comment

    [bar]
    enabled = false
    ; comment

    [foo]
    # comment
    other = value
    ; comment
");

            var doc = ConfigDocument.FromFile(path);

            doc.RemoveSection("foo");

            Assert.Single(doc.Lines.OfType<SectionLine>());
            Assert.Single(doc.Lines.OfType<CommentLine>());
            Assert.IsNotType<EmptyLine>(doc.Lines.First());
            Assert.IsNotType<EmptyLine>(doc.Lines.Last());
        }

        [Fact]
        public void can_rename_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = baz
    enabled
    # comment
");

            var doc = ConfigDocument.FromFile(path);

            doc.RenameSection("foo", null, "bar", null);

            Assert.Single(doc.Lines.OfType<SectionLine>());
            Assert.Equal("bar", doc.Lines.OfType<SectionLine>().First().Section);

            doc.RenameSection("bar", null, "bar", "foo or baz");

            Assert.Single(doc.Lines.OfType<SectionLine>());
            Assert.Equal("bar", doc.Lines.OfType<SectionLine>().First().Section);
            Assert.Equal("foo or baz", doc.Lines.OfType<SectionLine>().First().Subsection);

            doc.RenameSection("bar", "foo or baz", "foo", "bar");

            Assert.Equal("foo", doc.Lines.OfType<SectionLine>().First().Section);
            Assert.Equal("bar", doc.Lines.OfType<SectionLine>().First().Subsection);
        }

        [Fact]
        public void can_rename_section_with_spaces()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo ""bar or baz""]
    url = https
");

            var doc = ConfigDocument.FromFile(path);

            doc.RenameSection("foo", "bar or baz", "foo", "bar");

            Assert.Single(doc.Lines.OfType<SectionLine>());
            Assert.Equal("foo", doc.Lines.OfType<SectionLine>().First().Section);
            Assert.Equal("bar", doc.Lines.OfType<SectionLine>().First().Subsection);
        }

        [Fact]
        public void can_rename_multiple_section()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, @"[foo]
    bar = baz

    [foo]
    enabled
");

            var doc = ConfigDocument.FromFile(path);

            doc.RenameSection("foo", null, "bar", null);

            Assert.All(doc.Lines.OfType<SectionLine>(), x => Assert.Equal("bar", x.Section));
        }

        [Fact]
        public void throws_if_invalid_arguments()
        {
            var doc = ConfigDocument.FromFile(Path.GetTempFileName());

            Assert.Throws<ParseException>(() => doc.Add("foo_bar", null, "baz", "hello"));
            Assert.Throws<ParseException>(() => doc.Set("foo", null, "1baz", "hello"));
            Assert.Throws<ParseException>(() => doc.SetAll("foo_bar", null, "baz", "hello"));
            Assert.Throws<ParseException>(() => doc.SetAll("foo", null, "1baz", "hello"));
            Assert.Throws<ParseException>(() => doc.RenameSection("foo_bar", null, "baz", null));
            Assert.Throws<ParseException>(() => doc.RenameSection("foo", null, "baz_baz", null));
        }

        [Fact]
        public void can_set_value_in_specific_section()
        {
            var temp = Path.GetTempFileName();
            File.WriteAllText(temp, @"[file ""bar""]
	etag = asdfasdfasdf
[file ""baz""]
	url = https://foo/app.config
[file ""last""]
	weak
	etag = 7d4fe7db35e
");

            var doc = ConfigDocument.FromFile(temp);
            doc.Set("file", "bar", "etag", "asdfafd");
        }
    }
}
