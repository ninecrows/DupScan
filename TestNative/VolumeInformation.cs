using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace C9Native
{
    /// <summary>
    /// RAII Retrieve Volume information for the specified volume.
    /// </summary>
    public class VolumeInformation
    {
        /// <summary>
        /// Retrieve the path that was used to retrieve this object's contents.
        /// </summary>
        public string VolumePath { get; }

        /// <summary>
        /// Retrieve any failure status from our construction or null if construction succeeded.
        /// </summary>
        public Win32ErrorCode Status { get; }

        /// <summary>
        /// Return true if this object was created without an error.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsOk => Status == null;

        /// <summary>
        /// Volume label as a string.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Retrieve the volume serial number as a 32 bit unsigned integer.
        /// </summary>
        public uint VolumeSerialNumber { get; }

        /// <summary>
        /// Retrieve the volume serial number as a string.
        /// </summary>
        public string VolumeSerial { get; }

        /// <summary>
        /// Maximum path component length for this file system.
        /// </summary>
        public uint ComponentLength { get; } 

        /// <summary>
        /// Feature flags relevant to this volume's file system.
        /// </summary>
        public FileSystemFeature Features { get; }

        /// <summary>
        /// Name of the file system that this volume is formatted for.
        /// </summary>
        public string FileSystem { get; }

        /// <summary>
        /// RAII CTOR retrieves volume information for the volume that path points to.
        /// </summary>
        /// <param name="path">path to the volume we're interested in</param>
        public VolumeInformation(string path)
        {
            VolumePath = path;

            StringBuilder labelBuffer = new StringBuilder(1024, 1024);
            StringBuilder fileSystemNameBuffer = new StringBuilder(1024, 1024);

            bool ok = GetVolumeInformation(path,
                labelBuffer,
                labelBuffer.Capacity,
                out var volumeSerialNumber,
                out var componentLength,
                out var fileSystemFeatures,
                fileSystemNameBuffer,
                fileSystemNameBuffer.Capacity);

            // Transfer the retrieved data to our externally visible properties.
            if (ok)
            {
                Label = labelBuffer.ToString();
                VolumeSerialNumber = volumeSerialNumber;
                VolumeSerial = $"{volumeSerialNumber:X8}"; 
                ComponentLength = componentLength;
                Features = fileSystemFeatures;
                FileSystem = fileSystemNameBuffer.ToString();
            }
            else
            {
                Status = new Win32ErrorCode();
            }
        }

        /// <summary>
        /// File system feature flags returned as part of information.
        /// </summary>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            // ReSharper disable once IdentifierTypo
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

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumeInformation(
                string rootPathName,
                StringBuilder volumeNameBuffer,
                int volumeNameSize,
                out uint volumeSerialNumber,
                out uint maximumComponentLength,
                out FileSystemFeature fileSystemFlags,
                StringBuilder fileSystemNameBuffer,
                int nFileSystemNameSize);
    }
}
