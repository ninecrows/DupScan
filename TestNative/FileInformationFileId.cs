using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace C9Native
{
    /// <summary>
    /// Retrieve the unique file ID for this file.
    /// </summary>
    public class FileInformationFileId
    {
        /// <summary>
        /// Return the failure status if any or null if object creation succeeded.
        /// </summary>
        public Win32ErrorCode Status { get; private set; }

        /// <summary>
        /// Returns true if the construction succeeded. If false then Status will return the failure information.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsOk => Status == null;

        /// <summary>
        /// Retrieve the path passed in on object creation.
        /// </summary>
        public string Path { get; }

        private readonly uint[] _serialNumber = new uint[2];

        /// <summary>
        /// Return the two 32 bit pieces of the volume serial number part of the file id.
        /// </summary>
        /// <param name="index">Index 0 or 1 to the piece of the volume serial number that is of interest.</param>
        /// <returns>32 bit piece of the volume serial number.</returns>
        // ReSharper disable once UnusedMember.Global
        public uint GetSerialNumber(int index)
        {
            return (_serialNumber[index]);
        }

        /// <summary>
        /// Retrieve a string form of the full 64 bit volume serial number.
        /// </summary>
        public string FullSerialNumber { get; private set; }

        /// <summary>
        /// Retrieve the 32 bit volume serial number.
        /// </summary>
        public string VolumeSerialNumber { get; private set; }

        private readonly uint[] _myFileIdentifier = new uint[4];
        
        /// <summary>
        /// Retrieve the four 32 bit pieces of the file identifier.
        /// </summary>
        /// <param name="index">Index 0..3 of the piece of the file identifier of interest.</param>
        /// <returns>32 bit piece of the file identifier.</returns>
        // ReSharper disable once UnusedMember.Global
        public uint GetFileIdentifier(int index)
        {
            return _myFileIdentifier[index];
        }

        /// <summary>
        /// Retrieve the file specific identifier. Needs to be combined with the volume serial number to be unique.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Return the full string that identifies this file containing both volume information and file information.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string FullIdentifier => FullSerialNumber + "@" + Identifier;

        /// <summary>
        /// RAII CTOR Retrieve the pieces of the file identifier.
        /// </summary>
        /// <param name="path">Path to the file we're interested in.</param>
        public FileInformationFileId(string path)
        {
            Path = path;

            using BaseFileHandle handle = BaseFileHandle.ReadOnlyHandleFactory(path);
            LoadValues(handle);
        }

        /// <summary>
        /// Get the unique file identifier for the file associated with the provided handle. This version uses a pre-opened handle.
        /// </summary>
        /// <param name="handle">Wrapper object around a native file handle.</param>
        // ReSharper disable once UnusedMember.Global
        public FileInformationFileId(BaseFileHandle handle)
        {
            Path = handle.Path;
            LoadValues(handle);
        }

        /// <summary>
        /// Fill in the contents of this object. Factored out common code from the file path and native handle based versions.
        /// </summary>
        /// <param name="handle">Native handle in wrapper for the file to be examined.</param>
        private void LoadValues(BaseFileHandle handle)
        {
            // Results go here.
            var fileId = new FILE_ID_INFO();

            bool ok = GetFileInformationByHandleEx(handle.Handle,
                FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                out fileId,
                (uint)Marshal.SizeOf(fileId));

            // If the retrieval succeeded then we need to pack up the returned data in a usable form for retrieval.
            if (ok)
            {
                _serialNumber[0] = fileId.vsn1;
                _serialNumber[1] = fileId.vsn2;

                FullSerialNumber = $"{fileId.vsn1:X8}-{fileId.vsn2:X8}";
                VolumeSerialNumber = $"{fileId.vsn1:X8}";

                _myFileIdentifier[0] = fileId.id1;
                _myFileIdentifier[1] = fileId.id2;
                _myFileIdentifier[2] = fileId.id3;
                _myFileIdentifier[3] = fileId.id4;

                Identifier = $"{fileId.id1:X8}-{fileId.id2:X8}-{fileId.id3:X8}-{fileId.id4:X8}";
            }
            else
            {
                Status = new Win32ErrorCode();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandleEx(IntPtr hFile, 
            FILE_INFO_BY_HANDLE_CLASS infoClass,
            //out FILE_ID_BOTH_DIR_INFO dirInfo, 
            out FILE_ID_INFO info,
            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]
            //out byte[] buffer,
            uint dwBufferSize);

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        // ReSharper disable once InconsistentNaming
        private enum FILE_INFO_BY_HANDLE_CLASS
        {
            FileBasicInfo = 0,
            FileStandardInfo = 1,
            FileNameInfo = 2,
            FileRenameInfo = 3,
            FileDispositionInfo = 4,
            FileAllocationInfo = 5,
            FileEndOfFileInfo = 6,
            FileStreamInfo = 7,
            FileCompressionInfo = 8,
            FileAttributeTagInfo = 9,
            FileIdBothDirectoryInfo = 10,// 0x0A
            FileIdBothDirectoryRestartInfo = 11, // 0xB
            FileIoPriorityHintInfo = 12, // 0xC
            FileRemoteProtocolInfo = 13, // 0xD
            FileFullDirectoryInfo = 14, // 0xE
            FileFullDirectoryRestartInfo = 15, // 0xF
            FileStorageInfo = 16, // 0x10
            FileAlignmentInfo = 17, // 0x11
            FileIdInfo = 18, // 0x12
            FileIdExtdDirectoryInfo = 19, // 0x13
            FileIdExtdDirectoryRestartInfo = 20, // 0x14
            MaximumFileInfoByHandlesClass
        }

#if false
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        // ReSharper disable once UnusedType.Local
        // ReSharper disable once InconsistentNaming
        private struct FILE_ID_BOTH_DIR_INFO
        {
            public uint NextEntryOffset;
            public uint FileIndex;
            public LargeInteger CreationTime;
            public LargeInteger LastAccessTime;
            public LargeInteger LastWriteTime;
            public LargeInteger ChangeTime;
            public LargeInteger EndOfFile;
            public LargeInteger AllocationSize;
            public uint FileAttributes;
            public uint FileNameLength;
            public uint EaSize;
            public char ShortNameLength;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string ShortName;
            public LargeInteger FileId;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1)]
            public string FileName;
        }
#endif

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        // ReSharper disable once InconsistentNaming
        private readonly struct FILE_ID_INFO
        {
            public readonly uint vsn1, vsn2;
            public readonly uint id1, id2, id3, id4;
        }

#if false
        [StructLayout(LayoutKind.Explicit)]
        private struct LargeInteger
        {
            [FieldOffset(0)]
            public int Low;
            [FieldOffset(4)]
            public int High;
            [FieldOffset(0)]
            public long QuadPart;

            // use only when QuadPart cannot be passed
            public long ToInt64()
            {
                return ((long)this.High << 32) | (uint)this.Low;
            }

            // just for demonstration
            public static LargeInteger FromInt64(long value)
            {
                return new LargeInteger
                {
                    Low = (int)(value),
                    High = (int)((value >> 32))
                };
            }

        }
#endif
    }
}
