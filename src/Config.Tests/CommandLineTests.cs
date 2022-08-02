using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Medallion.Collections;
using Xunit;

namespace DotNetConfig
{
    public class CommandLineTests
    {
        internal static string[] SeverityLevels => new[] { "info", "warn", "error" };

        [Fact]
        public void when_argument_no_default_then_sets_default()
        {
            var command = new Command("foo")
            {
                new Argument<string>("string"),
                new Argument<long>("long"),
                new Argument<int>("int"),
                new Argument<DateTime>("date"),
                new Argument<bool>("bool"),
                new Argument<string?>("string2"),
                new Argument<long?>("long2"),
                new Argument<int?>("int2"),
                new Argument<DateTime?>("date2"),
                new Argument<bool?>("bool2"),
            }.WithConfigurableDefaults();

            Assert.True(command.Arguments.All(x => x.HasDefaultValue));
        }

        [Fact]
        public void given_command_tree_sets_all_default()
        {
            var config = Config.Build(Path.GetTempFileName())
                .SetString("foo", "name", "foo")
                .SetString("foo", "bar", "name", "bar")
                .SetBoolean("foo", "baz", "force", true)
                .SetString("foo", "baz.update", "name", "baz");

            var command = new Command("foo")
            {
                new Command("bar")
                {
                    new Argument<string?>("name"),
                },
                new Command("baz")
                {
                    new Command("update")
                    {
                        new Argument<string?>("name"),
                    },
                    new Command("install")
                    {
                        new Argument<string?>("name"),
                    },
                    new Command("uninstall")
                    {
                        new Argument<string?>("name"),
                        new Option<bool?>("force"),
                    },
                },
            }.WithConfigurableDefaults(configuration: config);

            Assert.True(Traverse
                .BreadthFirst(command, c => c.Children.OfType<Command>())
                .SelectMany(x => x.Arguments).All(x => x.HasDefaultValue));

            // Gets from the command-specific section
            Assert.Equal("bar", command.Children.OfType<Command>().First().Arguments[0].GetDefaultValue());
            Assert.Equal("baz", Traverse
                .DepthFirst(command, c => c.Children.OfType<Command>())
                .First(c => c.Name == "update")
                .Arguments[0].GetDefaultValue());

            // Uses default from lifted top-level section for shared "name" argument
            Assert.Equal("foo", Traverse
                .DepthFirst(command, c => c.Children.OfType<Command>())
                .First(c => c.Name == "install")
                .Arguments[0].GetDefaultValue());

            // Non-shared but still lifted since no conflicts
            Assert.True(Traverse
                .DepthFirst(command, c => c.Children.OfType<Command>())
                .First(c => c.Name == "uninstall")
                .Options.OfType<IOption>().First().Argument.GetDefaultValue() as bool?);
        }

        [Fact]
        public void given_string_array_sets_all_values()
        {
            var config = Config.Build(Path.GetTempFileName())
                .AddString("cli", "include", "foo")
                .AddString("cli", "include", "bar")
                .AddString("cli", "include", "baz");

            var command = new RootCommand()
            {
                new Argument<string[]>("include"),
            }.WithConfigurableDefaults("cli", configuration: config);

            Assert.Equal(new[] { "foo", "bar", "baz" }, (string[]?)command.Arguments[0].GetDefaultValue());

            command.Handler = CommandHandler.Create<string[]>(include =>
                Assert.Equal(new[] { "foo", "bar", "baz" }, include));

            new CommandLineBuilder(command).Build().Invoke(new string[] { });
        }

        [Fact]
        public void given_string_list_sets_all_values()
        {
            var config = Config.Build(Path.GetTempFileName())
                .AddString("cli", "include", "foo")
                .AddString("cli", "include", "bar")
                .AddString("cli", "include", "baz");

            var command = new RootCommand()
            {
                new Argument<List<string>>("include"),
            }.WithConfigurableDefaults("cli", configuration: config);

            Assert.NotNull(command.Arguments[0].GetDefaultValue());
            Assert.Equal(new[] { "foo", "bar", "baz" }, (IEnumerable<string>)command.Arguments[0].GetDefaultValue()!);

            command.Handler = CommandHandler.Create<List<string>>(include =>
                Assert.Equal(new[] { "foo", "bar", "baz" }, include));

            new CommandLineBuilder(command).Build().Invoke(new string[] { });
        }

