using System;
using System.Diagnostics;
using System.IO;
namespace Project_Metro_Compiler
{
    class Program
    {
        static int Main(string[] args)
        {

            // command line arguments should be provided for source & target file
            // [0] = source file, [1] = target file -- file extensions must be provided

            if(args.Length != 2)
            {
                Console.WriteLine($"Incorrect number of arguments given.\nExpected 2, got {args.Length}.");
                return -1;
            }
            if(!File.Exists("Resources\\License"))
            {
                Console.WriteLine("Unable to find nasm license file. Exiting\n");
                Environment.Exit(0);
            }


            Console.WriteLine(File.ReadAllText("Resources\\License"));
            Process pNasm = new()
            {
                StartInfo =
                {
                    FileName = "Resources\\nasm.exe",
                    Arguments = $"-f bin {AppDomain.CurrentDomain.BaseDirectory + args[0]} -o {AppDomain.CurrentDomain.BaseDirectory + args[1]}.bin"
                }
                
            };

            pNasm.Start();


            int hresult = Parser.Parse($"{args[1]}.bin");
            if (hresult == -1)
                throw new NotImplementedException("ToDo");

            Compiler compiler = new(Parser.content);
            compiler.CreateIso(args[1]);

            return 0;
        }
    }
}
