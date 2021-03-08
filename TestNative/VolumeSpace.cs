using System.Runtime.InteropServices;

namespace C9Native
{
    /// <summary>
    /// Given a volume path, RAII provide the capacity information for the volume.
    /// </summary>
    public class VolumeSpace
    {
        /// <summary>
        /// Retrieve the volume path that this object was created for.
        /// </summary>
        public string VolumePath { get; }
        
        /// <summary>
        /// Return the number of bytes available to the current user.
        /// </summary>
        public ulong AvailableBytes { get; }

        /// <summary>
        /// Human readable version of available space.
        /// </summary>
        public string AvailableText { get; }

        /// <summary>
        /// Return the total number of bytes that the volume can store.
        /// </summary>
        public ulong TotalBytes { get; }
        
        /// <summary>
        /// Text representation of the total space on the volume.
        /// </summary>
        public string TotalText { get; }

        /// <summary>
        /// Return the number of bytes of free space on the volume.
        /// </summary>
        public ulong FreeBytes { get; }

        /// <summary>
        /// Text representation of the free space on the volume.
        /// </summary>
        public string FreeText { get; }

        /// <summary>
        /// Retrieve any failure status code from the setup of this object.
        /// </summary>
        public Win32ErrorCode Status { get; }

        /// <summary>
        /// Return true if the object was loaded successfully. A false result indicates error information is stored.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsOk => Status == null;

        /// <summary>
        /// RAII Style create an object holding the free space information for this volume.
        /// </summary>
        /// <param name="path">Volume path for which the information is to be retrieved.</param>
        public VolumeSpace(string path)
        {
            VolumePath = path;

            bool ok = GetDiskFreeSpaceExW(path,
                out var availableBytes,
                out var totalBytes,
                out var freeBytes);

            if (ok)
            {
                AvailableBytes = availableBytes;
                TotalBytes = totalBytes;
                FreeBytes = freeBytes;

                TotalText = DescribeCapacity(TotalBytes);
                AvailableText = DescribeCapacity(AvailableBytes);
                FreeText = DescribeCapacity(FreeBytes);
            }
            else
            {
                Status = new Win32ErrorCode();
            }
        }

        // Provide a concise description of the capacity returned. Reading long strings of digits can be challenging.
        private static string DescribeCapacity(ulong value)
        {
            const ulong kb = 1024;
            const ulong mb = 1024 * 1024;
            const ulong gb = 1024 * 1024 * 1024;
            const ulong tb = 1024L * 1024L * 1024L * 1024L;

            float fraction = value;
            string suffix;
            if (value < kb)
            {
                suffix = " Bytes";
            }
            else if (value < mb)
            {
                fraction = (float) value / kb;
                suffix = "KB";
            }
            else if (value < gb)
            {
                fraction = (float) value / mb;
                suffix = "MB";
            }
            else if (value < tb)
            {
                fraction = (float) value / gb;
                suffix = "GB";
            }
            else
            {
                fraction = (float) value / tb;
                suffix = "TB";
            }

            return $"{fraction:F2}{suffix}";
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceExW(string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);
    }
}
