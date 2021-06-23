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
        private readonly PrimaryVolume primaryVolume;
        private IntPtr pPrimaryVolume;
        private readonly BootRecord bootRecord;
        private IntPtr pBootRecord;
        private readonly VolumeDescriptorSetTermination vdst;
        private IntPtr pVdst;

        private bool disposedValue;

        public ISO()
        {
            primaryVolume = new();
            bootRecord = new();
            vdst = new();
        }
        public void Build()
        {
            pPrimaryVolume = primaryVolume.Build(out _);
            pBootRecord = bootRecord.Build(out _);
            pVdst = vdst.Build(out _);
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
                Marshal.FreeHGlobal(pBootRecord);
                Marshal.FreeHGlobal(pVdst);
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
    public class PrimaryVolume : ISector
    {
        public const byte TYPE_CODE = 0x1;
        public const string STANDARD_IDENTIFIER = "CD001";
        public const byte VERSION = 0x01;
        public const byte UNUSED1 = 0x00;
        public string volumeIdentifier = "biniso primary volume"; //ToDo: This can be changed, but it must comply with standards of being up to 32 characters max.

        private enum Offsets : int
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
    public class BootRecord : ISector
    {
        public byte type = 0;
        public const string IDENTIFIER = "CD001";
        public const byte VERSION = 0x1;
        public string bootSystemIdentifier = "EL TORITO SPECIFICATION";
        public const byte BOOT_SYSTEM_USE = 0x13; //ToDo: Kris, this is hardcoded in your code for valdiation entry, will it always be 0x13?
        private enum Offsets
        {
            type = 0,
            identifier = 1,
            version = 6,
            bootSystemIdentifier = 7,
            bootIdentifier = 39,
            bootSystemUse = 71,
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allocationSize"></param>
        /// <returns></returns>
        public IntPtr Build(out int allocationSize)
        {
            allocationSize = 2048;
            IntPtr pBootRecord = Marshal.AllocCoTaskMem(allocationSize);

            //Zero memory.
            for (int i = 0; i < allocationSize; i++)
                Marshal.WriteByte(pBootRecord + i, 0);

            //Write the Type.
            Marshal.WriteByte(pBootRecord + (int)Offsets.type, type);

            //Write the Identifier.
            for (int i = 0; i < IDENTIFIER.Length; i++)
                Marshal.WriteByte(pBootRecord + (int)Offsets.identifier + i, (byte)IDENTIFIER[i]);

            //Write the Version.
            Marshal.WriteByte(pBootRecord + (int)Offsets.version, VERSION);

            //Write the boot system identifier.
            for (int i = 0; i < bootSystemIdentifier.Length; i++)
                Marshal.WriteByte(pBootRecord + (int)Offsets.bootSystemIdentifier + i, (byte)bootSystemIdentifier[i]);


            //Write the validation entry index.
            Marshal.WriteByte(pBootRecord + (int)Offsets.bootSystemUse, BOOT_SYSTEM_USE);

            return pBootRecord;
        }
    }
    public class VolumeDescriptorSetTermination : ISector
    {
        private const byte TYPE = 0xFF;
        private const string IDENTIFIER = "CD001";
        private const byte VERSION = 0x1;
        enum Offsets
        {
            Type = 0,
            Identifier = 1,
            Version = 6,
        }
        public IntPtr Build(out int allocationSize)
        {
            allocationSize = 2048;
            //Pad the sector to 2048 per Kris's request.
            IntPtr pVDST = Marshal.AllocHGlobal(allocationSize);

            //Zero memory.
            for (int i = 0; i < allocationSize; i++)
                Marshal.WriteByte(pVDST + i, 0);

            //Write the Type.
            Marshal.WriteByte(pVDST + (int)Offsets.Type, TYPE);

            //Write the Identifier.
            for (int i = 0; i < IDENTIFIER.Length; i++)
                Marshal.WriteByte(pVDST + (int)Offsets.Identifier + i, (byte)IDENTIFIER[i]);

            //Write the version
            Marshal.WriteByte(pVDST + (int)Offsets.Version, VERSION);


            return pVDST;
        }
    }
    public interface ISector
    {
        IntPtr Build(out int allocationSize);
    }
}
