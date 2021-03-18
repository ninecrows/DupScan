using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace C9FileHelpers
{
    [Serializable]
    public class FileInformation
    {
        public FileInformation(string fileName)
        {
            path = fileName;

            FileInfo information = new (fileName);

            created = new ExTimeStamp(information.CreationTime); 
            
            modified = new ExTimeStamp(information.LastWriteTime);
            
            size = information.Length;

            directory = information.DirectoryName;
            fullPath = information.FullName;
            //var aFileId = C9Native.GetFileInformation.GetFileIdentity(fileName); 
        }

        public string path;
        public string fullPath;
        public string directory;
        public long size;

        public ExTimeStamp created;
        
        public ExTimeStamp modified;

        /// <summary>
        /// The path to the file (not including file name or drive letter) as an ordered list of path elements.
        /// </summary>
        [OptionalField] public string[] pathElements;

        /// <summary>
        /// The drive letter that this file was most recently associated with.
        /// </summary>
        [OptionalField] public string driveLetter;


        [OptionalField] public string fileId;

        /// <summary>
        /// Hashed data stored for this file.
        /// </summary>
        [OptionalField] public FileHashes hashes;

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

        public string GetName()
        {
           var pieces = path.Split('\\');

            return (pieces.Last());
        }

        public string GetFullPath()
        {
            return (directory + "\\" + GetName());
        }

        public bool PathMatches(string realFile)
        {
            var aFullPath = GetFullPath().ToLower();
            var realPath = realFile.ToLower();

            return aFullPath.Equals(realPath);
        }

        public bool FileMatches(FileInfo realFile)
        {
            var result = PathMatches(realFile.FullName) && size == realFile.Length && modified.Raw == realFile.LastWriteTime.Ticks;

            return result;
        }
    }
}
