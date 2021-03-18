using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace C9FileHelpers
{
    public class FindFiles : IEnumerable<string>
    {
        /// <summary>
        /// The path we were asked to scan for files.
        /// </summary>
        public string BasePath { get; }
        private readonly List<string> _fileList = new();

        /// <summary>
        /// The list of files found in all subfolders under BasePath.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IList<string> FileList => _fileList.AsReadOnly();

        private readonly List<string> _foldersScanned = new();

        /// <summary>
        /// The list of folders that were scanned under BasePath
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IList<string> FoldersScanned => _foldersScanned.AsReadOnly();

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
                _fileList.AddRange(moreFiles);
                _foldersScanned.AddRange(folderList);

                // Add these folders to the working list remaining to be scanned.
                moreFolders.AddRange(folderList);

                // Pop the top item as the next place to go looking.
                checkHere = moreFolders[0];
                moreFolders.RemoveAt(0);
            } while (moreFolders.Count > 0);
        }

        private class MyEnumerator : IEnumerator<string>
        {
            private readonly List<string> _fileList;
            private int _position = -1;
            //private string _current;

            public MyEnumerator(List<string> thisList)
            {
                _fileList = thisList;
            }

            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedMember.Local
            private IEnumerator getEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                _position++;
                return _position < _fileList.Count;
            }

            public void Reset()
            {
                _position = -1;
            }

            string IEnumerator<string>.Current
            {
                get
                {
                    try
                    {
                        return _fileList[_position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            public object Current
            {
                get
                {
                    return (IEnumerator<string>)Current;
                }
            }

            public void Dispose()
            {
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new MyEnumerator(_fileList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MyEnumerator(_fileList);
        }
    }
}
