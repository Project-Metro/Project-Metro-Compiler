using System;
using static Project_Metro_Compiler.Parser;
namespace Project_Metro_Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            int hresult = Parse(@"C:\Users\Harry\AppData\Local\bin\NASM\boot.bin");

            ISO newIso = new ISO(content);
            newIso.Build();
        }
    }
}
