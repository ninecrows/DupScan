using System;

namespace C9Native
{
    /// <summary>
    /// Wrapper with RAII constructor for opening a read only native file handle.
    /// </summary>
    public class ReadFileHandle : BaseFileHandle
    {
        /// <summary>
        /// RAII CTOR create and load this object with a native file handle that points to the specified path.
        /// </summary>
        /// <param name="path">Path to the file we want a native handle for.</param>
        public ReadFileHandle(string path) : 
            base(path,
                EFileAccess.FILE_GENERIC_READ,
                EFileShare.Read,
                IntPtr.Zero,
                ECreationDisposition.OpenExisting,
                EFileAttributes.Normal,
                IntPtr.Zero)
        {
        }
    }
}
