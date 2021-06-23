using System;
using static Project_Metro_Compiler.Parser;
namespace Project_Metro_Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            int hresult = Parse("testFile.bin");
            Compiler compiler = new Compiler(content);
            
        }
    }
}
