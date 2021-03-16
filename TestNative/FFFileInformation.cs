using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace C9Native
{
    /// <summary>
    /// Mode select enumeration for GetFileInformationByHandleExW(...)
    /// </summary>
     [SuppressMessage("ReSharper", "UnusedMember.Global")]
     [SuppressMessage("ReSharper", "CommentTypo")]
     [SuppressMessage("ReSharper", "IdentifierTypo")]
    // ReSharper disable once ArrangeTypeModifiers
    // ReSharper disable once InconsistentNaming
    internal enum FILE_INFO_BY_HANDLE_CLASS
    {
        /// <summary>
        /// Select FILE_BASIC_INFO return
        /// </summary>
        FileBasicInfo,

        /// <summary>
        /// Select FILE_STANDARD_INFO return
        /// </summary>
        FileStandardInfo,

        /// <summary>
        /// Select FILE_NAME_INFO return
        /// </summary>
        FileNameInfo,

        /// <summary>
        /// Select FILE_RENAME_INFO return from GetFileInformationByHandleExW()
        /// </summary>
        FileRenameInfo,

        /// <summary>
        /// Select FILE_DISPOSITION_INFO return
        /// </summary>
        FileDispositionInfo,

        /// <summary>
        /// Select FILE ALLOCATION INFO. return
        /// </summary>
        FileAllocationInfo,

        /// <summary>
        /// Select FILE_END_OF_FILE_INFO return
        /// </summary>
        FileEndOfFileInfo,

        /// <summary>
        /// Select FILE_STREAM_INFO return
        /// </summary>
        FileStreamInfo,

        /// <summary>
        /// Select FILE_COMPRESSION_INFO return
        /// </summary>
        FileCompressionInfo,

        /// <summary>
        /// Select FILE_ATTRIBUTE_TAG_INFO return
        /// </summary>
        FileAttributeTagInfo,

        /// <summary>
        /// Select FILE_ID_BOTH_DIR_INFO return
        /// </summary>
        FileIdBothDirectoryInfo,

        /// <summary>
        /// Select FILE_ID_BOTH_DIR_INFO return
        /// </summary>
        FileIdBothDirectoryRestartInfo,

        /// <summary>
        /// Select FILE_IO_PRIORITY_HINT_INFO return
        /// </summary>
        FileIoPriorityHintInfo,

        /// <summary>
        /// Select FILE_REMOTE_PROTOCOL_INFO return
        /// </summary>
        FileRemoteProtocolInfo,

        /// <summary>
        /// Select FILE_FULL_DIR_INFO return
        /// </summary>
        FileFullDirectoryInfo,

        /// <summary>
        /// Select FILE_FULL_DIR_INFO return
        /// </summary>
        FileFullDirectoryRestartInfo,

        /// <summary>
        /// Select FILE_STORAGE_INFO return
        /// </summary>
        FileStorageInfo,
        
        /// <summary>
        /// Select FILE_ALIGNMENT_INFO return
        /// </summary>
        FileAlignmentInfo,

        /// <summary>
        /// Select FILE_ID_INFO return
        /// </summary>
        FileIdInfo,

        /// <summary>
        /// Select FILE_ID_EXTD_DIR_INFO return
        /// </summary>
        FileIdExtdDirectoryInfo,

        /// <summary>
        /// Select FILE_ID_EXTD_DIR_INFO return
        /// </summary>
        FileIdExtdDirectoryRestartInfo,

        /// <summary>
        /// 
        /// </summary>
        FileDispositionInfoEx,

        /// <summary>
        /// 
        /// </summary>
        FileRenameInfoEx,

        /// <summary>
        /// End of this enumeration
        /// </summary>
        MaximumFileInfoByHandleClass
    };

    /// <summary>
    /// List of file system features needed externally for the moment.
    /// </summary>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum FileSystemFeature : uint
    {
        /// <summary>
        /// The file system preserves the case of file names when it places a name on disk.
        /// </summary>
        CasePreservedNames = 2,

        /// <summary>
        /// The file system supports case-sensitive file names.
        /// </summary>
        CaseSensitiveSearch = 1,

        /// <summary>
        /// The specified volume is a direct access (DAX) volume. This flag was introduced in Windows 10, version 1607.
        /// </summary>
        DaxVolume = 0x20000000,

        /// <summary>
        /// The file system supports file-based compression.
        /// </summary>
        FileCompression = 0x10,

        /// <summary>
        /// The file system supports named streams.
        /// </summary>
        NamedStreams = 0x40000,

        /// <summary>
        /// The file system preserves and enforces access control lists (ACL).
        /// </summary>
        PersistentACLS = 8,

        /// <summary>
        /// The specified volume is read-only.
        /// </summary>
        ReadOnlyVolume = 0x80000,

        /// <summary>
        /// The volume supports a single sequential write.
        /// </summary>
        SequentialWriteOnce = 0x100000,

        /// <summary>
        /// The file system supports the Encrypted File System (EFS).
        /// </summary>
        SupportsEncryption = 0x20000,

        /// <summary>
        /// The specified volume supports extended attributes. An extended attribute is a piece of
        /// application-specific metadata that an application can associate with a file and is not part
        /// of the file's data.
        /// </summary>
        SupportsExtendedAttributes = 0x00800000,

        /// <summary>
        /// The specified volume supports hard links. For more information, see Hard Links and Junctions.
        /// </summary>
        SupportsHardLinks = 0x00400000,

        /// <summary>
        /// The file system supports object identifiers.
        /// </summary>
        SupportsObjectIDs = 0x10000,

        /// <summary>
        /// The file system supports open by FileID. For more information, see FILE_ID_BOTH_DIR_INFO.
        /// </summary>
        SupportsOpenByFileId = 0x01000000,

        /// <summary>
        /// The file system supports re-parse points.
        /// </summary>
        SupportsReparsePoints = 0x80,

        /// <summary>
        /// The file system supports sparse files.
        /// </summary>
        SupportsSparseFiles = 0x40,

        /// <summary>
        /// The volume supports transactions.
        /// </summary>
        SupportsTransactions = 0x200000,

        /// <summary>
        /// The specified volume supports update sequence number (USN) journals. For more information,
        /// see Change Journal Records.
        /// </summary>
        SupportsUsnJournal = 0x02000000,

        /// <summary>
        /// The file system supports Unicode in file names as they appear on disk.
        /// </summary>
        UnicodeOnDisk = 4,

        /// <summary>
        /// The specified volume is a compressed volume, for example, a DoubleSpace volume.
        /// </summary>
        VolumeIsCompressed = 0x8000,

        /// <summary>
        /// The file system supports disk quotas.
        /// </summary>
        VolumeQuotas = 0x20
    }
    
    /// <summary>
    /// Output data layout for GetFileInformationByHandleExW(...) with 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    internal struct FILE_ID_128
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Identifier;
    }

    // ReSharper disable once InconsistentNaming
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal struct FILE_ID_INFO
    {
        // ReSharper disable once InconsistentNaming
        public UInt64 VolumeSerialNumber;

        public byte[] FileId;
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetFileInformation
    {
        /// <summary>
        /// Used by the main scan program to grab information directly. Should be fixed.
        /// </summary>
        /// <param name="rootPathName">root name</param>
        /// <param name="volumeNameBuffer">volume name</param>
        /// <param name="volumeNameSize">size of buffer</param>
        /// <param name="volumeSerialNumber">vsn</param>
        /// <param name="maximumComponentLength">component length</param>
        /// <param name="fileSystemFlags">flags</param>
        /// <param name="fileSystemNameBuffer">name here</param>
        /// <param name="nFileSystemNameSize">size of name</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", EntryPoint = "GetVolumeInformationW", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetVolumeInformationW(
                string rootPathName,
                StringBuilder volumeNameBuffer,
                int volumeNameSize,
                out uint volumeSerialNumber,
                out uint maximumComponentLength,
                out FileSystemFeature fileSystemFlags,
                StringBuilder fileSystemNameBuffer,
                int nFileSystemNameSize
            );


        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle", SetLastError = true)]
        static extern
     Boolean GetFileInformationByHandle(
                                              IntPtr hFile,
                                                out BY_HANDLE_FILE_INFORMATION lpFileInformation
                                                    );

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private readonly struct BY_HANDLE_FILE_INFORMATION
        {
            public readonly uint dwFileAttributes;

            public readonly System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public readonly System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public readonly System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

            public readonly uint dwVolumeSerialNumber;
            public readonly uint nFileSizeHigh;
            public readonly uint nFileSizeLow;
            public readonly uint nNumberOfLinks;
            public readonly uint nFileIndexHigh;
            public readonly uint nFileIndexLow;
        }
        
        /// <summary>
        /// Flags needed externally
        /// </summary>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        public enum EFileAccess : uint
        {
            //
            // Standard Section
            //

#pragma warning disable 1591
            AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
            MaximumAllowed = 0x2000000,     // MaximumAllowed access type

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,        // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,       // directory
            FILE_WRITE_DATA = 0x0002,       // file & pipe
            FILE_ADD_FILE = 0x0002,         // directory
            FILE_APPEND_DATA = 0x0004,      // file
            FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
            FILE_READ_EA = 0x0008,          // file & directory
            FILE_WRITE_EA = 0x0010,         // file & directory
            FILE_EXECUTE = 0x0020,          // file
            FILE_TRAVERSE = 0x0020,         // directory
            FILE_DELETE_CHILD = 0x0040,     // directory
#pragma warning disable 1591
            FILE_READ_ATTRIBUTES = 0x0080,      // all
#pragma warning restore 1591
#pragma warning disable 1591
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all
#pragma warning restore 1591

            //
            // Generic Section
            //

            /// <summary>
            /// 
            /// </summary>
            GenericRead = 0x80000000,
            /// <summary>
            /// 
            /// </summary>
            GenericWrite = 0x40000000,
            /// <summary>
            /// 
            /// </summary>
            GenericExecute = 0x20000000,
            /// <summary>
            /// 
            /// </summary>
            GenericAll = 0x10000000,

            /// <summary>
            /// 
            /// </summary>
            SPECIFIC_RIGHTS_ALL = 0x00FFFF,

            /// <summary>
            /// 
            /// </summary>
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            /// <summary>
            /// 
            /// </summary>
            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            /// <summary>
            /// 
            /// </summary>
            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            /// <summary>
            /// 
            /// </summary>
            FILE_GENERIC_EXECUTE =
            StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
#pragma warning restore 1591
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum EFileShare : uint
        {
            /// <summary>
            /// 
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access. 
            /// Otherwise, other processes cannot open the object if they request read access. 
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access. 
            /// Otherwise, other processes cannot open the object if they request write access. 
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access. 
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }

        /// <summary>
        /// 
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always. 
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist. 
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always. 
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right. 
            /// </summary>
            TruncateExisting = 5
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum EFileAttributes : uint
        {
#pragma warning disable 1591
            Readonly = 0x00000001,
            Hidden = 0x00000002,
           System = 0x00000004,
            Directory = 0x00000010,
            /// <summary>
            /// 
            /// </summary>
            Archive = 0x00000020,
            /// <summary>
            /// 
            /// </summary>
            Device = 0x00000040,
            /// <summary>
            /// 
            /// </summary>
            Normal = 0x00000080,
            /// <summary>
            /// 
            /// </summary>
            Temporary = 0x00000100,
            /// <summary>
            /// 
            /// </summary>
            SparseFile = 0x00000200,
            /// <summary>
            /// 
            /// </summary>
            ReparsePoint = 0x00000400,
            /// <summary>
            /// 
            /// </summary>
            Compressed = 0x00000800,
            /// <summary>
            /// 
            /// </summary>
            Offline = 0x00001000,
            /// <summary>
            /// 
            /// </summary>
            NotContentIndexed = 0x00002000,
            /// <summary>
            /// 
            /// </summary>
            Encrypted = 0x00004000,
            /// <summary>
            /// 
            /// </summary>
            Write_Through = 0x80000000,
            /// <summary>
            /// 
            /// </summary>
            Overlapped = 0x40000000,
            /// <summary>
            /// 
            /// </summary>
            NoBuffering = 0x20000000,
            /// <summary>
            /// 
            /// </summary>
            RandomAccess = 0x10000000,
            /// <summary>
            /// 
            /// </summary>
            SequentialScan = 0x08000000,
            /// <summary>
            /// 
            /// </summary>
            DeleteOnClose = 0x04000000,
            /// <summary>
            /// 
            /// </summary>
            BackupSemantics = 0x02000000,
            /// <summary>
            /// 
            /// </summary>
            PosixSemantics = 0x01000000,
            /// <summary>
            /// 
            /// </summary>
            OpenReparsePoint = 0x00200000,
            /// <summary>
            /// 
            /// </summary>
            OpenNoRecall = 0x00100000,
            /// <summary>
            /// 
            /// </summary>
            FirstPipeInstance = 0x00080000
#pragma warning restore 1591
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <param name="securityAttributes"></param>
        /// <param name="creationDisposition"></param>
        /// <param name="flagsAndAttributes"></param>
        /// <param name="templateFile"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
                            [MarshalAs(UnmanagedType.LPWStr)] string filename,
                            [MarshalAs(UnmanagedType.U4)] EFileAccess access,
                            [MarshalAs(UnmanagedType.U4)] EFileShare share,
                            IntPtr securityAttributes,
                            [MarshalAs(UnmanagedType.U4)] ECreationDisposition creationDisposition,
                            [MarshalAs(UnmanagedType.U4)] EFileAttributes flagsAndAttributes,
                            IntPtr templateFile
            );

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        // ReSharper disable once UnusedMember.Local
        private static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        // ReSharper disable once UnusedMember.Local
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel23.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
        // ReSharper disable once UnusedMember.Local
        private static extern bool FreeLibrary(IntPtr hLibModule);

        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once InconsistentNaming
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileIdentity(string path)
        {
            string result = null;

            var handle = INVALID_HANDLE_VALUE;
            try
            {
                handle = CreateFileW(path,
                   EFileAccess.FILE_READ_ATTRIBUTES,
                   EFileShare.Read,
                   (IntPtr)null,
                   ECreationDisposition.OpenExisting,
                   EFileAttributes.Normal,
                   (IntPtr)null);

                // We obtained a handle, now time to use it.
                if (handle != INVALID_HANDLE_VALUE)
                {
                    GetFileInformationByHandle(handle, out var information);

                    result =
                        $"{information.dwVolumeSerialNumber >> 16:X4}-{information.dwVolumeSerialNumber & 0xffff:x4}:{information.nFileIndexHigh:X8}.{information.nFileIndexLow:X8}";
                }
            }
            finally
            {
                if (handle != INVALID_HANDLE_VALUE)
                {
                    CloseHandle(handle);
                }
            }

            return result;
        }
    }
}
