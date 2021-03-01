using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace C9FileHelpers
{
    [Serializable]
    public class FileInformation
    {
        public FileInformation(string fileName)
        {
            path = fileName;

            FileInfo information = new FileInfo(fileName);

            created = new ExTimeStamp(information.CreationTime); //File.GetCreationTime(path);
            //created8601 = created.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            modified = new ExTimeStamp(information.LastWriteTime); //information.LastWriteTime; //File.GetLastWriteTime(path);
            // modified8601 = modified.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            size = information.Length;

            directory = information.DirectoryName;
            fullPath = information.FullName;
            String fileId = "";  //C9Native.GetFileInformation.GetFileIdentity(fileName);
        }

        public string path;
        public string fullPath;
        public string directory;
        public long size;

        public /*DateTime*/ ExTimeStamp created;
        //public string created8601;
        public /*DateTime*/ ExTimeStamp modified;
        //public string modified8601;

        [OptionalField]
        public string fileId;

        [OptionalField]
        public string fileHashSha256;

        [OptionalField]
        public string fileHashSha512;

        [OptionalField]
        public string fileHashSha1;

        [OptionalField]
        public string fileHashMd5;

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
        /// Optional human provided information on book type files.
        /// </summary>
        [OptionalField] public BookInformation book;

        /// <summary>
        /// Book publisher if a book.
        /// </summary>
       // [OptionalField] public string publisher;

        /// <summary>
        /// Book authors if a book.
        /// </summary>
       // [OptionalField] public string[] authors;

        /// <summary>
        /// Book title if a book.
        /// </summary>
       // [OptionalField] public string title;

        /// <summary>
        /// Book pages if a book.
        /// </summary>
       // [OptionalField] public int pages;

        /// <summary>
        /// Book copyrighht date if a book.
        /// </summary>
        //[OptionalField] public int copyright;

        /// <summary>
        /// Book edition (if any) if a book.
        /// </summary>
        //[OptionalField] public string edition;

        /// <summary>
        /// Quality of this file if any. Types of damage and other related issues as a string.
        /// </summary>
       // [OptionalField] public string quality;

        /// <summary>
        /// Categories that this item is part of. Could be fiction, games, sf, fantasy, techincal, C# or many others. Used to facilitate searches.
        /// </summary>
        //[OptionalField] public string[] categories;

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
                    if (modified.Raw == realFile.LastWriteTime.Ticks)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }
}
