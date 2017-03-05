using MagicOnion.CodeAnalysis;
using MagicOnion.Generator;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                { "u|unuseunityattr", "[optional, default=false]Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer", _ => { UnuseUnityAttr = true; } },
                { "c|conditionalsymbol=", "[optional, default=empty]conditional compiler symbol", x => { ConditionalSymbols.AddRange(x.Split(',')); } },
                { "n|namespace=", "[optional, default=MagicOnion]Set namespace root name", x => { NamespaceRoot = x; } },
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

            Test(definitions);

            Console.WriteLine("Method Collect Complete:" + sw.Elapsed.ToString());

            

            Console.WriteLine("Output Generation Start");
            sw.Restart();

            var texts = definitions
                .GroupBy(x => x.Namespace)
                .OrderBy(x => x.Key)
                .Select(x => new CodeTemplate()
                {
                    Namespace = x.Key,
                    Interfaces = x.ToArray()
                })
                .ToArray();

            var registerTemplate = new RegisterTemplate
            {
                Namespace = cmdArgs.NamespaceRoot,
                Interfaces = definitions.Where(x => x.IsServiceDifinition).ToArray(),
                UnuseUnityAttribute = cmdArgs.UnuseUnityAttr
            };

            var sb = new StringBuilder();
            sb.AppendLine(registerTemplate.TransformText());
            foreach (var item in texts)
            {
                sb.AppendLine(item.TransformText());
            }
            Output(cmdArgs.OutputPath, sb.ToString());

            Console.WriteLine("String Generation Complete:" + sw.Elapsed.ToString());
            Console.WriteLine();
        }

        static void Output(string path, string text)
        {
            path = path.Replace("global::", "");

            const string prefix = "[Out]";
            Console.WriteLine(prefix + path);

            var fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            System.IO.File.WriteAllText(path, text);
        }

        static void Test(InterfaceDefintion[] definitions)
        {
            //foreach (var method in definitions.SelectMany(x => x.Methods))
            //{
            //    // method.ReturnType;

            //    method.UnwrappedOriginalResposneTypeSymbol.

            //    foreach (var p in method.Parameters)
            //    {
            //        // p.TypeName
            //    }


            //}

        }
    }
}
