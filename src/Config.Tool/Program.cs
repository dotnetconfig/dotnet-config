using System;
using System.Linq;
using Mono.Options;

namespace VisualStudio
{
    class Program
    {
        static int Main(string[] args)
        {
            var help = false;
            var options = new OptionSet
            {
                { "?|h|help", "Display this help", h => help = h != null },
            };

            options.Parse(args);

            if (args.Length == 1 && help)
            {
                Console.Write($"Usage: {ThisAssembly.Metadata.AssemblyName} [command] [options]");
                return 0;
            }

            var commandArgs = args.ToList();

            // if (args.Length > 0 && commands.ContainsKey(args[0]))
            // {
            //     command = args[0];
            //     commandArgs.RemoveAt(0);
            // }

            // return await commands[command].ExecuteAsync(commandArgs, Console.Out);
            
            return 0;
        }
    }
}