        [Fact]
        public void given_string_ilist_sets_all_values()
        {
            var config = Config.Build(Path.GetTempFileName())
                .AddString("cli", "include", "foo")
                .AddString("cli", "include", "bar")
                .AddString("cli", "include", "baz");

            var command = new RootCommand()
            {
                new Argument<IList<string>>("include"),
            }.WithConfigurableDefaults("cli", configuration: config);

            Assert.NotNull(command.Arguments[0].GetDefaultValue());
            Assert.Equal(new[] { "foo", "bar", "baz" }, (IEnumerable<string>)command.Arguments[0].GetDefaultValue()!);

            command.Handler = CommandHandler.Create<IList<string>>(include =>
                Assert.Equal(new[] { "foo", "bar", "baz" }, include));

            new CommandLineBuilder(command).Build().Invoke(new string[] { });
        }

        [Fact]
        public void given_string_icollection_sets_all_values()
        {
            var config = Config.Build(Path.GetTempFileName())
                .AddString("cli", "include", "foo")
                .AddString("cli", "include", "bar")
                .AddString("cli", "include", "baz");

            var command = new RootCommand()
            {
                new Argument<ICollection<string>>("include"),
            }.WithConfigurableDefaults("cli", configuration: config);

            Assert.NotNull(command.Arguments[0].GetDefaultValue());
            Assert.Equal(new[] { "foo", "bar", "baz" }, (IEnumerable<string>)command.Arguments[0].GetDefaultValue()!);

            command.Handler = CommandHandler.Create<ICollection<string>>(include =>
                Assert.Equal(new[] { "foo", "bar", "baz" }, include));

            new CommandLineBuilder(command).Build().Invoke(new string[] { });
        }

        [Fact]
        public void given_int_array_sets_all_values()
        {
            var config = Config.Build(Path.GetTempFileName())
                .AddNumber("cli", "include", 25)
                .AddNumber("cli", "include", 50)
                .AddNumber("cli", "include", 100);

            var command = new RootCommand()
            {
                new Argument<int[]>("include"),
            }.WithConfigurableDefaults("cli", configuration: config);

            command.Handler = CommandHandler.Create<int[]>(include =>
                Assert.Equal(new[] { 25, 50, 100 }, include));

            new CommandLineBuilder(command).Build().Invoke(new string[] { });
        }

        [Fact]
        public void given_long_array_sets_all_values()
        {
            var config = Config.Build(Path.GetTempFileName())
                .AddNumber("cli", "include", 25)
                .AddNumber("cli", "include", 50)
                .AddNumber("cli", "include", 100);

            var command = new RootCommand()
            {
                new Argument<long[]>("include"),
            }.WithConfigurableDefaults("cli", configuration: config);

            command.Handler = CommandHandler.Create<long[]>(include =>
                Assert.Equal(new long[] { 25, 50, 100 }, include));

            new CommandLineBuilder(command).Build().Invoke(new string[] { });
        }

        [Fact]
        public void SetDefaults()
        {
            // Sync changes to option and argument names with the FormatCommant.Handler above.
            var rootCommand = new RootCommand
            {
                new Argument<string?>("workspace")
                {
                    Arity = ArgumentArity.ZeroOrOne,
                }.LegalFilePathsOnly(),
                new Option(new[] { "--no-restore" }),
                new Option(new[] { "--folder", "-f" }),
                new Option(new[] { "--fix-whitespace", "-w" }),
                new Option<string?>(new[] { "--fix-style", "-s" }) { Name = "severity" }.FromAmong(SeverityLevels),
                new Option<string?>(new[] { "--fix-analyzers", "-a" }).FromAmong(SeverityLevels),
                //{
                    //Argument = new Argument<string?>("severity") { Arity = ArgumentArity.ZeroOrOne }.FromAmong(SeverityLevels)
                //},
                new Option<string[]>(new[] { "--diagnostics" }, () => Array.Empty<string>())
                {
                    //Argument = new Argument<string[]>(() => Array.Empty<string>())
                },
                new Option(new[] { "--include" })
                {
                    //Argument = new Argument<string[]>(() => Array.Empty<string>())
                },
                new Option(new[] { "--exclude" })
                {
                    //Argument = new Argument<string[]>(() => Array.Empty<string>())
                },
                new Option(new[] { "--check" }),
                new Option(new[] { "--report" })
                {
                    //Argument = new Argument<string?>(() => null) { Name = "report-path" }.LegalFilePathsOnly()
                },
                new Option(new[] { "--verbosity", "-v" })
                {
                    //Argument = new Argument<string?>() { Arity = ArgumentArity.ExactlyOne }.FromAmong(VerbosityLevels)
                },
                new Option(new[] { "--include-generated" })
                {
                    IsHidden = true
                },
                new Option(new[] { "--binarylog" })
                {
                    //Argument = new Argument<string?>(() => null) { Name = "binary-log-path", Arity = ArgumentArity.ZeroOrOne }.LegalFilePathsOnly()
                },
            };
        }
    }
}
