﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Json;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using C9DupScan;
using C9FileHelpers;
using C9Native;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace DupScan
{
    [Serializable]
    class FileInformation
    {
        /// <summary>
        /// Given the path to the file we're interested in, load up this object with all of the relevant information.
        /// </summary>
        /// <param name="fileName">Path to the file that we're intersted in getting information from</param>
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

            String fileId = C9Native.GetFileInformation.GetFileIdentity(fileName);
        }

        /// <summary>
        /// Full path to the file we're interested in at the time of the scan. Includes drive letter, directory path, file name and extension.
        /// </summary>
        public string path;

        /// <summary>
        /// Just the path to the file. Includes the drive letter at the time of the scan and the directory path but not file name parts.
        /// </summary>
        public string directory;

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long size;

        /// <summary>
        /// Time stamp when this file was created in 100 nSec ticks.
        /// </summary>
        public DateTime created;

        public string created8601;

        /// <summary>
        /// Time stamp when this file was last modified in 100 nSec ticks.
        /// </summary>
        public DateTime modified;

        public string modified8601;

        /// <summary>
        /// Windows unique file id for this file. Contains volume identifier and file identifier.
        /// </summary>
        [OptionalField] public string fileId;

        /// <summary>
        /// SHA256 hash of the contents of this file.
        /// </summary>
        [OptionalField] public string fileHash;

        /// <summary>
        /// Volume ID of the drive volume this file resides on.
        /// </summary>
        [OptionalField] public string volumeId;

        /// <summary>
        /// Timestamp when this file was first recorded
        /// </summary>
        [OptionalField] public DateTime firstSeen;

        /// <summary>
        /// Timestamp when this file was most recently recorded.
        /// </summary>
        [OptionalField] public DateTime lastSeen;

        /// <summary>
        /// The path to the file (not including file name or drive letter) as an ordered list of path elements.
        /// </summary>
        [OptionalField] public string[] pathElements;

        /// <summary>
        /// The drive letter that this file was most recently associated with.
        /// </summary>
        [OptionalField] public string driveLetter;

        /// <summary>
        /// The label string for the drive on which this file was most recently seen.
        /// </summary>
        [OptionalField] public string driveLabel;

        /// <summary>
        /// The base name for this file without extension or path.
        /// </summary>
        [OptionalField] public string fileName;

        /// <summary>
        /// Just the extension for this file. Intended to assist with searches.
        /// </summary>
        [OptionalField] public string fileExtension;

        /// <summary>
        /// Array of string tags indicating what sorts of validation this file has passed of failed.
        /// </summary>
        [OptionalField] public string[] verified;

        /// <summary>
        /// Notes related to verification, type of damage, missing pages, corrupted images and such.
        /// </summary>
        [OptionalField] public string[] verificationNotes;

        /// <summary>
        /// Human readable file type(s) associated with this file
        /// </summary>
        [OptionalField] public string[] fileType;

        /// <summary>
        /// First 'n' bytes as base64 of this file. Used to retrieve information even if the whole file is not immediately available.
        /// </summary>
        [OptionalField] public string beginsWith;

        /// <summary>
        /// Last 'n bytes as base64 of this file. Used to retrieve information even if the whole file is not immediately available.
        /// </summary>
        [OptionalField] public string endsWith;

        /// <summary>
        /// Book publisher if a book.
        /// </summary>
        [OptionalField] public string publisher;

        /// <summary>
        /// Book authors if a book.
        /// </summary>
        [OptionalField] public string[] authors;

        /// <summary>
        /// Book title if a book.
        /// </summary>
        [OptionalField] public string title;

        /// <summary>
        /// Book pages if a book.
        /// </summary>
        [OptionalField] public int pages;

        /// <summary>
        /// Book copyrighht date if a book.
        /// </summary>
        [OptionalField] public int copyright;

        /// <summary>
        /// Book edition (if any) if a book.
        /// </summary>
        [OptionalField] public string edition;

        /// <summary>
        /// Quality of this file if any. Types of damage and other related issues as a string.
        /// </summary>
        [OptionalField] public string quality;

        /// <summary>
        /// Categories that this item is part of. Could be fiction, games, sf, fantasy, techincal, C# or many others. Used to facilitate searches.
        /// </summary>
        [OptionalField] public string[] categories;

        public String GetName()
        {
            String checkPath;

            var pieces = path.Split('\\');

            return (pieces.Last());
        }

        public String GetFullPath()
        {
            return (directory + "\\" + GetName());
        }

        public bool PathMatches(String realFile)
        {
            String fullPath = GetFullPath().ToLower();
            String realPath = realFile.ToLower();

            return (fullPath.Equals(realPath));
        }

        public bool FileMatches(FileInfo realFile)
        {
            bool result = false;

            if (PathMatches(realFile.FullName))
            {
                if (size == realFile.Length)
                {
                    if (modified == realFile.LastWriteTime)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    class Program
    {
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
                delegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    MonitorInfoEx mi = new MonitorInfoEx();
                    mi.Size = (int) Marshal.SizeOf(mi);
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

        private static Logger log = new Logger("DupLog");

        static void Main(string[] args)
        {
            log.Add("Starting in \"" + Directory.GetCurrentDirectory() + "\"");

            var iid = new FileInformationFileId("h:\\index.json");

            var hashes = new FileHashes("h:\\index.json");

            string wherzzze = Directory.GetCurrentDirectory();
            FindFiles filesOfInterestHerezzz = new FindFiles(wherzzze);

            string ss = JsonConvert.SerializeObject(hashes, Formatting.Indented);
            log.Add(ss);
            var hffhh = JsonConvert.DeserializeObject<FileHashes>(ss);

            byte[] bsonData;
            {
                var ms = new MemoryStream();

                using var writer = new Newtonsoft.Json.Bson.BsonWriter(ms);

                //writer.WriteRaw(ss);
                var ser = new JsonSerializer();
                ser.Serialize(writer, hashes);
                bsonData = ms.ToArray();
            }

            // Full map of volumes on the system and their particulars.
            Volumes volumes = new Volumes();
            //TestNative.GetFileInformation.CallMe("Foo.json");

            var displays = GetDisplays();

            var filesOfInterest = new List<string>();

            var oldFiles = new Dictionary<string, List<FileInformation>>();
            var nameToFileInformation = new Dictionary<string, FileInformation>();
            var idToFIleInformation = new Dictionary<string, FileInformation>();
            var nameToHash = new Dictionary<string, string>();

            MongoClient client = null;
            IMongoDatabase database = null;
            try
            {
                client = new MongoClient("mongodb://localhost:27017");
                database = client.GetDatabase("EBooks");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to MongoDb \"" + ex.ToString() + "\"");
                //throw;
            }

            // Test storing hashes into this DB.
            {
                var testCollection = database.GetCollection<BsonDocument>("Test");
                var document = new BsonDocument();
                var ddd = BsonDocument.Parse(ss);
                testCollection.InsertOne(ddd);

                var c2 = database.GetCollection<FileHashes>("HashTest");
                c2.InsertOne(hashes);
            }

            IMongoCollection<MongoDB.Bson.BsonDocument> fileslist = null;
            if (database != null)
            {
                fileslist = database.GetCollection<MongoDB.Bson.BsonDocument>("FilesList");
            }

            // If we have an existing index for this folder then lets use it.
            if (File.Exists("index.json"))
            {
                if (System.IO.File.Exists("index.json"))
                {
                    try
                    {
                        FileStream instream = File.OpenRead("index.json");
                        DataContractJsonSerializer inserializer =
                            new DataContractJsonSerializer(typeof(Dictionary<String, List<FileInformation>>));
                        oldFiles = (Dictionary<String, List<FileInformation>>) inserializer.ReadObject(instream);
                        instream.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception reading index \"" + ex.ToString() + "\"");
                        //throw;
                    }
                }

                //stream.Position = 0;
                //FileStream output = File.Create("index.json");
                //stream.CopyTo(output);
                //output.Close();

                // Build look ups...
                foreach (String hash in oldFiles.Keys)
                {
                    List<FileInformation> files = oldFiles[hash];
                    foreach (FileInformation information in files)
                    {
                        // Store the hash back into the object if it isn't already there.
                        if (information.fileHash == null)
                        {
                            information.fileHash = hash;
                        }

                        var when = information.created.Ticks;
                        string json = JsonConvert.SerializeObject(information, Formatting.Indented);
                        Console.WriteLine("{0},\n", json);

                        // Store the file id if it isn't alraedy there.
                        if ((information.fileId == null) && File.Exists(information.path))
                        {
                            var iii = new C9Native.FileInformationFileId(information.path);
                            var hhh = new FileHashes(information.path);

                            var newStyle = new C9FileHelpers.FileInformation(information.path);
                            string jjj = JsonConvert.SerializeObject(newStyle, Formatting.Indented);
                            log.Add(jjj);

                            if (newStyle.fullPath.Length >= 2 && newStyle.fullPath[1] == ':')
                            {
                                var driveLetter = newStyle.fullPath[0];
                                string drivePath = driveLetter + ":\\";
                                StringBuilder volumeName = new StringBuilder(1024);
                                int nameSize = 1024;
                                uint volumeSerialNumber = 0;
                                uint maxLength = 1024;
                                C9Native.FileSystemFeature feature = 0;
                                StringBuilder FileSystemName = new StringBuilder(1024);
                                int namesize = 1024;
                                bool result = C9Native.GetFileInformation.GetVolumeInformationW(
                                    drivePath, volumeName, nameSize,
                                     out volumeSerialNumber,  out maxLength,
                                    out feature,
                                    FileSystemName,
                                    namesize
                                );
                            }

                            String fileId = C9Native.GetFileInformation.GetFileIdentity(information.path);
                            information.fileId = fileId;

                            //Console.WriteLine("Got {0} -> {1}", fileId, information.path);
                        }

                        nameToHash[information.path] = hash;
                        nameToFileInformation[information.path] = information;
                    }
                }
            }

            string where = Directory.GetCurrentDirectory();
            FindFiles filesOfInterestHere = new FindFiles(where);

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

            // List of files that we could look at...
            getFilesIn(target, filesOfInterest);

            // Track known things...
            Dictionary<String, List<FileInformation>> previousFiles = new Dictionary<String, List<FileInformation>>();
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
                    try
                    {
                        if (nameToFileInformation.ContainsKey(file))
                        {
                            if (nameToFileInformation[file].FileMatches(information))
                            {
                                Console.WriteLine(">" + file + "< already found");
                                if (!previousFiles.ContainsKey(file))
                                {
                                    previousFiles[file] = new List<FileInformation>();
                                }

                                // Find the pieces of information we normally need.
                                var myInformation = nameToFileInformation[file];
                                String asBase64 = nameToFileInformation[file].fileHash;
                                previousFiles[file].Add(nameToFileInformation[file]);

                                // If we don't have an entry for this hash yet then we need to add one.
                                if (!knownFiles.ContainsKey(asBase64))
                                {
                                    knownFiles[asBase64] = new List<FileInformation>();
                                }

                                // Add the file entry to this hash item.
                                knownFiles[asBase64].Add(myInformation);

                                Console.WriteLine("Previous \"" + file + "\"");
                            }
                            else
                            {
                                Console.WriteLine("#" + file + "# not found");
                            }
                        }
                        else
                        {
                            String asBase64 = null;
                            String result = "";
                            byte[] data = File.ReadAllBytes(file);
                            using (SHA256 hash = SHA256.Create())
                            {
                                byte[] hashValue = hash.ComputeHash(data);

                                asBase64 = Convert.ToBase64String(hashValue);


                                StringBuilder builder = new StringBuilder();
                                foreach (byte value in hashValue)
                                {
                                    builder.Append(value.ToString("x2"));
                                }

                                result = builder.ToString();
                            }

                            Console.WriteLine("[" + processedFiles + " of " + totalFiles + "]");
                            Console.WriteLine("\"" + file + "\"");
                            Console.WriteLine("    " + asBase64);
                            string id = C9Native.GetFileInformation.GetFileIdentity(file);

                            // Bump the processed files count.
                            processedFiles += 1;

                            //if (previousFiles.ContainsKey(file))
                            // {
                            //    Console.WriteLine("Already Known \"" + file + "\"");
                            //}

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
                    catch (Exception ex)
                    {
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
            DataContractJsonSerializer serializer =
                new DataContractJsonSerializer(typeof(Dictionary<String, List<FileInformation>>));
            serializer.WriteObject(stream, knownFiles);

            if (false)
            {
                stream.Position = 0;
                FileStream output = File.Create("index.json");
                stream.CopyTo(output);
                output.Close();
            }
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
