using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C9Native;
using JetBrains.Annotations;

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
