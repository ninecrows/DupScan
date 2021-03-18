using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace C9Native
{
    /// <summary>
    /// RAII Get a list of _volumes and the available detailed volume information.
    /// </summary>
    public class Volumes : IEnumerable<Volume>
    {
        // Store list of volume names we found when we scanned.
        private readonly List<string> _volumeNames = new();

        // Primary internal list of volumes we know about.
        private readonly List<Volume> _volumeList = new();

        /// <summary>
        /// Retrieve one of the volume names stored in this object.
        /// </summary>
        /// <param name="index">Index of the item being retrieved.</param>
        /// <returns>Volume path name</returns>
        public Volume this[int index] => _volumeList[index];

        /// <summary>
        /// Number of items in the base volume list.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public int Count => _volumeList.Count;

        // Index of volumes by volume path.
        private readonly Dictionary<string, Volume> _volumes = new();

        /// <summary>
        /// Retrieve the volume information for one volume we know about.
        /// </summary>
        /// <param name="index">Volume path for the volume we're interested in</param>
        /// <returns>Volume object containing the details.</returns>
        public Volume this[string index] => _volumes[index];
        
        /// <summary>
        /// Get a list of all volumes with this serial number.
        /// </summary>
        /// <param name="aSerialNumber">String form of the volume serial number we're looking for.</param>
        /// <returns>List of matching volumes.</returns>
        // ReSharper disable once UnusedMember.Global
        public List<Volume> BySerial(string aSerialNumber)
        {
            List<Volume> hits = new();

            // Walk the list and accumulate all items that have this serial number, should only be one.
            foreach (Volume volume in this)
            {
                if (volume.Information != null && volume.Information.VolumeSerial == aSerialNumber)
                {
                    hits.Add(volume);
                }
            }

            return hits;
        }

        /// <summary>
        /// Get the one volume that should have this serial number. Throws if there are duplicates.
        /// </summary>
        /// <param name="aSerialNumber">String form of the volume serial number we're looking for.</param>
        /// <returns>Matching volume or null if no match.</returns>
        // ReSharper disable once UnusedMember.Global
        public Volume BySerialOne(string aSerialNumber)
        {
            Volume hit = null;

            // Walk the list and accumulate all items that have this serial number, should only be one.
            foreach (Volume volume in this)
            {
                if (volume.Information != null && volume.Information.VolumeSerial == aSerialNumber)
                {
                    if (hit != null)
                    {
                        throw new Exception("Duplicate volume serial numbers found");
                    }
                    hit = volume;
                }
            }

            return hit;
        }

        /// <summary>
        /// Retrieve a list of the volume serial numbers in the volumes we know of.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public List<string> SerialNumbers
        {
            get
            {
                List<string> serials = new();

                foreach (var item in this)
                {
                    if (item.Information?.VolumeSerial != null)
                    {
                        serials.Add(item.Information.VolumeSerial);
                    }
                }

                return serials;
            }
        }

        /// <summary>
        /// Get a list of zero of more volumes that have the specified volume label.
        /// </summary>
        /// <param name="aLabel">Label we're looking for.</param>
        /// <returns>List of volumes that have that label.</returns>
        // ReSharper disable once UnusedMember.Global
        public List<Volume> ByLabel(string aLabel)
        {
            return this.Where(volume => volume.Information?.Label != null && volume.Information.Label == aLabel).ToList();
        }

        /// <summary>
        /// Retrieve a list of label strings for the volumes we know about.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public List<string> Labels => (from volume in this where volume.Information?.Label != null select volume.Information.Label).ToList();

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

            // Scan through the volume names we got and get detailed information for each.
            foreach (var name in _volumeNames)
            {
                var thisVolume = new Volume(name);
                
                _volumeList.Add(thisVolume);
                _volumes[name] = thisVolume;
            }
        }
        
        /// <summary>
        /// Make this class enumerable
        /// </summary>
        /// <returns>an enumerator for the contents of this object.</returns>
        public IEnumerator<Volume> GetEnumerator()
        {
            var enumerator = _volumeList.GetEnumerator();
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
