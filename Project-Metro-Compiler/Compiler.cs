using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Metro_Compiler
{
    class Compiler : ISO
    {
        public Compiler(byte[] data) : base(data)
        {
            Build();
        }
        //Kris will convert the input file (.bin expected?
        //Discussion about using direct .asm file if we can convert to bytecode, but likely overkill) into an ISO here.
    }
}
