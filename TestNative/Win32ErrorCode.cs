using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace C9Native
{
    /// <summary>
    /// RAII win32 status code storage and translation object.
    /// </summary>
    public class Win32ErrorCode
    {
        /// <summary>
        /// Retrieve the stored win32 status code.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Retrieve the human readable error message associated with this code.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Store a win32 status code and provide a human readable string if possible.
        /// </summary>
        /// <param name="code">Status code to be stored and translated</param>
        // ReSharper disable once UnusedMember.Global
        public Win32ErrorCode(int code)
        {
            this.Code = code;
            IntPtr buffer = IntPtr.Zero;
            int result = FormatMessageW(FORMAT_MESSAGE.ALLOCATE_BUFFER |
                FORMAT_MESSAGE.FROM_SYSTEM | 
                FORMAT_MESSAGE.IGNORE_INSERTS,
                IntPtr.Zero, 
                code, 
                0, 
                ref buffer, 0,
                IntPtr.Zero);

            if (result == 0)
            {
                Message = $"Win32Error({code})";
            }
            else
            {
                string thing = Marshal.PtrToStringUni(buffer);
                LocalFree(buffer);
                Message = thing; 
            }
        }

        /// <summary>
        /// Pull the marshaller stored win32 status code and provide translation
        /// </summary>
        public Win32ErrorCode()
        {
            Code = Marshal.GetLastWin32Error(); 
            IntPtr buffer = IntPtr.Zero;
            int result = FormatMessageW(FORMAT_MESSAGE.ALLOCATE_BUFFER |
                                        FORMAT_MESSAGE.FROM_SYSTEM |
                                        FORMAT_MESSAGE.IGNORE_INSERTS,
                IntPtr.Zero,
                Code,
                0,
                ref buffer, 0,
                IntPtr.Zero);

            if (result == 0)
            {
                Message = "Win32Error(" + Code.ToString() + ")";
            }
            else
            {
                string thing = Marshal.PtrToStringUni(buffer);
                LocalFree(buffer);
                Message = thing;
            }
        }
        
        [Flags]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum FORMAT_MESSAGE : uint
        {
            ALLOCATE_BUFFER = 0x00000100,
            IGNORE_INSERTS = 0x00000200,
            FROM_SYSTEM = 0x00001000,
            ARGUMENT_ARRAY = 0x00002000,
            FROM_HMODULE = 0x00000800,
            FROM_STRING = 0x00000400
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern int FormatMessageW(FORMAT_MESSAGE dwFlags, 
            IntPtr lpSource, 
            int dwMessageId, 
            uint dwLanguageId, 
            ref IntPtr aBuffer,
            int nSize, 
            IntPtr arguments);
    }
}
