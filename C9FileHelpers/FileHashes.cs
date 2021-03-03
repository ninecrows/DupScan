using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C9FileHelpers
{
    [Serializable]
    public class FileHashes
    {
        public string fileHashSha256;

        [OptionalField] public string fileHashSha512;

        [OptionalField] public string fileHashSha1;

        [OptionalField] public string fileHashMd5;

        [OptionalField] public string crc32;

        public FileHashes()
        {

        }

        /// <summary>
        /// Calculate hashes by reading the file and spinning threads to calculate hashes.
        /// </summary>
        /// <param name="fileName"></param>
        public void makeHashes(string fileName)
        {

        }
    }
}
