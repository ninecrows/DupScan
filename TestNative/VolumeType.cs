using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace C9Native
{
    /// <summary>
    /// RAII method to access the volume type for a provided volume.
    /// </summary>
    public class VolumeType
    {
        /// <summary>
        /// Human readable name for the type of this volume.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Retrieve the volume path this was created for.
        /// </summary>
        public string VolumePath { get; }

        /// <summary>
        /// Retrieve the type code for this volume. Numeric volume type.
        /// </summary>
        public DriveType Type { get; }

        /// <summary>
        /// RAII CTOR given a volume path this object provides the type of volume (if available)
        /// </summary>
        /// <param name="path">Volume path we're interested in</param>
        public VolumeType(string path)
        {
            VolumePath = path;

            Type = GetDriveTypeW(path);
            TypeName = Type.ToString();
        }

       [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
       static extern DriveType GetDriveTypeW([MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName);

       /// <summary>
       /// Drive type enumeration.
       /// </summary>
       [SuppressMessage("ReSharper", "UnusedMember.Global")]
       public enum DriveType : uint
       {
           /// <summary>The drive type cannot be determined.</summary>
           Unknown = 0,    //DRIVE_UNKNOWN
           /// <summary>The root path is invalid, for example, no volume is mounted at the path.</summary>
           Error = 1,        //DRIVE_NO_ROOT_DIR
           /// <summary>The drive is a type that has removable media, for example, a floppy drive or removable hard disk.</summary>
           Removable = 2,    //DRIVE_REMOVABLE
           /// <summary>The drive is a type that cannot be removed, for example, a fixed hard drive.</summary>
           Fixed = 3,        //DRIVE_FIXED
           /// <summary>The drive is a remote (network) drive.</summary>
           Remote = 4,        //DRIVE_REMOTE
           /// <summary>The drive is a CD-ROM drive.</summary>
           // ReSharper disable once InconsistentNaming
           // ReSharper disable once IdentifierTypo
           // ReSharper disable once CommentTypo
           CDROM = 5,        //DRIVE_CDROM
           /// <summary>The drive is a RAM disk.</summary>
           // ReSharper disable once InconsistentNaming
           // ReSharper disable once CommentTypo
           RAMDisk = 6        //DRIVE_RAMDISK
       }

    }
}
