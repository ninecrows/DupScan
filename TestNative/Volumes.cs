using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace C9Native
{
    /// <summary>
    /// RAII Get a list of _volumes and the available detailed volume information.
    /// </summary>
    public class Volumes
    {
        // Store list of volume names we found when we scanned.
        private readonly List<string> _volumeNames = new List<string>();

        /// <summary>
        /// Retrieve one of the volume names stored in this object.
        /// </summary>
        /// <param name="index">Index of the item being retrieved.</param>
        /// <returns>Volume path name</returns>
        public string this[int index] => _volumeNames[index];

        private readonly Dictionary<string, Volume> _volumes = new Dictionary<string, Volume>();

        /// <summary>
        /// Retrieve the volume information for one volume we know about.
        /// </summary>
        /// <param name="index">Volume path for the volume we're interested in</param>
        /// <returns>Volume object containing the details.</returns>
        public Volume this[string index] => _volumes[index];
        
        /// <summary>
        /// Load this object up with the current volume list information.
        /// </summary>
        public Volumes()
        {
            var thisItem = new StringBuilder(1024);
           
            using (var findHandle = FindFirstVolume(thisItem, (uint)thisItem.Capacity))
            {
                // If we have usable data...
                if (!findHandle.IsInvalid)
                {
                    // Remember this as a volume we've seen.
                    _volumeNames.Add(thisItem.ToString());

                    bool doWeHaveAnother;
                    do
                    {
                        thisItem = new StringBuilder(1024);
                        doWeHaveAnother = FindNextVolume(findHandle, thisItem, (uint) thisItem.Capacity);
                        if (doWeHaveAnother)
                        {
                            _volumeNames.Add(thisItem.ToString());
                        }
                    } while (doWeHaveAnother) ;
                }
            }

            foreach (string name in _volumeNames)
            {
                _volumes[name] = new Volume(name);
            }
        }

        // Internal use safe handle wrapper.
        // ReSharper disable once ClassNeverInstantiated.Local
        private class FindVolumeSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            // ReSharper disable once UnusedMember.Local
            private FindVolumeSafeHandle()
                : base(true)
            {
            }

            // ReSharper disable once UnusedMember.Global
            // ReSharper disable once UnusedMember.Local
            public FindVolumeSafeHandle(IntPtr preexistingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                SetHandle(preexistingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return FindVolumeClose(handle);
            }
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern FindVolumeSafeHandle FindFirstVolume([Out] StringBuilder lpszVolumeName, uint cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindNextVolume(FindVolumeSafeHandle hFindVolume, [Out] StringBuilder lpszVolumeName, uint cchBufferLength);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindVolumeClose(IntPtr hFindVolume);
    }
}
