using System;
using System.Runtime.InteropServices;
using static Project_Metro_Compiler.MemoryMethods;
namespace Project_Metro_Compiler
{
    class ISO : ConsoleHelper, IDisposable
    {
        private readonly PrimaryVolume primaryVolume;
        private IntPtr pPrimaryVolume;
        private readonly BootRecord bootRecord;
        private IntPtr pBootRecord;
        private readonly VolumeDescriptorSetTermination vdst;
        private IntPtr pVdst;
        private readonly ValidationEntry validationEntry;
        private IntPtr pValidationEntry;
        private readonly DataVolume dataVolume;
        private IntPtr pDataVolume;
        private readonly EmptySector emptySector;
        private IntPtr pEmptySector;

        private bool disposedValue;

        protected ISO(byte[] dataSource)
        {

            primaryVolume = new();
            bootRecord = new();
            vdst = new();
            validationEntry = new();
            dataVolume = new(dataSource);
            emptySector = new();
        }
        protected byte[] Build()
        {

            Console.Write("Generating the empty sector...");
            pEmptySector = emptySector.Build(out int emptySectorSize);
            MarkLineAsComplete();

            Console.Write("Building primary volumes...");
            pPrimaryVolume = primaryVolume.Build(out int primaryVolumeSize);
            MarkLineAsComplete();

            Console.Write("Creating boot records...");
            pBootRecord = bootRecord.Build(out int bootRecordSize);
            MarkLineAsComplete();

            Console.Write("Marking the volume descriptor...");
            pVdst = vdst.Build(out int vdstSize);
            MarkLineAsComplete();

            Console.Write("Validating the validation entry...");
            pValidationEntry = validationEntry.Build(out int validationEntrySize);
            MarkLineAsComplete();

            Console.Write("Populating the data volume...");
            pDataVolume = dataVolume.Build(out int dataVolumeSize);
            MarkLineAsComplete();


            int isoSize = primaryVolumeSize + bootRecordSize + vdstSize + validationEntrySize + dataVolumeSize + emptySectorSize;
            byte[] isoContent = new byte[isoSize];

            int bIndex = 0;
            for (int i = 0; i < emptySectorSize; i++)
            {
                isoContent[bIndex] = 0;
                bIndex++;
            }
            for (int i = 0; i < primaryVolumeSize; i++)
            {
                isoContent[bIndex] = Marshal.ReadByte(pPrimaryVolume + i);
                bIndex++;
            }
            for (int i = 0; i < bootRecordSize; i++)
            {
                isoContent[bIndex] = Marshal.ReadByte(pBootRecord + i);
                bIndex++;
            }
            for (int i = 0; i < vdstSize; i++)
            {
                isoContent[bIndex] = Marshal.ReadByte(pVdst + i);
                bIndex++;
            }
            for (int i = 0; i < validationEntrySize; i++)
            {
                isoContent[bIndex] = Marshal.ReadByte(pValidationEntry + i);
                bIndex++;
            }
            for (int i = 0; i < dataVolumeSize; i++)
            {
                isoContent[bIndex] = Marshal.ReadByte(pDataVolume + i);
                bIndex++;
            }
            return isoContent;
        }

        private class EmptySector : ISector
        {
            public IntPtr Build(out int allocationSize)
            {
                allocationSize = 0x8000;
                IntPtr pEmptySector = Marshal.AllocHGlobal(allocationSize);
                ZeroMemory(pEmptySector, allocationSize);
                return pEmptySector;
            }
        }
        /// <summary>
        /// <see cref="https://wiki.osdev.org/ISO_9660#The_Primary_Volume_Descriptor"/>
        /// </summary>
        private class PrimaryVolume : ISector
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

                ZeroMemory(pPrimaryVolume, allocationSize);

                // Write the Type Code.
                Marshal.WriteByte(pPrimaryVolume + (int)Offsets.typeCode, TYPE_CODE);

                // Write the Standard Identifier.
                WriteString(pPrimaryVolume + (int)Offsets.standardIdentifier, STANDARD_IDENTIFIER);

                // Write the Version.
                Marshal.WriteByte(pPrimaryVolume + (int)Offsets.version, VERSION);

                // Write the first Unused byte.
                Marshal.WriteByte(pPrimaryVolume + (int)Offsets.unused1, UNUSED1);

                // Write the Volume Identifier.
                for (int i = 0; i < volumeIdentifier.Length; i++)
                    Marshal.WriteByte(pPrimaryVolume + (int)Offsets.volumeIdentifier + i, (byte)volumeIdentifier[i]);

                // From Kris' research, everything else post this can be 0 and is thereby not required.

