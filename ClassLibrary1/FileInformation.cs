using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            created = information.CreationTime; //File.GetCreationTime(path);
            created8601 = created.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            modified = information.LastWriteTime; //File.GetLastWriteTime(path);
            modified8601 = modified.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            size = information.Length;

            directory = information.DirectoryName;

           //String fileId = C9Native.GetFileInformation.GetFileIdentity(fileName);
        }

        public string path;
        public string directory;
        public long size;

        public DateTime created;
        public string created8601;
        public DateTime modified;
        public string modified8601;

        [OptionalField]
        public string fileId;

        [OptionalField]
        public string fileHash;

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
}
