using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using C9Native;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Exception = System.Exception;
using TimeSpan = System.TimeSpan;

namespace FindHashDuplicates
{
    [BsonIgnoreExtraElements]
    [JsonObject(MemberSerialization.OptIn)]
    class FileHashInformation
    {
        [BsonId] private ObjectId _id;
        public ObjectId Id { get; }

        [JsonProperty] [BsonElement("version")]
        private int _version = 1;

        [JsonProperty] [BsonElement("path")] 
        private string _path;

        public string Path => _path;

        // Path with volume root stripped.
        [JsonProperty] [BsonElement("basepath")]
        private string _basepath;

        public string BasePath => _basepath;

        // Unique file id for this file
        [JsonProperty] [BsonElement("fileid")] 
        private string _fileid;

        public string FileId => _fileid;

        // volume id for this file.
        [JsonProperty] [BsonElement("volumeid")]
        private string _volumeid;

        public string VolumeId => _volumeid;

        // Individual path components for searching.
        [JsonProperty] [BsonElement("pathcomponents")]
        private string[] _pathComponents;

        // File extension for searching.
        [JsonProperty] [BsonElement("extension")]
        private string _extension;

        [JsonProperty] [BsonElement("size")] private long _size;

        public long Size => _size;

        [JsonProperty] [BsonElement("created")]
        private DateTime _created;

        public DateTime Created => _created;

        [JsonProperty] [BsonElement("modified")]
        private DateTime _modified;

        public DateTime Modified => _modified;

        [JsonProperty] [BsonElement("sha256hash")]
        private string _sha256hash = null;

        public string Sha256 => _sha256hash;

        [JsonProperty] [BsonElement("seen")] private DateTime _seen;

        public DateTime Seen => _seen;

        private FileInfo _information;

        [JsonProperty][BsonElement("exists")]
        public bool Exists { get; set; } = false;

        // Status of access to this file. Null if we got what we needed.
        [JsonProperty] [BsonElement("status")] public string Status { get; set; } = null;

