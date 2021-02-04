using System;
using System.Runtime.InteropServices;
using System.Text;

namespace C9Native
{
    // Handle PInvoke for this function
    /*
     BOOL 
     GetFileInformationByHandleExW(
        HANDLE hFile,
        FILE_INFO_BY_HANDLE_CLASS FileInformationClass,
        LPVOID lpFileInformation,
        DWORD dwBufferSize
        );
     */

    /// <summary>
    /// Mode select enumeration for GetFileInformationByHandleExW(...)
    /// </summary>
    enum FILE_INFO_BY_HANDLE_CLASS
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

    [Flags]
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
    struct FILE_ID_128
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Identifier;
    }

    struct FILE_ID_INFO
    {
        public UInt64 VolumeSerialNumber;

        public byte[] FileId;
        //        public FILE_ID_128 FileId;
    }

    [StructLayout(LayoutKind.Sequential)]

    struct FileIdInfo
    {
        public UInt64 VolumeSerialNumber;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileId;
    }

    //private struct 

    public class GetFileInformation
    {
        [DllImport("Kernel32.dll", EntryPoint = "GetVolumeInformationW", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetVolumeInformationW(
                string rootPathName,
                StringBuilder volumeNameBuffer,
                int volumeNameSize,
                out uint volumeSerialNumber,
                out uint maximumComponentLength,
                out FileSystemFeature fileSystemFlags,
                StringBuilder fileSystemNameBuffer,
                int nFileSystemNameSize
            );

        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
        static extern 
            /*BOOL*/ bool GetFileInformationByHandleExW(
            /*HANDLE*/ IntPtr hFile,
            FILE_INFO_BY_HANDLE_CLASS FileInformationClass,

            ///*LPVOID*/ IntPtr lpFileInformation,
            ref FILE_ID_INFO result,

            /*DWORD*/ UInt32 dwBufferSize
            );

        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleExW", SetLastError = true)]
        static extern /*BOOL*/ bool GetRawFileInformationByHandleExW(
                                                /*HANDLE*/ IntPtr hFile,
                                                FILE_INFO_BY_HANDLE_CLASS FileInformationClass,
                                                ///*LPVOID*/ IntPtr lpFileInformation,
                                                IntPtr result,
                                                /*DWORD*/ UInt32 dwBufferSize
            );

        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool GetFileInformationByHandleEx(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, out FILE_ID_BOTH_DIR_INFO dirInfo, uint dwBufferSize);

        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle", SetLastError = true)]
        static extern
        /*BOOL*/ Boolean GetFileInformationByHandle(
                                                /*HANDLE*/ IntPtr hFile,
                                                out BY_HANDLE_FILE_INFORMATION lpFileInformation
                                                    );

        [StructLayout(LayoutKind.Sequential)]
        struct MyData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] FileId;
            public UInt64 VolumeId;
        };

        [DllImport("TestTarget.dll", EntryPoint = "RunThis")]
        static extern int RunThis(string path, MyData data);

        [DllImport("TestTarget.dll", EntryPoint = "FakeFileId")]
        static extern int FakeFileId(string path, ref MyData data);

        [StructLayout(LayoutKind.Sequential)]
        struct BY_HANDLE_FILE_INFORMATION
        {
            /*DWORD*/
            public UInt32 dwFileAttributes;
            //System.DateTime /*FILETIME*/ ftCreationTime;
            //System.DateTime /*FILETIME*/  ftLastAccessTime;
            //System.DateTime /*FILETIME*/  ftLastWriteTime;

            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

            /*DWORD*/
            public UInt32 dwVolumeSerialNumber;
            /*DWORD*/
            public UInt32 nFileSizeHigh;
            /*DWORD*/
            public UInt32 nFileSizeLow;
            /*DWORD*/
            public UInt32 nNumberOfLinks;
            /*DWORD*/
            public UInt32 nFileIndexHigh;
            /*DWORD*/
            public UInt32 nFileIndexLow;
        }

        //[DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle")]
        //static extern bool GetFileInformationByHandle(IntPtr /*HANDLE*/ handle,
        //   ref BY_HANDLE_FILE_INFORMATION information);

        [Flags]
        public enum EFileAccess : uint
        {
            //
            // Standart Section
            //

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
            FILE_READ_ATTRIBUTES = 0x0080,      // all
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all

            //
            // Generic Section
            //

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            FILE_GENERIC_EXECUTE =
            StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
        }

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

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

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

        //   [DllImport("kernel32.dll", EntryPoint = "CreateFileW")]
        //   static extern IntPtr /*HANDLE*/ CreateFileW(
        ///*LPCWSTR*/ string lpFileName,
        // /*DWORD*/ UInt32 dwDesiredAccess,
        // /*DWORD*/ UInt32 dwShareMode,
        // /*LPSECURITY_ATTRIBUTES*/ IntPtr lpSecurityAttributes,
        ///*DWORD*/ UInt32 dwCreationDisposition,
        // /*DWORD*/ UInt32 dwFlagsAndAttributes,
        // /*HANDLE*/ IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        //[DllImport("kernel32.dll", EntryPoint="LoadLibraryW")]
        //extern static /*HMODULE*/ IntPtr LoadLibraryW(
        ///*LPCSTR*/ string lpLibFileName
        //);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        //  [DllImport("kernel32.dll", EntryPoint ="GetProcAddress")]
        //  extern static /*FARPROC*/ IntPtr GetProcAddress(
        // /*HMODULE*/ IntPtr hModule,
        // /*LPCSTR*/ string lpProcName
        //);

        [DllImport("kernel23.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
        extern static /*BOOL*/ bool FreeLibrary(
        /*HMODULE*/ IntPtr hLibModule
       );

        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public static String GetFileIdentity(String path)
        {
            string result = null;

            IntPtr handle = INVALID_HANDLE_VALUE;
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
                    BY_HANDLE_FILE_INFORMATION information = new BY_HANDLE_FILE_INFORMATION();

                    bool ok = GetFileInformationByHandle(handle, out information);

                    result = string.Format("{0:X4}-{1:x4}:{2:X8}.{3:X8}",
                        information.dwVolumeSerialNumber >> 16, information.dwVolumeSerialNumber & 0xffff,
                        information.nFileIndexHigh, information.nFileIndexLow);
                    //result = information.dwVolumeSerialNumber + ":" + information.nFileIndexHigh + "." + information.nFileIndexLow;
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

        public static void CallMe(string path)
        {
            IntPtr library = LoadLibraryW("TestTarget.dll");
            if (library != null)
            {
                bool ok = FreeLibrary(library);
                if (!ok)
                {
                    int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                }
            }

            MyData data = new MyData();

            int status = FakeFileId("AbcDef", ref data);

            int value = RunThis("c:\\Temp\\Foo.json", data);

            System.IO.FileStream stream = System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            var handle = stream.SafeFileHandle;

            // Make an object to receive this data.
            FILE_ID_INFO myInfo = new FILE_ID_INFO();

            bool result = GetFileInformationByHandleExW(handle.DangerousGetHandle(),
              FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
              ref myInfo,
              24);
        }
    }
}
