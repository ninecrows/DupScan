using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace C9Native
{
    /// <summary>
    /// RAII Retrieve mounted paths for the provided volume.
    /// </summary>
    public class VolumePathNames
    {
        /// <summary>
        /// The name of the volume we were asked to get mount paths for.
        /// </summary>
        public string VolumeName { get; }

        // List of mount paths that this volume is at.
        private readonly List<string> _pathNames = new List<string>();

        // Completion status code.

        /// <summary>
        /// Retrieve one of the paths that this volume is mounted at.
        /// </summary>
        /// <param name="index">Index of the path to return</param>
        /// <returns>the string that represents one of the mount paths for this volume</returns>
        public string this[int index] => _pathNames[index];

        /// <summary>
        /// Property that provides the number of paths stored here.
        /// </summary>
        public int Count => _pathNames.Count;

        /// <summary>
        /// Provide read only access to the name of the volume we grabbed the mount paths for.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string Volume => VolumeName;

        /// <summary>
        /// Return true if we got the information we were looking for.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsOk => Status == null;

        /// <summary>
        /// Return the completion status code for this retrieval.
        /// </summary>
        public Win32ErrorCode Status { get; }

        /// <summary>
        /// Construct object loaded with the paths (zero or more) associated with this volume.
        /// </summary>
        /// <param name="volume">Volume path that we want mount paths for.</param>
        public VolumePathNames(string volume)
        {
            // Remember the volume path we're looking at.
            VolumeName = volume;

            // Buffer where we'll get the information. This needs to be a char array and not a StringBuilder because we get 
            // multiple '\0' separated path strings with '\0\0' to terminate.
            char[] result = new char[1024];

            UInt32 resultLength = 0;
            bool isOk = GetVolumePathNamesForVolumeNameW(volume,
                result, (uint) result.Length,
                ref resultLength);

            // If we got what we were looking for...
            if (isOk)
            {
                // Walk through the returned buffer and pull each '\0' separated string and store it in our array.
                int begins = 0;
                for (int offset = 0; offset < resultLength; offset++)
                {
                    // If we're looking at a separator then we need to pull the string and update the new begin index.
                    if (result[offset] == '\0')
                    {
                        // Ignore any strings that are empty...that means we're done with this data set.
                        if (offset - begins > 0)
                        {
                            string thisOne = new string(result, begins, offset - begins);
                            _pathNames.Add(thisOne);
                            begins = offset + 1;
                        }
                    }
                }
            }

            // If we failed to get what we were looking for then the payload will be empty and we'll save the error code here.
            else
            {
                Status = new Win32ErrorCode();
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumePathNamesForVolumeNameW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszVolumeName,
            [Out] char[] lpszVolumePathNames, uint aBufferLength,
            ref uint aReturnLength);
    }
}