        public
        FileHashInformation(
            string path,
            VolumeInformationStore volumes
            )
        {
            _path = path;

            _information = new FileInfo(path);

            // If this file exists then we can harvest more information from it.
            if (_information.Exists)
            {
                Exists = true;

                // Update the seen flag.
                _seen = DateTime.UtcNow;

                _size = _information.Length;
                _created = _information.CreationTimeUtc;
                _modified = _information.LastWriteTimeUtc;

                // Get the native information related to this file.
                try
                {
                    var extra = new FileInformationFileId(path);
                    _fileid = extra.FullIdentifier;
                    _volumeid = extra.VolumeSerialNumber;


                    // This should work...something is pretty badly wrong if we don't find it as this is the list of all volumes here.
                    if (volumes.Exists(_volumeid))
                    {
                        var hmm = volumes.ByVsn(_volumeid);
                        var heads = hmm.Paths;
                        foreach (var head in heads)
                        {
                            if (head.ToLower() == path.Substring(0, head.Length).ToLower())
                            {
                                _basepath = path.Substring(head.Length);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Could not find {_volumeid}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed getting native info {ex.Message}");
                }

                // Grab file name and path parts to make searching easier.
                _extension = _information.Extension.ToLower();
                {
                    var pathOnly = _information.DirectoryName;
                    string[] fragments = pathOnly.Split("\\");
                    if (fragments[0][1] == ':')
                    {
                        fragments = fragments.Skip(1).ToArray();
                    }

                    _pathComponents = fragments;
                }
            }
        }

        public
            string
            MakeHash()
        {
            string base64hash = "";

            // Read in the whole file
            byte[] buffer = File.ReadAllBytes(_path);

            // Hash what we read.
            byte[] hash = null;
            using (var sha256 = SHA256.Create())
            {
               hash = sha256.ComputeHash(buffer);
            }

            // ...and to the final form as a string...
            base64hash = System.Convert.ToBase64String(hash);

            // And save it for later.
            _sha256hash = base64hash;

            return base64hash;
        }

        /// <summary>
        /// Try to put together a new, valid _path from the base path, the volume serial number and the volumes lookup.
        /// </summary>
        /// <param name="volumes">Volumes index to look up the vsn -> volume path.</param>
        /// <returns>null if no match or a valid path if we have a match.</returns>
        private string Pathify(VolumeInformationStore volumes)
        {
            if (_volumeid != null && volumes.Exists(_volumeid) && _basepath != null)
            {
                if (volumes.ByVsn(_volumeid).Paths.Length > 0)
                {
                    string root = volumes.ByVsn(_volumeid).Paths[0];
                    _path = root + _basepath;

                    return _path;
                }
            }

            return null;
        }

        // Epsilon for time comparisons.
        static TimeSpan mSec = new TimeSpan(0, 0, 0, 0, 1);

        public bool
            FixIfMissing(VolumeInformationStore volumes)
        {
            var changed = false;

            // Update the seen flag.
            _seen = DateTime.UtcNow;

            // {jkw} Temporary to load file ids and vsns where missing. Later this should be fixed.
            if (File.Exists(_path))
            {
                // Get the native information related to this file.
                try
                {
                    var extra = new FileInformationFileId(_path);
                    if (_fileid == null)
                    {
                        changed = true;
                        _fileid = extra.FullIdentifier;
                    }

                    if (_volumeid == null)
                    {
                        changed = true;
                        _volumeid = extra.VolumeSerialNumber;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed in fix if missing {ex.Message}");
                }
            }

            // Try to fix up the path information if the required volume is present.
            if (Pathify(volumes) != null || _path != null)
            {
                // Load the valid-ish path so we can process parts of it.
                _information = new FileInfo(_path);

                // If this file exists then we can extract information.
                if (File.Exists(_path))
                {
                    if (!Exists)
                    {
                        Exists = true;
                        changed = true;
                    }

                    if (_information.Length != _size)
                    {
                        changed = true;
                        _size = _information.Length;
                    }

                    // Check whether the creation time is different.
                    {
                        var interval = (_created - _information.CreationTimeUtc).Duration();
                        if (TimeSpan.Compare(interval, mSec) == 1)
                        {
                            changed = true;
                            _created = _information.CreationTimeUtc;
                        }
                    }

                    // Check whether the modified time is different.
                    {
                        var interval = (_modified - _information.LastWriteTimeUtc).Duration();
                        if (TimeSpan.Compare(interval, mSec) == 1)
                        {
                            changed = true;
                            _modified = _information.LastWriteTimeUtc;
                        }
                    }

                    try
                    {
                        // Get the native information related to this file.
                        var extra = new FileInformationFileId(_path);
                        if (_fileid == null)
                        {
                            changed = true;
                            _fileid = extra.FullIdentifier;
                        }

                        if (_volumeid == null)
                        {
                            changed = true;
                            _volumeid = extra.VolumeSerialNumber;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                    }

                    // This should work...something is pretty badly wrong if we don't find it as this is the list of all volumes here.
                    if (_basepath == null && _volumeid != null && volumes.Exists(_volumeid))
                    {
                        var hmm = volumes.ByVsn(_volumeid);
                        var heads = hmm.Paths;
                        foreach (var head in heads)
                        {
                            if (head.ToLower() == _path.Substring(0, head.Length).ToLower())
                            {
                                changed = true;
                                _basepath = _path.Substring(head.Length);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Could not find {_volumeid}");
                    }

                    // Grab file name and path parts to make searching easier.
                    if (_extension == null)
                    {
                        changed = true;
                        _extension = _information.Extension.ToLower();
                    }

                    if (_pathComponents == null)
                    {
                        var pathOnly = _information.DirectoryName;
                        string[] fragments = pathOnly.Split("\\");
                        if (fragments[0][1] == ':')
                        {
                            fragments = fragments.Skip(1).ToArray();
                        }

                        changed = true;
                        _pathComponents = fragments;
                    }

                    var testPathify = Pathify(volumes);

                    
                }
            }

            _version = 1;

            return changed;
        }

        /// <summary>
        /// Check whether this file exists now. 
        /// </summary>
        /// <returns>true if the file existence has changed, false otherwise</returns>
        public bool CheckExists()
        {
            _information = new FileInfo(_path);

            bool oldexists = Exists;

            // If this file exists then we can harvest more information from it.
            if (_information.Exists)
            {
                Exists = true;
            }

            return (oldexists != Exists);
        }

        public void AddHash(string myHash)
        {
            _sha256hash = myHash;
        }

        /// <summary>
        /// Return true if both objects look identical.
        /// </summary>
        /// <param name="information">Another information object that we're trying to compare.</param>
        /// <returns>true if the objects look alike.</returns>
        public bool IsMatch(FileHashInformation information)
        {
            bool result = false;

            var sizeMatch = (Size == information.Size);
            var tickDifference = Created - information.Created;
            var creationMatch = (tickDifference.Ticks < 10000);

            var modificationDifference = Modified - information.Modified;
            var modifiedMatch = (modificationDifference.Ticks < 10000);

            if (sizeMatch &&
                creationMatch &&
                modifiedMatch)
            {
                result = true;
            }

            return result;
        }
    }

}
