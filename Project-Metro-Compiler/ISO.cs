using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Project_Metro_Compiler
{
    class ISO : IDisposable
    {
        //Use this to represent an ISO (see binso CD class). 
        //It would be useful to represent each section as an item within a struct, OR a class.
        readonly PrimaryVolume primaryVolume;
        IntPtr pPrimaryVolume;
        private bool disposedValue;

        public ISO()
        {
            primaryVolume = new();
        }
        public void Build()
        {
            pPrimaryVolume = primaryVolume.Build(out int allocationSize);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                Marshal.FreeHGlobal(pPrimaryVolume);
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ISO()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// <see cref="https://wiki.osdev.org/ISO_9660#The_Primary_Volume_Descriptor"/>
    /// </summary>
    public class PrimaryVolume
    {
        public const byte TYPE_CODE = 0x1;
        public const string STANDARD_IDENTIFIER = "CD001";
        public const byte VERSION = 0x01;
        public const byte UNUSED1 = 0x00;
        public string volumeIdentifier = "biniso primary volume"; //ToDo: This can be changed, but it must comply with standards of being up to 32 characters max.

        public enum Offsets : int
        {
            typeCode = 0,
            standardIdentifier = 1,
            version = 6,
            unused1 = 7,
            systemIdentifier = 8,
            volumeIdentifier = 40,
            unusedField1 = 72,
            volumeSpaceSize = 80,
            unsuedField2 = 88,
            volumeSetSize = 120,
            volumeSequenceNumber = 124,
            logicalBlockSize = 128,
            pathTableSize = 132,
            locationOfTypeLPathTable = 140,
            locationOfOptionalTypeLPathTable = 144,
            locationOfTypeMPathTable = 148,
            locationOfOptionalTypeMPathTable = 152,
            directoryEntrForTheRootDirectory = 156,
            volumeSetIdentifier = 190,
            publisherIdentifier = 318,
            dataPreparerIdentifier = 446,
            applicationIdentifier = 574,
            copyrightFileIdentifier = 702,
            abstractFileIdentifier = 739,
            bibliographicFileIdentifier = 776,
            volumeCreationDateAndTime = 813,
            volumeModificationDateAndtime = 830,
            volumeExpirationDateAndTime = 847,
            volumeEffectiveDateAndTime = 864,
            fileStructureVersion = 881,
            unused2 = 882,
            applicationUsed = 883,
            reserved = 1395
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allocationSize"></param>
        /// <returns>A pointer to the unmanaged block of memory.</returns>
        public IntPtr Build(out int allocationSize)
        {
            allocationSize = 2048;
            IntPtr pPrimaryVolume = Marshal.AllocHGlobal(allocationSize);

            //Zero memory.
            for (int i = 0; i < allocationSize; i++)
                Marshal.WriteByte(pPrimaryVolume+i, 0);
            
            //Write the Type Code.
            Marshal.WriteByte(pPrimaryVolume + (int)Offsets.typeCode, TYPE_CODE);

            //Write the Standard Identifier.
            for (int i = 0; i < STANDARD_IDENTIFIER.Length; i++)
                Marshal.WriteByte(pPrimaryVolume + (int)Offsets.standardIdentifier + i, (byte)STANDARD_IDENTIFIER[i]);

            //Write the Version.
            Marshal.WriteByte(pPrimaryVolume+(int)Offsets.version, VERSION);

            //Write the first Unused byte.
            Marshal.WriteByte(pPrimaryVolume + (int)Offsets.unused1, UNUSED1);

            //Write the Volume Identifier.
            for (int i = 0; i < volumeIdentifier.Length; i++)
                Marshal.WriteByte(pPrimaryVolume + (int)Offsets.volumeIdentifier + i, (byte)volumeIdentifier[i]);

            //From Kris' research, everything else post this can be 0 and is thereby not required.

            //Return the PTR for freeing later
            return pPrimaryVolume;
        }
    };
}
