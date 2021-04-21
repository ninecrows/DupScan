using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using C9Native;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace FindHashDuplicates
{
    [BsonIgnoreExtraElements]
    [JsonObject(MemberSerialization.OptIn)]
    public class VolumeInformationItem
    {
        [BsonId] private ObjectId _id;

        [JsonProperty]
        [BsonElement("label")]
        public string Label { get; set;  }

        [JsonProperty]
        [BsonElement("serialnumber")]
        public string SerialNumber { get; set;  }

        [JsonProperty]
        [BsonElement("filesystem")]
        public string FileSystem { get; set;  }

        [JsonProperty]
        [BsonElement("type")]
        public string Type { get; set;  }

        [JsonProperty]
        [BsonElement("paths")]
        public string[] Paths { get; set;  }

        [JsonProperty]
        [BsonElement("freespace")]
        public ulong FreeSpace { get; set;  }

        [JsonProperty]
        [BsonElement("totalspace")]
        public ulong TotalSpace { get; set;  }

        [JsonProperty]
        [BsonElement("features")]
        public int Features { get; set;  }

        [JsonProperty]
        [BsonElement("seen")]
        public DateTime Seen { get; set;  }

        public string Describe()
        {
            string result = $"\"{Label}\" vsn:{SerialNumber} fs:{FileSystem} t:{Type} t:{TotalSpace}\n{Seen}\n";
            foreach (var name in Paths)
            {
                result += $"\t{name}\n";
            }

            return result;
        }

        /// <summary>
        /// Load this object up from information provided by the Volume object we've been provided.
        /// </summary>
        /// <param name="volume"></param>
        public VolumeInformationItem(Volume volume)
        {
            Label = volume.Information.Label;
            SerialNumber = volume.Information.VolumeSerial;
            FileSystem = volume.Information.FileSystem;
            Type = volume.Type.TypeName;

            // Grab all of the path roots for this volume.
            // I'm sure there's a better way to do this but for now...
            Paths = new string[volume.Names.Count];
            {
                int index = 0;
                foreach (var path in volume.Names)
                {
                    Paths[index++] = path;
                }
            }

            FreeSpace = volume.Space.FreeBytes;
            TotalSpace = volume.Space.TotalBytes;

            Features = (int)volume.Information.Features;

            // Record when we saw this.
            Seen = DateTime.UtcNow;
        }

        public VolumeInformationItem()
        {
        }
    }
}
