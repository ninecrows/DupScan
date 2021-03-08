using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson.Serialization.IdGenerators;

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

        private BufferSegments segments;

        public FileHashes()
        {

        }

        public FileHashes(string fileName)
        {
            makeHashes(fileName);
        }

        /// <summary>
        /// Calculate hashes by reading the file and spinning threads to calculate hashes.
        /// </summary>
        /// <param name="fileName"></param>
        public void makeHashes(string fileName)
        {
            //byte[] buffer;

            var infile = File.OpenRead(fileName);

            byte[] buffer = File.ReadAllBytes(fileName);

            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] value = mySHA256.ComputeHash(buffer);

                string valueBase64 = Convert.ToBase64String(value);

                fileHashSha256 = valueBase64;

                var segments = new BufferSegments(buffer, 64);
            }
        }

#if false
        public string BytesToHex(byte[] buffer)
        {
            StringBuilder working = new StringBuilder();

            foreach (var item in buffer)
            {
                working.AppendFormat("{0:X2} ", item);
            }

            return working.ToString();
        }
#endif
    }

    class BufferSegments
    {
        private byte[] leadBytes;
        private byte[] trailBytes;

        private string leadData;
        public string LeadData
        {
            get => leadData;
        }
        private string trailData;
        public string TrailData
        {
            get => trailData; 
        }

        public BufferSegments(byte[] buffer, int length)
        {
            // Plenty of data so we get full sized lead and trail segments.
            if (buffer.Length >= length)
            {
                leadBytes = new byte[length];
                for (int index = 0; index < length; index++)
                {
                    leadBytes[index] = buffer[index];
                }

                trailBytes = new byte[length];
                for (int index = 0; index < length; index++)
                {
                    trailBytes[index] = buffer[buffer.Length + index - length];
                }
            }

            // Short buffer so lead and trail are the same data.
            else
            {
                leadBytes = buffer;
                trailBytes = buffer;
            }

            // Now calculate the hex encoded versions
            leadData = BytesToHex(leadBytes);
            trailData = BytesToHex(trailBytes);

            string tttaaa = Encoding.ASCII.GetString(leadBytes);

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < trailBytes.Length; index++)
            {
                byte current = leadBytes[index];
                if ((current >= 32 && current < 0x7f) ||
                    current == 10 || current == 13 || current == 9)
                {
                    builder.Append((char)current);
                }
                else
                {
                    builder.Append(".");
                }
            }

            string readable = builder.ToString();
        }

        private string BytesToHex(byte[] buffer)
        {
            StringBuilder working = new StringBuilder();

            foreach (var item in buffer)
            {
                working.AppendFormat("{0:X2} ", item);
            }

            return working.ToString();
        }
    }
}
