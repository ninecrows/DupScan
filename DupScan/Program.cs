using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.InteropServices;

namespace DupScan
{
    [Serializable]
    class FileInformation
    {
        public FileInformation(string fileName)
        {
            path = fileName;

            FileInfo information = new FileInfo(fileName);

            created = information.CreationTime; //File.GetCreationTime(path);
            created8601 = created.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            modified = information.LastWriteTime; //File.GetLastWriteTime(path);
            modified8601 = modified.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            size = information.Length;

            directory = information.DirectoryName;
        }

        public string path;
        public string directory;
        public long size;

        public DateTime created;
        public string created8601;
        public DateTime modified;
        public string modified8601;
    }

    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern /*BOOL*/ bool GetVolumeInformationW(
/*LPCWSTR*/ String lpRootPathName,
/*LPWSTR*/ StringBuilder lpVolumeNameBuffer,
/*DWORD*/ int nVolumeNameSize,
/*LPDWORD*/ out UInt32 lpVolumeSerialNumber,
/*LPDWORD*/ out UInt32 lpMaximumComponentLength,
/*LPDWORD*/ out FileSystemFeature lpFileSystemFlags,
/*LPWSTR*/ StringBuilder lpFileSystemNameBuffer,
/*DWORD*/ int nFileSystemNameSize
);

    [Flags]
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

    delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
/*EnumMonitorsDelegate*/ MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    /// <summary>
    /// The struct that contains the display information
    /// </summary>
    public class DisplayInfo
    {
        public string Availability { get; set; }
        public string ScreenHeight { get; set; }
        public string ScreenWidth { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
    }
    /// <summary>
    /// Collection of display information
    /// </summary>
    public class DisplayInfoCollection : List<DisplayInfo>
    {
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    /// <summary>
    /// Returns the number of Displays using the Win32 functions
    /// </summary>
    /// <returns>collection of Display Info</returns>
    public static DisplayInfoCollection GetDisplays()
    {
        DisplayInfoCollection col = new DisplayInfoCollection();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                MonitorInfoEx mi = new MonitorInfoEx();
                mi.Size = (int)Marshal.SizeOf(mi);
                bool success = GetMonitorInfo(hMonitor, ref mi);
                if (success)
                {
                    DisplayInfo di = new DisplayInfo();
                    di.ScreenWidth = (mi.Monitor.right - mi.Monitor.left).ToString();
                    di.ScreenHeight = (mi.Monitor.bottom - mi.Monitor.top).ToString();
                    di.MonitorArea = mi.Monitor;
                    di.WorkArea = mi.Work;
                    di.Availability = mi.Flags.ToString();
                    col.Add(di);
                }
                return true;
            }, IntPtr.Zero);
        return col;
    }

    // size of a device name string
    private const int CCHDEVICENAME = 32;

