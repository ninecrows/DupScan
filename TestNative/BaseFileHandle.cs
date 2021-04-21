using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace C9Native
{
    /// <summary>
    /// Wrapper with RAII constructor for opening a read only native file handle.
    /// </summary>
    public class BaseFileHandle : IDisposable
    {
        /// <summary>
        /// Handle that will be filled in by the CTOR and used subsequently.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Retrieve completion status information if there is a failure.
        /// </summary>
        public Win32ErrorCode Status { get; }

        /// <summary>
        /// Return true if this object was loaded successfully.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsOk => Status == null;

        /// <summary>
        /// Invalid handle value for comparison.
        /// </summary>
        protected static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        
        /// <summary>
        /// Null handle value for comparison.
        /// </summary>
        protected static readonly IntPtr NullHandleValue = new IntPtr(0);

        /// <summary>
        /// Return the path that was used to open this file handle.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Factory for read only file handles.
        /// </summary>
        /// <param name="path">Path to the file that we want a native handle for.</param>
        /// <returns>File handle container</returns>
        // ReSharper disable once UnusedMember.Global
        public static BaseFileHandle ReadOnlyHandleFactory(string path)
        {
            return new BaseFileHandle(path,
                EFileAccess.FILE_GENERIC_READ,
                EFileShare.Read,
                IntPtr.Zero,
                ECreationDisposition.OpenExisting,
                EFileAttributes.Normal,
                IntPtr.Zero);
        }

        /// <summary>
        /// Base native handle constructor. Call up from 'live' class.
        /// </summary>
        /// <param name="path">Path to file that we want a handle to</param>
        /// <param name="inAccess">Access parameters</param>
        /// <param name="inShare">Sharing access</param>
        /// <param name="inSecurity">Security descriptor</param>
        /// <param name="inCreation">Creation parameters</param>
        /// <param name="inAttributes">File attributes if we create a new file</param>
        /// <param name="inTemplate">Template file</param>
        protected BaseFileHandle(string path,
            EFileAccess inAccess,
            EFileShare inShare,
            IntPtr inSecurity,
            ECreationDisposition inCreation,
            EFileAttributes inAttributes,
            IntPtr inTemplate)
        {
            // Remember the file path that we started with.
            Path = path;

            string extpath = "\\\\?\\" + path;

            // Open the handle with all of the requested flags.
            Handle = CreateFileW(extpath, inAccess, inShare, inSecurity, inCreation, inAttributes, inTemplate);

            // Grab and store the failure status if we didn't get a handle successfully.
            if (!IsHandleValid(Handle))
            {
                Status = new Win32ErrorCode();
            }
        }

        /// <summary>
        /// Release native handle if we hold one.
        /// Atomic exchange so this should be thread safe.
        /// </summary>
        public void Dispose()
        {
            // Atomic exchange to ensure we don't double close this thing.
            IntPtr handle = IntPtr.Zero;
            Interlocked.Exchange(ref handle, Handle);

            // If we got a valid handle we need to close it.
            if (IsHandleValid(handle))
            {
                var ok = CloseHandle(handle);
                if (!ok)
                {
                    // ReSharper disable once UnusedVariable
                    var status = new Win32ErrorCode();
                }
            }
        }

        /// <summary>
        /// Return true if this object contains a valid handle.
        /// </summary>
        /// <param name="handle">handle to be checked</param>
        /// <returns>true if the provided handle is valid</returns>
        protected static bool IsHandleValid(IntPtr handle)
        {
            return (handle != InvalidHandleValue && handle != NullHandleValue);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] EFileAccess access,
            [MarshalAs(UnmanagedType.U4)] EFileShare share,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] ECreationDisposition creationDisposition,
            [MarshalAs(UnmanagedType.U4)] EFileAttributes flagsAndAttributes,
            IntPtr templateFile);

        /// <summary>
        /// Control of file creation.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            New = 1,
            /// <summary>
            /// Creates a new file, always.
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist.
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always.
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            TruncateExisting = 5
        }

        /// <summary>
        /// Flags for control of attributes.
        /// </summary>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected enum EFileAttributes : uint
        {
#pragma warning disable 1591
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
#pragma warning restore 1591
        }

        /// <summary>
        /// Flags that control sharing.
        /// </summary>
        [Flags]
        protected enum EFileShare : uint
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
        /// Flags controlling access
        /// </summary>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected enum EFileAccess : uint
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
            // ReSharper disable once IdentifierTypo
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
#pragma warning restore 1591
        }
    }
}
