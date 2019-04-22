using System;
using System.Runtime.InteropServices;

namespace TestNative
{
    // Handle PInvoke for this function
    /*
     * BOOL GetFileInformationByHandleExW(
  HANDLE                    hFile,
  FILE_INFO_BY_HANDLE_CLASS FileInformationClass,
  LPVOID                    lpFileInformation,
  DWORD                     dwBufferSize
);
     */
    // Returning at least the 

    enum FILE_INFO_BY_HANDLE_CLASS
    {
        FileBasicInfo,
        FileStandardInfo,
        FileNameInfo,
        FileRenameInfo,
        FileDispositionInfo,
        FileAllocationInfo,
        FileEndOfFileInfo,
        FileStreamInfo,
        FileCompressionInfo,
        FileAttributeTagInfo,
        FileIdBothDirectoryInfo,
        FileIdBothDirectoryRestartInfo,
        FileIoPriorityHintInfo,
        FileRemoteProtocolInfo,
        FileFullDirectoryInfo,
        FileFullDirectoryRestartInfo,
        FileStorageInfo,
        FileAlignmentInfo,
        FileIdInfo,
        FileIdExtdDirectoryInfo,
        FileIdExtdDirectoryRestartInfo,
        FileDispositionInfoEx,
        FileRenameInfoEx,
        MaximumFileInfoByHandleClass
    };

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
        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError=true)]
        static extern /*BOOL*/ bool GetFileInformationByHandleExW(
            /*HANDLE*/ IntPtr hFile,
            FILE_INFO_BY_HANDLE_CLASS FileInformationClass,
            ///*LPVOID*/ IntPtr lpFileInformation,
            ref FILE_ID_INFO result,
            /*DWORD*/ UInt32 dwBufferSize);

        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleExW", SetLastError = true)]
        static extern /*BOOL*/ bool GetRawFileInformationByHandleExW(
    /*HANDLE*/ IntPtr hFile,
    FILE_INFO_BY_HANDLE_CLASS FileInformationClass,
            ///*LPVOID*/ IntPtr lpFileInformation,
            IntPtr result,
    /*DWORD*/ UInt32 dwBufferSize);

        [StructLayout(LayoutKind.Sequential)]
        struct MyData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] FileId;
            public UInt64 VolumeId;
        };

        [DllImport("TestTarget.dll", EntryPoint="RunThis")]
        static extern int RunThis(string path, MyData data);

        [DllImport("TestTarget.dll", EntryPoint = "FakeFileId")]
        static extern int FakeFileId(string path, ref MyData data);

        struct BY_HANDLE_FILE_INFORMATION
        {
            /*DWORD*/ UInt32 dwFileAttributes;
            System.DateTime /*FILETIME*/ ftCreationTime;
            System.DateTime /*FILETIME*/  ftLastAccessTime;
            System.DateTime /*FILETIME*/  ftLastWriteTime;
            /*DWORD*/ UInt32 dwVolumeSerialNumber;
            /*DWORD*/ UInt32 nFileSizeHigh;
            /*DWORD*/ UInt32 nFileSizeLow;
            /*DWORD*/ UInt32 nNumberOfLinks;
            /*DWORD*/ UInt32 nFileIndexHigh;
            /*DWORD*/ UInt32 nFileIndexLow;
        }

        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle")]
        static extern bool GetFileInformationByHandle(IntPtr /*HANDLE*/ handle,
            ref BY_HANDLE_FILE_INFORMATION information);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW")]
        static extern IntPtr /*HANDLE*/ CreateFileW(
  /*LPCWSTR*/ string lpFileName,
  /*DWORD*/ UInt32 dwDesiredAccess,
  /*DWORD*/ UInt32 dwShareMode,
  /*LPSECURITY_ATTRIBUTES*/ IntPtr lpSecurityAttributes,
  /*DWORD*/ UInt32 dwCreationDisposition,
  /*DWORD*/ UInt32 dwFlagsAndAttributes,
  /*HANDLE*/ IntPtr hTemplateFile);


        [DllImport("kernel32.dll", EntryPoint="LoadLibraryW")]
        extern static /*HMODULE*/ IntPtr LoadLibraryW(
  /*LPCSTR*/ string lpLibFileName
);

        [DllImport("kernel32.dll", EntryPoint ="GetProcAddress")]
        extern static /*FARPROC*/ IntPtr GetProcAddress(
  /*HMODULE*/ IntPtr hModule,
  /*LPCSTR*/ string lpProcName
);

        [DllImport("kernel23.dll", EntryPoint ="FreeLibrary", SetLastError =true)]
        extern static /*BOOL*/ bool FreeLibrary(
  /*HMODULE*/ IntPtr hLibModule
);
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
