using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FindAndCopyFiles
{
    class Program
    {
#if false
        static private string[] sources =
        {
            "C:\\Users\\Kyle Wilson\\Downloads\\eMule\\Incoming",
            "\\\\arioch\\Incoming",
            "\\\\boojum\\Incoming",
            "\\\\CHESHIRECAT64\\Incoming",
            "\\\\jabberwock\\Incoming",
            "\\\\loviatar\\Incoming2"
        };

        static private string destination = "C:\\Users\\Kyle Wilson\\Downloads\\eMule\\zzz_317";

        static private string[] destinations =
        {
            "C:\\Users\\Kyle Wilson\\Downloads\\eMule\\zzz_317",
            "V:\\eBooks\\zzz_317",
            "T:\\eBooks\\zzz_317",
            "h:\\EBooks\\Processed\\Games\\ZZ_Working"
        };
#endif
        static void Main(string[] args)
        {
            string ppath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string getMyJson = File.ReadAllText(ppath + "\\FindFilesConfiguration.json");
            FindFilesConfiguration reread = JsonConvert.DeserializeObject<FindFilesConfiguration>(getMyJson);
            Console.WriteLine($"Read configuration {reread.sources.Count} sources to {reread.destinations.Count} destinations");

            // Accumulate list of all files in all of the source folders.
            List<string> files = new List<string>();
            foreach (var item in reread.sources)
            {
                FindAllFiles(ref files, item);
            }
            Console.WriteLine($"Found {files.Count} files");
            
            // Walk through each file we found and do what needs to be done.
            foreach (var item in files)
            {
                Console.WriteLine($"Transfer: \"{item}\"");

                // Move this file to our first target folder.
                var testItem = new FileTransferHelper(item, reread.destinations.First());
                testItem.DoMove();
                Console.WriteLine($"  {testItem.IsDirect}  To: \"{testItem.FinalToPath}\"");

                // Now copy the file from the initial target folder to the other target folders where we want copies.
                foreach (var dest in reread.destinations)
                {
                    if (dest != testItem.To)
                    {
                        var restItems = new FileTransferHelper(testItem.FinalToPath, dest);
                        restItems.DoCopy();
                        Console.WriteLine($"      Also: \"{restItems.FinalToPath}\"");
                    }
                }
            }

            Console.WriteLine(".");
        }

        static
            void
            FindAllFiles(
                ref List<string> informationHere,
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
                informationHere.AddRange(files);
            }
        }

#if false
        static
            string
            MoveAndDontReplace(
                string from,
                string to
            )
        {
            string final = "";

            FileInfo fromInfo = new FileInfo(from);
            string extension = fromInfo.Extension;
            string name = fromInfo.Name;
            string baseName = name.Substring(0, name.Length - extension.Length);

            string tryTarget = to + "\\" + baseName + extension;
            int bump = 1;
            if (!File.Exists(tryTarget))
            {

            }

            else
            {
                tryTarget = to + "\\" + baseName + $" ({bump})" + extension;
                while (File.Exists(tryTarget))
                {
                    bump += 1;
                    tryTarget = to + "\\" + baseName + $" ({bump})" + extension;
                }
            }

            final = tryTarget;

            return (final);
        }
#endif
    }
}
