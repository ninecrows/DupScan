using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindHashDuplicates
{
    class FindFiles : IEnumerable<string>
    {
        private readonly List<string> _files = new List<string>();

        public
        FindFiles(string where)
        {
            FindAllFiles(where);
        }

        public
        FindFiles(string[] where)
        {
            foreach (var item in where)
            {
                FindAllFiles(item);
            }
        }

        public
            FindFiles(IEnumerable<string> where)
        {
            foreach (var item in where)
            {
                FindAllFiles(item);
            }
        }

        public IReadOnlyList<string> Files => _files;

        public string this[int index] => _files[index];

        public int Count => _files.Count;

        private 
            void FindAllFiles(
                string pathToScan
            )
        {
            // List of paths that we need to scan. This will grow as we walk the tree and shrink as we pull items off.
            List<string> searchHere = new List<string>();
            searchHere.Add(pathToScan);

            while (searchHere.Count > 0)
            {
                // Pop the head of the list.
                string thisPath = searchHere[0];
                searchHere.RemoveAt(0);

                string[] files = Directory.GetFiles(thisPath);
                string[] folders = Directory.GetDirectories(thisPath);

                searchHere.AddRange(folders);
                _files.AddRange(files);
            }

            _files.Sort();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
