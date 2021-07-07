using System;
using static Project_Metro_Compiler.Parser;
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

            int hresult = Parse(args[0]);
            Compiler compiler = new Compiler(content);
            compiler.CreateIso(args[1]);
            return 0;
        }
    }
}
