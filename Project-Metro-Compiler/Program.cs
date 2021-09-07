using System;
using System.Diagnostics;
using System.IO;
namespace Project_Metro_Compiler
{
    class Program : ConsoleHelper
    {

        const string TITLE = @"
██████╗ ██████╗  ██████╗      ██╗███████╗ ██████╗████████╗    ███╗   ███╗███████╗████████╗██████╗  ██████╗ 
██╔══██╗██╔══██╗██╔═══██╗     ██║██╔════╝██╔════╝╚══██╔══╝    ████╗ ████║██╔════╝╚══██╔══╝██╔══██╗██╔═══██╗
██████╔╝██████╔╝██║   ██║     ██║█████╗  ██║        ██║       ██╔████╔██║█████╗     ██║   ██████╔╝██║   ██║
██╔═══╝ ██╔══██╗██║   ██║██   ██║██╔══╝  ██║        ██║       ██║╚██╔╝██║██╔══╝     ██║   ██╔══██╗██║   ██║
██║     ██║  ██║╚██████╔╝╚█████╔╝███████╗╚██████╗   ██║       ██║ ╚═╝ ██║███████╗   ██║   ██║  ██║╚██████╔╝
╚═╝     ╚═╝  ╚═╝ ╚═════╝  ╚════╝ ╚══════╝ ╚═════╝   ╚═╝       ╚═╝     ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ";
        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(TITLE);
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("A University of Lincoln UROS (2021) project by ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Harry G Riley (19701020)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(", ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Kristaps Jurkans (19701672)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(", and ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Olegs Jakovlevs (25187386)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine('.');

            // command line arguments should be provided for source & target file
            // [0] = source file, [1] = target file -- file extensions must be provided

            if (args.Length != 2)
            {
                Console.WriteLine($"Incorrect number of arguments given.\nExpected 2, got {args.Length}. Exiting");
                return -1;
            }
            if (!File.Exists("Resources/LICENSE"))
            {
                string license_path = Directory.GetCurrentDirectory() + "/Resources/LICENSE";
                Console.WriteLine("Unable to find NASM license file. Exiting\n");
                return -1;
            }
            Console.WriteLine("**********************************************************************");
            Console.WriteLine(File.ReadAllText("Resources/LICENSE"));
            Console.WriteLine("**********************************************************************");

            Console.WriteLine($"Current working directory is: {Directory.GetCurrentDirectory()}");

            // calling a command to check if NASM is installed on the machine
            Console.Write("Checking if NASM is installed...");
            Process pCheck = new(){
                StartInfo ={
                    FileName = "which",
                    Arguments = "nasm",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };

            try {
                pCheck.Start();
            } catch {
                throw;
            }

            string pCheckoutput = pCheck.StandardOutput.ReadToEnd();
            pCheck.WaitForExit();
            
            // if the command returns nothing, then no NASM install was found
            if (pCheckoutput == ""){
                MarkLineAsFailed();
                Console.WriteLine("NASM install not found, cannot continue.");
                Environment.Exit(-1);
            }

            MarkLineAsComplete();

            Console.Write("Creating binary file with NASM...");
            Process pNasm = new()
            {
                StartInfo =
                {
                    FileName = $"nasm",
                    Arguments = $"-f bin {AppDomain.CurrentDomain.BaseDirectory + args[0]} -o {AppDomain.CurrentDomain.BaseDirectory + args[1]}.bin",
                    RedirectStandardError = true
                }
            };

            try{
                pNasm.Start();
            }
            catch{
                throw;
            }

            string nasmResult = pNasm.StandardError.ReadToEnd();
            if (nasmResult != "")
            {
                MarkLineAsFailed();
                Console.WriteLine(nasmResult);
                Environment.Exit(0);
            }
            MarkLineAsComplete();

            Console.Write("Parsing binary file...");
            int hresult = Parser.Parse($"{AppDomain.CurrentDomain.BaseDirectory + args[1]}.bin");
            if (hresult == -1)
            {
                MarkLineAsFailed();
                Console.WriteLine("Failed to parse binary file. Exiting\n");
                return -1;
            }
            MarkLineAsComplete();

            Compiler compiler = new(Parser.content);

            Console.Write("Compiling data to ISO format...");
            compiler.CreateIso(AppDomain.CurrentDomain.BaseDirectory + args[1]);
            MarkLineAsComplete();
            Console.WriteLine("**********************************************************************");
            Console.WriteLine("ISO Generation Completed.");
            return 0;
        }
    }
    class ConsoleHelper
    {
        public static void MarkLineAsComplete()
        {
            ConsoleColor originalColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("COMPLETE!");
            Console.ForegroundColor = originalColour;
        }
        public static void MarkLineAsFailed()
        {
            ConsoleColor originalColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAILED!");
            Console.ForegroundColor = originalColour;
        }
    }
}
