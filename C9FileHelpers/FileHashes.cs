using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Force.Crc32;
using MongoDB.Bson.Serialization.IdGenerators;
//using Crc32CAlgorithm = Crc32C.Crc32CAlgorithm;

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
            MakeHashes(fileName);
        }

        /// <summary>
        /// Calculate hashes by reading the file and spinning threads to calculate hashes.
        /// </summary>
        /// <param name="fileName"></param>
        public void MakeHashes(string fileName)
        {
            var infile = File.OpenRead(fileName);

            byte[] buffer = File.ReadAllBytes(fileName);

            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] value = mySHA256.ComputeHash(buffer);

                string valueBase64 = Convert.ToBase64String(value);

                fileHashSha256 = valueBase64;
            }

            using (SHA512 mySHA512 = SHA512.Create())
            {
                byte[] value = mySHA512.ComputeHash(buffer);

                string valueBase64 = Convert.ToBase64String(value);

                fileHashSha512 = valueBase64;
            }

            using (SHA1 mySHA1 = SHA1.Create())
            {
                byte[] value = mySHA1.ComputeHash(buffer);

                string valueBase64 = Convert.ToBase64String(value);

                fileHashSha1 = valueBase64;
            }

            using (MD5 myMD5 = MD5.Create())
            {
                byte[] value = myMD5.ComputeHash(buffer);

                string valueBase64 = Convert.ToBase64String(value);

                fileHashMd5 = valueBase64;
            }

            var result = Crc32CAlgorithm.Compute(buffer);

            var trad = Crc32Algorithm.Compute(buffer);

            var segments = new BufferSegments(buffer, 64);
        }
    }

    /// <summary>
    /// Given a buffer and a size for lead and trail bits, extract what we can and provide it in various forms.
    /// </summary>
    internal class BufferSegments
    {
        /// <summary>
        /// Retrieve raw binary lead byte data.
        /// </summary>
        public byte[] LeadBytes { get; }

        /// <summary>
        /// Retrieve raw binary trail byte data.
        /// </summary>
        public byte[] TrailBytes { get; }

        /// <summary>
        /// Hex encoded lead byte data.
        /// </summary>
        public string LeadData { get; }

        /// <summary>
        /// Hex encoded trail byte data.
        /// </summary>
        public string TrailData { get; }

        /// <summary>
        /// Leading section of the buffer as text.
        /// </summary>
        public string LeadText { get; }

        /// <summary>
        /// Retrieve the binary buffer data as base 64 encoded string.
        /// </summary>
        public string LeadBase64 { get; }

        /// <summary>
        /// Retrieve the binary buffer data as base64 encoded string.
        /// </summary>
        public string TrailBase64 { get; }

        /// <summary>
        /// Given a large buffer full of file data and a length, load this object with appropriate bits.
        /// </summary>
        /// <param name="buffer">Buffer containing all of the data in the file.</param>
        /// <param name="length">Number of lead and trail bytes we'd like to keep.</param>
        public BufferSegments(byte[] buffer, int length)
        {
            // Plenty of data so we get full sized lead and trail segments.
            if (buffer.Length >= length)
            {
                LeadBytes = new byte[length];
                for (int index = 0; index < length; index++)
                {
                    LeadBytes[index] = buffer[index];
                }

                TrailBytes = new byte[length];
                for (int index = 0; index < length; index++)
                {
                    TrailBytes[index] = buffer[buffer.Length + index - length];
                }
            }

            // Short buffer so lead and trail are the same data.
            else
            {
                LeadBytes = buffer;
                TrailBytes = buffer;
            }

            // Now calculate the hex encoded versions
            LeadData = BytesToHex(LeadBytes);
            TrailData = BytesToHex(TrailBytes);

            // Grab the lead area as text characters (mostly aimed at PDF files).
            LeadText = Encoding.ASCII.GetString(LeadBytes);

            LeadBase64 = Convert.ToBase64String(LeadBytes);
            TrailBase64 = Convert.ToBase64String(TrailBytes);
        }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once UnusedMember.Local
        private static string AsciifyBuffer(IEnumerable<byte> buffer)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var current in buffer)
            {
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

            return builder.ToString();
        }

        private static string BytesToHex(byte[] buffer)
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
