using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace DupScan
{
    [Serializable]
    class FileInformation
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
        }

        public string path;
        public string directory;
        public long size;

        public DateTime created;
        public string created8601;
        public DateTime modified;
        public string modified8601;
    }

    class Program
    {
        static void Main(string[] args)
        {
            //TestNative.GetFileInformation.CallMe("Foo.json");

            List<string> filesOfInterest = new List<string>();

            string where = Directory.GetCurrentDirectory();

            string target = ".";
            string alt = null;
            if (args.Length >= 1)
            {
                target = args[0];
                alt = target + "xx";
            }

            // If we have an alternate tree location but it doesn't currently exist then create it.
            if (alt != null)
            {
                if (!Directory.Exists(alt))
                {
                    Directory.CreateDirectory(alt);
                }
            }

            getFilesIn(target, filesOfInterest);

            // Track known things...
            Dictionary<String, List<FileInformation>> knownFiles = new Dictionary<String, List<FileInformation>>();

            foreach (string file in filesOfInterest)
            {
                FileInfo information = new FileInfo(file);
                if (information.Length < (2L * 1024L * 1024L * 1024L) - 1L)
                {
                    byte[] data = File.ReadAllBytes(file);
                    using (SHA256 hash = SHA256.Create())
                    {
                        byte[] hashValue = hash.ComputeHash(data);

                        string asBase64 = Convert.ToBase64String(hashValue);

                        StringBuilder builder = new StringBuilder();
                        foreach (byte value in hashValue)
                        {
                            builder.Append(value.ToString("x2"));
                        }
                        string result = builder.ToString();

                        Console.WriteLine("\"" + file + "\"");
                        Console.WriteLine("    " + asBase64);

                        if (!knownFiles.ContainsKey(asBase64))
                        {
                            knownFiles.Add(asBase64, new List<FileInformation>());
                        }

                        // Add in another known instance of this file.
                        knownFiles[asBase64].Add(new FileInformation(file));
                    }
                }
            }

            Console.WriteLine("Found " + knownFiles.Count + " unique files out of " + filesOfInterest.Count);

            // Walk through the list of unique files that we found...
            foreach (string key in knownFiles.Keys)
            {
                // This one has duplicates and we have an alternate location
                if (knownFiles[key].Count > 1 && alt != null)
                {
                    for (int ct = 1; ct < knownFiles[key].Count; ct++)
                    {
                        // Grab the information we have about this file.
                        FileInformation thisOne = knownFiles[key][ct];


                        //int endSeparator = thisOne.path.LastIndexOf('\\');
                        
                        string pathPart = thisOne.path.Substring(target.Length);
                        int endSeparator = pathPart.LastIndexOf('\\');
                        string finalPart = pathPart.Substring(0, endSeparator);
                        string baseName = pathPart.Substring(endSeparator + 1);
                        string altPath = alt + finalPart;
                        if (!Directory.Exists(altPath))
                        {
                            Directory.CreateDirectory(altPath);                            
                        }

                        string altName = altPath + "\\" + baseName;
                        if (!File.Exists(altName))
                        {
                            File.Move(thisOne.path, altName);
                        }
                    }
                }
            }

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<String, List<FileInformation>>));
            serializer.WriteObject(stream, knownFiles);

            stream.Position = 0;
            FileStream output = File.Create("index.json");
            stream.CopyTo(output);
            output.Close();
        }
        
        private static void getFilesIn(string where, List<string> files)
        {
            string[] moreFolders = Directory.GetDirectories(where);
            string[] moreFiles = Directory.GetFiles(where);

            files.AddRange(moreFiles);

            foreach (string directory in moreFolders)
            {
                getFilesIn(directory, files);
            }
        }
    }
}
