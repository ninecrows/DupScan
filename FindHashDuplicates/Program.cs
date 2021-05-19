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
            // Move duplicates to a new folder in addition to scanning.
            bool doMoveDuplicates = false;

            // Only scan for new volumes and load them in.
            bool doOnlyVolumes = false;

            // Move all items in this folder that are archived anywhere else.
            bool doClean = false;

            // Volume scan, skip any items that can't be processed rather than failing.
            bool doVolume = false;

            string doRootedClean = null;

            List<string> pathRoots = new();

            // Scan arguments 
            foreach (var arg in args)
            {
                // Switches...must have at least two characters.
                if (arg.Length >= 2 && arg[0] == '-')
                {
                    switch (arg[1])
                    {
                        case 'c':
                            doClean = true;
                            Console.WriteLine("Clean mode");
                            break;

                        case 'd':
                            doMoveDuplicates = true;
                            Console.WriteLine("Move Duplicetes");
                            break;

                        case 'v':
                            doVolume = true;
                            Console.WriteLine("Ignore failures for volume scan");
                            break;

                        case 'V':
                            doOnlyVolumes = true;
                            Console.WriteLine("Scan Volumes Only");
                            break;

                        case 'r':
                            doRootedClean = arg.Substring(2);
                            Console.WriteLine($"Rooted clean at \"{doRootedClean}\"");
                            break;

                        default:
                            Console.WriteLine($"Unknown switch \"{arg}\"");
                            break;
                    }
                }
                else
                {
                    pathRoots.Add(arg);
                }
            }

            if (pathRoots.Count == 0 && !doOnlyVolumes)
            {
                Console.WriteLine("Nothing to do, so we're done");
             System.Environment.Exit(0);   
            }

            {
                string scanlist = string.Join("\" \"", pathRoots);
                Console.WriteLine($"Scan \"{scanlist}\"");
            }

            string mongodbConnectionString = "mongodb://chaos:27017";

            //  Connect to our database
            var client = new MongoClient(mongodbConnectionString);

            // List of all known files that exist.
            var database = client.GetDatabase("BooksIndex");

            // Files that no longer exist go here...
            //var itemsGone = client.GetDatabase("BooksGone");

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
                        Console.WriteLine($"Volume add: {vi.Label} -> {vi.SerialNumber}");

                        volumesInDatabase.InsertOne(vi);
                    }
                    else
                    {
                        if (acountHits > 1)
                        {
                            Console.WriteLine($"{acountHits} hits on \"{vi.SerialNumber}");
                        }
                        else
                        {
                            if (vi.Label != athisList[0].Label)
                            {
                                Console.WriteLine($"{vi.SerialNumber} has \"{athisList[0].Label}\" now \"{vi.Label}\"");
                            }
                        }
                    }
                }

                allKnownVolumes = new(volumesInDatabase.AsQueryable().ToList());
            }

            // If we're also scanning files then start the scanning process.
            if (!doOnlyVolumes)
            {
                string whereToScan = pathRoots[0];

                // Get our list of files in this area.
                var files = new FindFiles(pathRoots.ToArray());

                string stamp = DateTime.Now.ToString();
                stamp = stamp.Replace("/", "-");
                stamp = stamp.Replace(":", "-");
                string whereHold = pathRoots[0] + " " + stamp;

                Console.WriteLine($"Found {files.Count} files");

                var collection = database.GetCollection<FileHashInformation>("HashIndexOut");
                var collectionHistory = database.GetCollection<FileHashInformation>("HashIndexHistory");
                IMongoCollection<FileHashInformation> checkCollection = null; // database.GetCollection<FileHashInformation>("HashIndexCheck");

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

                if (files.Count > 0)
                {
                    string vsn = liveVolumes.ByPath(whereToScan).SerialNumber;
                    var filter1 = Builders<FileHashInformation>.Filter.Eq("volumeid", vsn);
                    var hits = collection.Find(filter1);
                    var countHits = hits.Count();
                    var thisList = hits.ToList();

                    long keepCount = 0;
                    long removedCount = 0;

                    foreach (var item in thisList)
                    {
                        string basePath = liveVolumes.RootByVsn(item.VolumeId);
                        string fullPath = basePath + item.BasePath;
                        if (File.Exists(fullPath))
                        {
                            keepCount += 1;
                            if (keepCount % 1000 == 0)
                            {
                                Console.WriteLine($"Processing: {keepCount}/{removedCount}");
                            }
                        }
                        else
                        {
                            removedCount += 1;
                            Console.WriteLine($"Removed: {keepCount}/{removedCount} - \"{fullPath}\"");

                            // Need to grab the ID of the item to remove here before we mess with it.
                            var removeFilter = Builders<FileHashInformation>.Filter.Eq("_id", item.Id);

                            // Write to database of removed files HashIndexHistory and remove from HashIndexOut.
                            try
                            {
                                collectionHistory.InsertOne(item);
                            }
                            catch (MongoDB.Driver.MongoWriteException myException)
                            {
                                var newItem = item.NewItem();
                                collectionHistory.InsertOne(newItem);
                            }

                            
                            var removeList = collection.Find(removeFilter);
                            var removeCount = removeList.Count();
                            var removeRecords = removeList.ToList();
                            collection.DeleteOne(removeFilter);
                        }
                    }
                    Console.WriteLine($"Keep: {keepCount}, Removed: {removedCount}");
                }

#if false
                // Index of file information from file path.
                Dictionary<string, FileHashInformation> infoIndex = new();
                {
                    var beginsTime = DateTime.Now;
                    foreach (var item in files)
                    {
                        var thisFile = new FileHashInformation(item, liveVolumes);
                        infoIndex[thisFile.BasePath] = thisFile;
                    }

                    var endTime = DateTime.Now;
                    Console.WriteLine($"Detailed info done in: {endTime - beginsTime}");
                }
#endif

                Console.WriteLine($"Begin processing files for hashing...");
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

                                if (doMoveDuplicates)
                                {
                                    Console.WriteLine($"Keep: \"{index[thisFile.Sha256].Path}\"");
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
                                try
                                {
                                    thisFile.MakeHash();
                                    collection.InsertOne(thisFile);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Hash fail {ex.Message} in \"{thisFile.Path}\"");
                                }

                                if (checkCollection != null)
                                {
                                    checkCollection.InsertOne(thisFile);
                                }

                                Console.WriteLine($"\"{item}\" Done");

                                if (thisFile.Sha256 != null)
                                {
                                    // A file with this same contents is elsewhere in this area.
                                    if (index.ContainsKey(thisFile.Sha256))
                                    {
                                        if (doMoveDuplicates)
                                        {
                                            Console.WriteLine($"Keep: \"{index[thisFile.Sha256].Path}\"");
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
                            }

                            else
                            {
                                Console.WriteLine($"Big hash {thisFile.Path}");

                                try
                                {
                                    using (FileStream fs = File.OpenRead(thisFile.Path))
                                    {
                                        using var sha256 = SHA256.Create();
                                        {
                                            var hash = sha256.ComputeHash(fs);
                                            thisFile.AddHash(Convert.ToBase64String(hash));
                                            collection.InsertOne(thisFile);

                                            if (checkCollection != null)
                                            {
                                                checkCollection.InsertOne(thisFile);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to bighash {ex.Message} \"{thisFile.Path}\"");
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
