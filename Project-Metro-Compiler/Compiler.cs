using System;
using System.IO;
namespace Project_Metro_Compiler
{
    class Compiler : ISO
    {
        readonly byte[] isoContent;
        public Compiler(byte[] data) : base(data)
        {
            isoContent = Build();
        }
        public void CreateIso(string fileName)
        {
            try
            {
                File.WriteAllBytes(fileName, isoContent);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
            }

        }

    }
}
