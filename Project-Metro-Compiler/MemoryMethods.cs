using System;
using System.Runtime.InteropServices;

namespace Project_Metro_Compiler
{
    static class MemoryMethods
    {
        public static void ZeroMemory(IntPtr destination, int length)
        {
            for (int i = 0; i < length; i++)
                Marshal.WriteByte(destination + i, 0);
        }
        public static void WriteString(IntPtr destination, string value)
        {
            for (int i = 0; i < value.Length; i++)
                Marshal.WriteByte(destination + i, (byte)value[i]);

        }
    }
}
