using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C9Native;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Serializers;

namespace FindHashDuplicates
{
    /// <summary>
    /// Create a list of known volume information for this system in a form suitable for serialization to MongoDb
    /// </summary>
    public class VolumeInformationStore : IEnumerable<VolumeInformationItem>
    {
        // Iterable list of items...usable for bulk serialization.
        private List<VolumeInformationItem> _items = new List<VolumeInformationItem>();

        // Look up a volume by its vsn.
        private Dictionary<string, VolumeInformationItem> _byvsn = new Dictionary<string, VolumeInformationItem>();
        
        private Dictionary<string, VolumeInformationItem> _bypath = new Dictionary<string, VolumeInformationItem>();

        public
        VolumeInformationStore()
        {
            Volumes data = new Volumes();

            foreach (var item in data)
            {
                if (item.Information.Label != null && 
                    item.Information.Label != "EFI" && 
                    item.Information.Label != "System Reserved")
                {
                    var storeas = new VolumeInformationItem(item);

                    _items.Add(storeas);
                    _byvsn[item.Information.VolumeSerial] = storeas;
                    foreach (var ii in item.Names)
                    {
                        _bypath[ii] = storeas;
                    }
                }
            }
        }

        /// <summary>
        /// Construct a container from the provided list.
        /// </summary>
        /// <param name="vi"></param>
        public
            VolumeInformationStore(IEnumerable<VolumeInformationItem> vi)
        {
            foreach (var item in vi)
            {
                // Insert into our main list...
                _items.Add(item);

                _byvsn[item.SerialNumber] = item;

                // ...and add to our index by VSN.
                foreach (var ii in item.Paths)
                {
                    _bypath[ii] = item;
                }
            }
        }

        /// <summary>
        /// Retrieve the volume information that corresponds to this VSN
        /// </summary>
        /// <param name="whichone">String-ized volume serial number we're looking for.</param>
        /// <returns>Volume information item.</returns>
        public VolumeInformationItem ByVsn(string whichone)
        {
            return (_byvsn[whichone]);
        }

        public string RootByVsn(string whichone)
        {
            return (_byvsn[whichone].Paths[0]);
        }


        public string RootByPath(string where)
        {
            string matchPath = null;

            foreach (var path in _bypath.Keys)
            {
                var length = path.Length;
                if (path.Length <= where.Length && 
                    path.ToLower() == where.Substring(0, path.Length).ToLower())
                {
                    // If we don't yet have a match then save this one.
                    if (matchPath == null)
                    {
                        matchPath = path;
                    }

                    // We're looking for the longest match as that is the most specific.
                    else if (path.Length > matchPath.Length)
                    {
                        matchPath = path;
                    }
                }
            }

            return matchPath;
        }

        /// <summary>
        /// Given a file path, look up the volume that corresponds to that path (or null if no match).
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public VolumeInformationItem ByPath(string where)
        {
            string volumeWhere = RootByPath(where);
            if (volumeWhere != null)
            {
                return _bypath[volumeWhere];
            }
            else
            {
                    return null;
            }
        }

        /// <summary>
        /// Check to see if the vsn is known in this collection.
        /// </summary>
        /// <param name="whichone">Volume serial number that is of interest.</param>
        /// <returns>true if there is a volume with this VSN stored here.</returns>
        public bool Exists(string whichone)
        {
            return (_byvsn.ContainsKey(whichone));
        }

        public IEnumerator<VolumeInformationItem> GetEnumerator()
        {
            return ((IEnumerable<VolumeInformationItem>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }
}