    /// <summary>
    /// The MONITORINFOEX structure contains information about a display monitor.
    /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
    /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name 
    /// for the display monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MonitorInfoEx
    {
        /// <summary>
        /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function. 
        /// Doing so lets the function determine the type of structure you are passing to it.
        /// </summary>
        public int Size;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public Rect Monitor;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications, 
        /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor. 
        /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars. 
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public Rect Work;

        /// <summary>
        /// The attributes of the display monitor.
        /// 
        /// This member can be the following value:
        ///   1 : MONITORINFOF_PRIMARY
        /// </summary>
        public uint Flags;

        /// <summary>
        /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name, 
        /// and so can save some bytes by using a MONITORINFO structure.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string DeviceName;

        public void Init()
        {
            this.Size = 40 + 2 * CCHDEVICENAME;
            this.DeviceName = string.Empty;
        }
    }

    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive. 
    /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle. 
    /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including, 
    /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectStruct
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;
    }

        static void Main(string[] args)
        {
            //TestNative.GetFileInformation.CallMe("Foo.json");

            DisplayInfoCollection displays = GetDisplays();

            List<string> filesOfInterest = new List<string>();

            Dictionary<String, List<FileInformation>> oldFiles = new Dictionary<String, List<FileInformation>>();
            Dictionary<String, FileInformation> nameToFileInformation = new Dictionary<String, FileInformation>();
            Dictionary<String, String> nameToHash = new Dictionary<String, String>();
            {
                FileStream instream = File.OpenRead("index.json");
                DataContractJsonSerializer inserializer = new DataContractJsonSerializer(typeof(Dictionary<String, List<FileInformation>>));
                oldFiles = (Dictionary<String, List<FileInformation>>)inserializer.ReadObject(instream);                                      

                //stream.Position = 0;
                //FileStream output = File.Create("index.json");
                //stream.CopyTo(output);
                //output.Close();
            }

            foreach (String hash in oldFiles.Keys)
            {
                List<FileInformation> files = oldFiles[hash];
                foreach (FileInformation information in files)
                {
                    nameToHash[information.path] = hash;
                    nameToFileInformation[information.path] = information;
                }
            }

            string where = Directory.GetCurrentDirectory();

            string target = ".";
            string alt = null;
            if (args.Length >= 1)
            {
                target = args[0];
                alt = target + "xx";
            }

            // If we have an alternate tree location but it doesn't currently exist then create it.
            if (alt != null)
            {
                if (!Directory.Exists(alt))
                {
                    Directory.CreateDirectory(alt);
                }
            }

            getFilesIn(target, filesOfInterest);

            // Track known things...
            Dictionary<String, List<FileInformation>> knownFiles = new Dictionary<String, List<FileInformation>>();
            Dictionary<String, List<FileInformation>> workingFiles = new Dictionary<String, List<FileInformation>>();

            filesOfInterest.Sort();
            int totalFiles = filesOfInterest.Count;
            int processedFiles = 0;
            foreach (string file in filesOfInterest)
            {
                FileInfo information = new FileInfo(file);
                if (information.Length < (2L * 1024L * 1024L * 1024L) - 1L)
                {
                    byte[] data = File.ReadAllBytes(file);
                    using (SHA256 hash = SHA256.Create())
                    {
                        byte[] hashValue = hash.ComputeHash(data);

                        string asBase64 = Convert.ToBase64String(hashValue);

                        StringBuilder builder = new StringBuilder();
                        foreach (byte value in hashValue)
                        {
                            builder.Append(value.ToString("x2"));
                        }
                        string result = builder.ToString();

                        Console.WriteLine("[" + processedFiles + " of " + totalFiles + "]");
                        Console.WriteLine("\"" + file + "\"");
                        Console.WriteLine("    " + asBase64);
                        string id = TestNative.GetFileInformation.GetFileIdentity(file);

                        // Bump the processed files count.
                        processedFiles += 1;

                        if (!knownFiles.ContainsKey(asBase64))
                        {
                            knownFiles.Add(asBase64, new List<FileInformation>());
                        }
                        else
                        {
                            Console.WriteLine("-{0}- Entries", knownFiles[asBase64].Count + 1);
                        }

                        // Add in another known instance of this file.
                        knownFiles[asBase64].Add(new FileInformation(file));

                        if (!workingFiles.ContainsKey(asBase64))
                        {
                            workingFiles.Add(asBase64, new List<FileInformation>());

                            // Add in our kept file...
                            workingFiles[asBase64].Add(new FileInformation(file));
                        }
                        else
                        {
                            Console.WriteLine("Keep {0}", workingFiles[asBase64][0].path);

                            //FileInformation thisOne = file;


                            //int endSeparator = thisOne.path.LastIndexOf('\\');

                            string pathPart = file.Substring(target.Length);
                            int endSeparator = pathPart.LastIndexOf('\\');
                            string finalPart = pathPart.Substring(0, endSeparator);
                            string baseName = pathPart.Substring(endSeparator + 1);
                            string altPath = alt + finalPart;
                            if (!Directory.Exists(altPath))
                            {
                                Directory.CreateDirectory(altPath);
                            }

                            string altName = altPath + "\\" + baseName;
                            if (!File.Exists(altName))
                            {
                                File.Move(file, altName);
                                Console.WriteLine("Move {0} -> {1}", file, altName);
                            }
                            else
                            {
                                Console.WriteLine("Skip {0} -> {1}", file, altName);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Found " + knownFiles.Count + " unique files out of " + filesOfInterest.Count);
#if false
            // Walk through the list of unique files that we found...
            foreach (string key in knownFiles.Keys)
            {
                // This one has duplicates and we have an alternate location
                if (knownFiles[key].Count > 1 && alt != null)
                {
                    for (int ct = 1; ct < knownFiles[key].Count; ct++)
                    {
                        // Grab the information we have about this file.
                        FileInformation thisOne = knownFiles[key][ct];


                        //int endSeparator = thisOne.path.LastIndexOf('\\');
                        
                        string pathPart = thisOne.path.Substring(target.Length);
                        int endSeparator = pathPart.LastIndexOf('\\');
                        string finalPart = pathPart.Substring(0, endSeparator);
                        string baseName = pathPart.Substring(endSeparator + 1);
                        string altPath = alt + finalPart;
                        if (!Directory.Exists(altPath))
                        {
                            Directory.CreateDirectory(altPath);                            
                        }

                        string altName = altPath + "\\" + baseName;
                        if (!File.Exists(altName))
                        {
                            File.Move(thisOne.path, altName);
                        }
                    }
                }
            }
#endif

                MemoryStream stream = new MemoryStream();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<String, List<FileInformation>>));
                serializer.WriteObject(stream, knownFiles);

                stream.Position = 0;
                FileStream output = File.Create("index.json");
                stream.CopyTo(output);
                output.Close();
            }
        
        private static void getFilesIn(string where, List<string> files)
        {
            string[] moreFolders = Directory.GetDirectories(where);
            string[] moreFiles = Directory.GetFiles(where);

            files.AddRange(moreFiles);

            foreach (string directory in moreFolders)
            {
                getFilesIn(directory, files);
            }
        }
    }
}
