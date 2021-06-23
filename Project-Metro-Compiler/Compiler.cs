using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void CreateIso()
        {
            //ToDo: add exception handling and naming of file.
            File.WriteAllBytes("testFile.iso", isoContent);
        }
  
    }
}
