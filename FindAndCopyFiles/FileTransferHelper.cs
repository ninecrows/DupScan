using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindAndCopyFiles
{
    class FileTransferHelper
    {
        // Caller provided paths from includes file name, to is just a base path with no 'bucket'.
        private string _from;
        private string _to;

        // Base name and extension for this file.
        private string _fromExtension;
        private string _fromBase;
        private string _bucket;

        // Path to the folder where this is to be delivered.
        private string _toPath;
        private bool _createdToPath = false;

        private string _finalToPath;
        private bool _directTo = false;
        private int _offsetTo = 0;

        public FileTransferHelper(
            string from,
            string to
        )
        {
            _from = from;
            _to = to;

            var fromInfo = new FileInfo(from);
            string fromName = fromInfo.Name;
            _fromExtension = fromInfo.Extension;
            _fromBase = fromName.Substring(0, fromName.Length - _fromExtension.Length);

            // Peel the path apart and get the last path piece as our 'bucket' name.
            string[] fragments = fromInfo.DirectoryName.Split("\\");
            _bucket = fragments.Last();
            _toPath = to + "\\" + _bucket;
        }

        public string DoMove()
        {
            // Can we move directly to the name we started with and not lose data?
            if (CheckTo(SimpleToName))
            {
                // Move the file but fail if we messed up and would overwrite.
                File.Move(_from, SimpleToName, false);
                _directTo = true;
                _finalToPath = SimpleToName;
            }

            // Search for a unique name we can write to.
            else
            {
                // Start at 1 and keep trying names until we hit one that doesn't already exist.
                int offset = 1;
                while (File.Exists(OffsetToName(offset)))
                {
                    offset += 1;
                }

                File.Move(_from, OffsetToName(offset));
                _finalToPath = OffsetToName(offset);
            }

            return _finalToPath;
        }

        public string DoCopy()
        {
            // Can we move directly to the name we started with and not lose data?
            if (CheckTo(SimpleToName))
            {
                // Move the file but fail if we messed up and would overwrite.
                File.Copy(_from, SimpleToName, false);
                _directTo = true;
                _finalToPath = SimpleToName;
            }

            // Search for a unique name we can write to.
            else
            {
                // Start at 1 and keep trying names until we hit one that doesn't already exist.
                int offset = 1;
                while (File.Exists(OffsetToName(offset)))
                {
                    offset += 1;
                }

                File.Copy(_from, OffsetToName(offset), false);
                _finalToPath = OffsetToName(offset);
            }

            return _finalToPath;
        }

        public string OffsetToName(
            int offset
            )
        {
            return (SimpleToPath + "\\" + _fromBase + $" ({offset})" + _fromExtension);
        }

        public string SimpleToName => SimpleToPath + "\\" + _fromBase + _fromExtension;

        /// <summary>
        /// Path that we're going to push data to. No filename but bucket is already in place.
        /// </summary>
        public string SimpleToPath => _toPath;

        public string FinalToPath => _finalToPath;

        public string From => _from;
        public string To => _to;
        public bool IsDirect => _directTo;

        /// <summary>
        /// Check to see if the name we want to write to already exists.
        /// </summary>
        /// <param name="toPath">Path to the file we're interested in.</param>
        /// <returns>true if the target path is free to target. false if something is already there.</returns>
        public bool CheckTo(string toPath)
        {
            bool ok = false;

            // If our target folder isn't there yet, create it. Make a note for later.
            if (!Directory.Exists(_toPath))
            {
                Directory.CreateDirectory(_toPath);
                _createdToPath = true;
            }

            // If our target file doesn't exist then return true.
            if (!File.Exists(toPath))
            {
                ok = true;
            }

            return (ok);
        }
    }
}
