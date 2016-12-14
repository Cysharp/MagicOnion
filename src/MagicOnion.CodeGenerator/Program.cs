using MagicOnion.CodeAnalysis;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.CodeGenerator
{
    class CommandlineArguments
    {
        // moc.exe

        public string InputPath { get; private set; }
        public string OutputPath { get; private set; }
        public bool UnuseUnityAttr { get; private set; }
        public List<string> ConditionalSymbols { get; private set; }
        public bool IsSeparate { get; private set; }
        public string NamespaceRoot { get; private set; }

        public bool IsParsed { get; set; }

        public CommandlineArguments(string[] args)
        {
            ConditionalSymbols = new List<string>();
            NamespaceRoot = "MagicOnion";

            var option = new OptionSet()
            {
                { "i|input=", "[required]Input path of analyze csproj", x => { InputPath = x; } },
                { "o|output=", "[required]Output path(file) or directory base(in separated mode)", x => { OutputPath = x; } },
                { "s|separate", "[optional, default=false]Output files are separated", _ => { IsSeparate = true; } },
                { "u|unuseunityattr", "[optional, default=false]Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on ZeroFormatterInitializer", _ => { UnuseUnityAttr = true; } },
                { "c|conditionalsymbol=", "[optional, default=empty]conditional compiler symbol", x => { ConditionalSymbols.AddRange(x.Split(',')); } },
                { "n|namespace=", "[optional, default=ZeroFormatter]Set namespace root name", x => { NamespaceRoot = x; } },
            };
            if (args.Length == 0)
            {
                goto SHOW_HELP;
            }
            else
            {
                option.Parse(args);

                if (InputPath == null || OutputPath == null)
                {
                    Console.WriteLine("Invalid Argument:" + string.Join(" ", args));
                    Console.WriteLine();
                    goto SHOW_HELP;
                }

                IsParsed = true;
                return;
            }

            SHOW_HELP:
            Console.WriteLine("moc arguments help:");
            option.WriteOptionDescriptions(Console.Out);
            IsParsed = false;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var cmdArgs = new CommandlineArguments(args);
            if (!cmdArgs.IsParsed)
            {
                return;
            }

            // Generator Start...

            var sw = Stopwatch.StartNew();
            Console.WriteLine("Project Compilation Start:" + cmdArgs.InputPath);

            var collector = new MethodCollector(cmdArgs.InputPath, cmdArgs.ConditionalSymbols);

            Console.WriteLine("Project Compilation Complete:" + sw.Elapsed.ToString());
            Console.WriteLine();

            sw.Restart();
            Console.WriteLine("Method Collect Start");

            var definitions = collector.Visit();

            Console.WriteLine("Method Collect Complete:" + sw.Elapsed.ToString());

            // TODO:Generate!
        }
    }
}
