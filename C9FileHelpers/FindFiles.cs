using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace C9FileHelpers
{
    public class FindFiles
    {
        /// <summary>
        /// The path we were asked to scan for files.
        /// </summary>
        public string BasePath { get; }
        private List<string> fileList = new List<string>();

        /// <summary>
        /// The list of files found in all subfolders under BasePath.
        /// </summary>
        public IList<string> FileList
        {
            get => fileList.AsReadOnly();
        }

        private List<string> foldersScanned = new List<string>();

        /// <summary>
        /// The list of folders that were scanned under BasePath
        /// </summary>
        public IList<string> FoldersScanned
        {
            get => foldersScanned.AsReadOnly();
        }

        /// <summary>
        /// RAII compile a list of all files and folders under the path we're provided.
        /// </summary>
        /// <param name="where"></param>
        public FindFiles(string where)
        {
            BasePath = where;

            // Working list of folders remaining to be scanned.
            List<string> moreFolders = new List<string>();
            string checkHere = where;
            do
            {
                // Find out what is here.
                string[] folderList = Directory.GetDirectories(checkHere);
                string[] moreFiles = Directory.GetFiles(checkHere);

                // Keep accumulating lists of files and folders.
                fileList.AddRange(moreFiles);
                foldersScanned.AddRange(folderList);

                // Add these folders to the working list remaining to be scanned.
                moreFolders.AddRange(folderList);

                // Pop the top item as the next place to go looking.
                checkHere = moreFolders[0];
                moreFolders.RemoveAt(0);
            } while (moreFolders.Count > 0);
        }
    }
}
