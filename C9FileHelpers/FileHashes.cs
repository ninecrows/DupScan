using Force.Crc32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace C9FileHelpers
{
    [Serializable]
    public class FileHashes
    {
        /// <summary>
        /// SHA 256 hash of the file contents.
        /// </summary>
        public string Sha256 { get; private set; }

        /// <summary>
        /// SHA 512 hash of the file contents.
        /// </summary>
        [field: OptionalField]
        public string Sha512 { get; private set; }

        /// <summary>
        /// SHA1 hash of the file contents.
        /// </summary>
        [field: OptionalField]
        public string Sha1 { get; private set; }

        /// <summary>
        /// MD5 hash of the file contents.
        /// </summary>
        [field: OptionalField]
        public string Md5 { get; private set; }

        /// <summary>
        /// 32 bit CRC of the file contents.
        /// </summary>
        [field: OptionalField]
        public string Crc32 { get; private set; }

        /// <summary>
        /// Leading and trailing file parts for type checking.
        /// </summary>
        public BufferSegments Segments { get; private set; }

        /// <summary>
        /// Create an empty hash object.
        /// </summary>
        public FileHashes()
        {
        }

        /// <summary>
        /// Compute the hashes for this file
        /// </summary>
        /// <param name="fileName">path to the file to be hashed.</param>
        public FileHashes(string fileName)
        {
            MakeHashes(fileName);
        }

        /// <summary>
        /// Calculate hashes by reading the file and spinning threads to calculate hashes.
        /// </summary>
        /// <param name="fileName">path to the file to be hashed.</param>
        public void MakeHashes(string fileName)
        {
            var buffer = File.ReadAllBytes(fileName);

            using (var mySha256 = SHA256.Create())
            { 
                Sha256 = ComputeHash(mySha256, buffer); 
            }

            using (var mySha512 = SHA512.Create())
            {
                Sha512 = ComputeHash(mySha512, buffer);
            }

            using (var mySha1 = SHA1.Create())
            {

                Sha1 = ComputeHash(mySha1, buffer);
            }

            using (var myMd5 = MD5.Create())
            {
                Md5 = ComputeHash(myMd5, buffer);
            }

            var crc = Crc32Algorithm.Compute(buffer);
            Crc32 = $"{crc:X8}";

            // Grab 64 bytes leading and trailing (if available)
            Segments = new BufferSegments(buffer, 64);
        }

        private static string ComputeHash(HashAlgorithm algorithm, byte[] buffer)
        {
            var value = algorithm.ComputeHash(buffer);

            var base64 = Convert.ToBase64String(value);

            return base64;
        }
    }

    /// <summary>
    /// Given a buffer and a size for lead and trail bits, extract what we can and provide it in various forms.
    /// </summary>
    public class BufferSegments
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
        /// Intel order magic number
        /// </summary>
        public string MagicIntel { get; }

        /// <summary>
        /// Motorola order magic number
        /// </summary>
        public string MagicMotorola { get; }

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

            // Sample magic number word in both byte orders.
            if (buffer.Length > 2)
            {
                var intelMagicNumber = buffer[0] | (buffer[1] << 8);
                MagicIntel = $"{intelMagicNumber:x4}";

                var motorolaMagicNumber = buffer[1] | (buffer[0] << 8);
                MagicMotorola = $"{motorolaMagicNumber:x4}";
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
