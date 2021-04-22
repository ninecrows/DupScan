using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using C9Native;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace FindHashDuplicates
{
    class Program
    {
        static void Main(string[] args)

        {
           string whereToScan = "h:\\EBooks";

            if (args.Length > 0)
            {
                whereToScan = args[0];
            }

            bool doMoveDuplicates = true;

            Console.WriteLine($"Scan \"{whereToScan}\"");

            // Get our list of files in this area.
            var files = new FindFiles(whereToScan);

            string stamp = DateTime.Now.ToString();
            stamp = stamp.Replace("/", "-");
            stamp = stamp.Replace(":", "-");
            string whereHold = whereToScan + " " + stamp;

            Console.WriteLine($"Found {files.Count} files");

            string mongodbConnectionString = "mongodb://localhost:27017";

            //  Connect to our database
            var client = new MongoClient(mongodbConnectionString);

            var database = client.GetDatabase("BooksIndex");

            // List of volumes that are present in this system.
            VolumeInformationStore liveVolumes = new VolumeInformationStore();
            VolumeInformationStore allKnownVolumes = null;
            {
                var volumesInDatabase = database.GetCollection<VolumeInformationItem>("Volumes");
                foreach (var vi in liveVolumes)
                {
                    var afilter = Builders<VolumeInformationItem>.Filter.Eq("serialnumber", vi.SerialNumber);
                    var acollation = new FindOptions()
                        {Collation = new Collation("en_US", strength: CollationStrength.Secondary)};
                    var afindOptions = new FindOptions()
                        {Collation = new Collation("en", strength: CollationStrength.Primary)};
                    var ahits = volumesInDatabase.Find(afilter, acollation);
                    var acountHits = ahits.Count();
                    var athisList = ahits.ToList();

                    if (acountHits == 0)
                    {
                        volumesInDatabase.InsertOne(vi);
                    }
                }
                allKnownVolumes = new(volumesInDatabase.AsQueryable().ToList());
            }
            
            var collection = database.GetCollection<FileHashInformation>("HashIndexOut");
            var checkCollection = database.GetCollection<FileHashInformation>("HashIndexCheck");

#if false
            var outcollection = database.GetCollection<FileHashInformation>("HashIndexOut");

            // Update the schema so we have 'real' information.
            {
                var filter = Builders<FileHashInformation>.Filter.Eq("version", BsonNull.Value);
                var cursor = collection.Find(filter);

                var count = cursor.Count();
                var remains = count;
                int up = 0;
                var cursorlist = cursor.ToList();
                foreach (var ii in cursorlist)
                {
                    //Console.WriteLine($"\"{ii.Path}\"");
                    var alreadyDone = Builders<FileHashInformation>.Filter.Eq("path", ii.Path);
                    var findOptions = new FindOptions()
                        { Collation = new Collation("en", strength: CollationStrength.Secondary) };
                    var checker = outcollection.Find(alreadyDone, findOptions);

                    up++;

                    if (checker.Count() == 0)
                    {
                        //Console.WriteLine($"\"{ii.Path}\"");
                        var changed = ii.FixIfMissing(liveVolumes);
                        outcollection.InsertOne(ii);
                        remains--;
                        if (up % 500 == 0)
                        {
                            Console.WriteLine($"\tDone {up} -> {remains} of {count}");
                        }
                    }
                    else
                    {
                        remains--;
                        if (up % 500 == 0)
                        {
                            Console.WriteLine($"\tSkip {up} -> {remains} of {count}");
                        }
                    }
                }
            }
#endif

            // Index of known files by hash.
            var index = new Dictionary<string, FileHashInformation>();
            
            {
                long itemCount = 0;
                long itemsLeft = files.Count;
                foreach (var item in files)
                {
                    var thisFile = new FileHashInformation(item, liveVolumes);
                    //thisFile.FixIfMissing(liveVolumes);
                    //var thisId = new FileInformationFileId(item);

                    var filter1 = Builders<FileHashInformation>.Filter.Eq("basepath", thisFile.BasePath);
                    //var filter2 = Builders<FileHashInformation>.Filter.Eq("volumeid", thisFile.VolumeId);
                    //var filter = Builders<FileHashInformation>.Filter.And(filter1, filter2);

                    // Another way to build this composite filter?
                    //var builder = Builders<FileHashInformation>.Filter;
                    //var filterz =
                     //   builder.Eq("volumeid", thisFile.VolumeId) &
                      //  builder.Eq("basepath", thisFile.BasePath);

                    //Builders<FileHashInformation>.Filter.Eq("path", item);
                    var collation = new FindOptions()
                        {Collation = new Collation("en", strength: CollationStrength.Secondary)};
                    //var findOptions = new FindOptions()
                    //    {Collation = new Collation("en", strength: CollationStrength.Primary)};
                    var hits = collection.Find(filter1, collation);
                    var countHits = hits.Count();
                    var thisList = hits.ToList();

                    int thisHits = 0;
                    FileHashInformation thisMatch = null;
                    foreach (var itemFound in thisList)
                    {
                        if (itemFound.VolumeId == thisFile.VolumeId)
                        {
                            thisMatch = itemFound;
                            thisHits++;
                        }
                    }

                    itemCount++;
                    itemsLeft--;

                    // If it looks like we've already processed this one...
                    if (thisMatch != null && thisFile.IsMatch(thisMatch))
                    {
                        // Announce if we get more than one hit for a given item...this is probably an error.
                        if (thisHits > 1)
                        {
                            Console.WriteLine($"Multiple hits {countHits} => \"{item}\"");
                            foreach (var ii in hits.ToEnumerable())
                            {
                                Console.WriteLine($"    \"{ii.Path}\"");
                            }
                        }

                        {
                            var thisHit = thisMatch; // hits.FirstOrDefault();

                            // Inject the stored hash code.
                            thisFile.AddHash(thisHit.Sha256);
                        }

                        if ((itemCount % 100) == 0)
                        {
                            Console.WriteLine($"Cached: {itemCount} / {itemsLeft}");
                        }

                        // A file with this same contents is elsewhere in this area.
                        if (index.ContainsKey(thisFile.Sha256))
                        {
                            Console.WriteLine($"Keep: \"{index[thisFile.Sha256].Path}\"");

                            if (doMoveDuplicates)
                            {
                                // Update this record in the database to 'exists'.
                                string stub = item.Substring(whereToScan.Length);
                                ShiftFile(whereHold + stub, item);
                            }
                        }

                        // No such file yet, add this one in.
                        else
                        {
                            index[thisFile.Sha256] = thisFile;
                        }
                    }

                    // New file that hasn't been seen before.
                    else if (thisFile.Exists)
                    {
                        if (thisFile.Size < (2L * 1024 * 1024 * 1024))
                        {
                            Console.Write($"{itemsLeft}: ");
                            thisFile.MakeHash();
                            collection.InsertOne(thisFile);
                            checkCollection.InsertOne(thisFile);
                            Console.WriteLine($"\"{item}\" Done");

                            // A file with this same contents is elsewhere in this area.
                            if (index.ContainsKey(thisFile.Sha256))
                            {
                                Console.WriteLine($"Keep: \"{index[thisFile.Sha256].Path}\"");

                                if (doMoveDuplicates)
                                {
                                    // Update this record in the database to 'exists'.
                                    string stub = item.Substring(whereToScan.Length);
                                    ShiftFile(whereHold + stub, item);
                                }
                            }

                            // No such file yet, add this one in.
                            else
                            {
                                index[thisFile.Sha256] = thisFile;
                            }
                        }

                        else
                        {
                            Console.WriteLine($"Big hash {thisFile.Path}");

                            using (FileStream fs = File.OpenRead(thisFile.Path))
                            {
                                using var sha256 = SHA256.Create();
                                {
                                    var hash = sha256.ComputeHash(fs);
                                    thisFile.AddHash(Convert.ToBase64String(hash));
                                    collection.InsertOne(thisFile);
                                    checkCollection.InsertOne(thisFile);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ShiftFile(
            string whereTo,
            string path
        )
        {
            var fi = new FileInfo(path);
            //string destPath = whereTo + "\\" + fi.Name;
            var fo = new FileInfo(whereTo);
            
            if (!Directory.Exists(fo.DirectoryName))
            {
                Directory.CreateDirectory(fo.DirectoryName);
            }

            Console.WriteLine($"Shift \"{path}\" -> \"{whereTo}\"");

            File.Move(path, whereTo, false);
        }
    }
}