                // Return the PTR for freeing later
                return pPrimaryVolume;
            }
        };
        /// <summary>
        /// <see cref="https://wiki.osdev.org/ISO_9660#The_Primary_Volume_Descriptor"/>
        /// </summary>
        private class BootRecord : ISector
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

                ZeroMemory(pBootRecord, allocationSize);

                // Write the Type.
                Marshal.WriteByte(pBootRecord + (int)Offsets.type, type);

                // Write the Identifier.
                WriteString(pBootRecord + (int)Offsets.identifier, IDENTIFIER);

                // Write the Version.
                Marshal.WriteByte(pBootRecord + (int)Offsets.version, VERSION);

                // Write the boot system identifier.
                for (int i = 0; i < bootSystemIdentifier.Length; i++)
                    Marshal.WriteByte(pBootRecord + (int)Offsets.bootSystemIdentifier + i, (byte)bootSystemIdentifier[i]);

                // Write the validation entry index.
                Marshal.WriteByte(pBootRecord + (int)Offsets.bootSystemUse, BOOT_SYSTEM_USE);

                return pBootRecord;
            }
        }
        /// <summary>
        /// <see cref="https://wiki.osdev.org/ISO_9660#The_Primary_Volume_Descriptor"/>
        /// </summary>
        private class VolumeDescriptorSetTermination : ISector
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

                ZeroMemory(pVDST, allocationSize);

                //Write the Type.
                Marshal.WriteByte(pVDST + (int)Offsets.Type, TYPE);

                //Write the Identifier.
                WriteString(pVDST + (int)Offsets.Identifier, IDENTIFIER);

                //Write the version
                Marshal.WriteByte(pVDST + (int)Offsets.Version, VERSION);


                return pVDST;
            }
        }
        /// <summary>
        /// <see cref="https://pdos.csail.mit.edu/6.828/2014/readings/boot-cdrom.pdf"/>
        /// </summary>
        private class ValidationEntry : ISector
        {
            private const byte HEADER_ID = 0x1;
            public byte platformId = 0;
            private const short RESERVED = 0;
            public string idString = "biniso boot record"; //Must be less or equal to 23 characters.
            private const short checksumWord = 0;
            private const byte KEY_BYTE_1 = 0x55;
            private const byte KEY_BYTE_2 = 0xAA;
            public byte bootIndicator = 0x88;
            public short sectorCount = 0x1;
            public int loadRba = 0x14;
            enum Offsets
            {
                headerId = 0,
                platformId = 1,
                reserved = 2,
                idString = 4,
                checksumWord = 28,
                keyByte1 = 30,
                keyByte2 = 31,

                bootIndicator = 32,
                bootMediaType = 33,
                loadSegment = 34,
                systemType = 36,
                unused1 = 37,
                sectorCount = 38,
                loadRba = 40,
                unused2 = 44,
            }
            public IntPtr Build(out int allocationSize)
            {
                allocationSize = 2048;
                IntPtr pValidationEntry = Marshal.AllocHGlobal(allocationSize);

                ZeroMemory(pValidationEntry, allocationSize);

                //Write the Header ID.
                Marshal.WriteByte(pValidationEntry + (int)Offsets.headerId, HEADER_ID);

                //Write the Platform ID.
                Marshal.WriteByte(pValidationEntry + (int)Offsets.platformId, platformId);

                //Write the reserved bytes.
                Marshal.WriteInt16(pValidationEntry + (int)Offsets.reserved, RESERVED);

                //Write the identifier
                WriteString(pValidationEntry + (int)Offsets.idString, idString);

                //Write the checksum word.
                Marshal.WriteInt16(pValidationEntry + (int)Offsets.checksumWord, checksumWord);

                //Write the key bytes.
                Marshal.WriteByte(pValidationEntry + (int)Offsets.keyByte1, KEY_BYTE_1);
                Marshal.WriteByte(pValidationEntry + (int)Offsets.keyByte2, KEY_BYTE_2);

                //Write the boot indicator.
                Marshal.WriteByte(pValidationEntry + (int)Offsets.bootIndicator, bootIndicator);

                //Write the sector count.
                Marshal.WriteInt16(pValidationEntry + (int)Offsets.sectorCount, sectorCount);

                //Write the load rba value.
                Marshal.WriteInt32(pValidationEntry + (int)Offsets.loadRba, loadRba);

                return pValidationEntry;
            }
        }

        /// <summary>
        /// Please add a source.
        /// </summary>
        private class DataVolume : ISector
        {
            private readonly byte[] dataSource;
            public DataVolume(byte[] dataSource)
            {
                this.dataSource = dataSource;
            }
            public IntPtr Build(out int allocationSize)
            {
                allocationSize = 2048;
                IntPtr pDataVolume = Marshal.AllocHGlobal(allocationSize);

                ZeroMemory(pDataVolume, allocationSize);

                for (int i = 0; i < dataSource.Length; i++)
                    Marshal.WriteByte(pDataVolume + i, dataSource[i]);

                return pDataVolume;

            }
        }
        private interface ISector
        {
            IntPtr Build(out int allocationSize);
        }

        #region dispose
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
                Marshal.FreeHGlobal(pValidationEntry);
                Marshal.FreeHGlobal(pDataVolume);
                Marshal.FreeHGlobal(pEmptySector);
                disposedValue = true;
            }
        }

        ~ISO()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}
