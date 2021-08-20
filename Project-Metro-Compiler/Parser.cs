using System;
using System.IO;
namespace Project_Metro_Compiler
{
    static class Parser
    {

        //Need to ensure the input file meets ISO standards. This needs discussing further.
        public static byte[] content;
        public static int Parse(string dataSourcePath)
        {
            try
            {
                content = File.ReadAllBytes(dataSourcePath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
                return -1;
            }
            return 0;
        }
    }
}
