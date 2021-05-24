using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.IdGenerators;

namespace C9Native
{
    /// <summary>
    /// Construct from a privilege name or LUID and provide all information about it.
    /// </summary>
    public
    class PrivilegeInformation
    {
        private LUID _luid;
        /// <summary>
        /// Return the LUID that identifies this privilege.
        /// </summary>
        public LUID luid => _luid;

        private string _name;
        /// <summary>
        /// Return the formal name of this privilege.
        /// </summary>
        public string name => _name;

        private string _display;
        /// <summary>
        /// Return the human readable description of this privilege.
        /// </summary>
        public string display => _display;

        /// <summary>
        /// Given the name of a privilege, construct an object with all information we might need about it.
        /// </summary>
        /// <param name="privilegename">Name of the privilege.</param>
        public
            PrivilegeInformation(string privilegename)
        {
            _name = privilegename;

            _luid = NameToLuid(privilegename);

            _display = NameToDisplayName(privilegename);
        }

        /// <summary>
        /// Given the LUID for a privilege construct an object with all the information we might need.
        /// </summary>
        /// <param name="luid">LUID for the privilege we're interested in.</param>
        public
            PrivilegeInformation(LUID luid)
        {
            _luid = luid;

            _name = LuidToName(luid);

            if (_name != null)
            {
                _display = NameToDisplayName(_name);
            }
        }

        /// <summary>
        /// Return a full human readable description of this privilege.
        /// </summary>
        /// <returns></returns>
        public string Describe()
        {
            return $"{_name} -> {_display} ({_luid.HighPart}/{_luid.LowPart})";
        }

        public static 
            LUID NameToLuid(string name)
        {
            var result = new LUID();

            [DllImport("advapi32.dll")]
            static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
                ref LUID lpLuid);

            bool getok = LookupPrivilegeValue(null, name, ref result);
            if (!getok)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// Given a privilege name, return the human readable display name for that privilege.
        /// </summary>
        /// <param name="name">Name of the privilege we're interested in.</param>
        /// <returns>Human readable string that describes the provided privilege name.</returns>
        public static 
            string NameToDisplayName(string name)
        {
            string result = null;

            [DllImport("advapi32.dll", SetLastError = true)]
            static extern bool LookupPrivilegeDisplayName(
                string systemName,
                string privilegeName, //in
                System.Text.StringBuilder displayName,  // out
                ref uint cbDisplayName,
                out uint languageId
            );

            uint length = 0;
            uint language;
            bool lenok = LookupPrivilegeDisplayName(null, 
                name, 
                null, ref length, 
                out language);

            if (length > 0)
            {
                var builder = new StringBuilder();
                builder.EnsureCapacity((int)length);

                bool getok = LookupPrivilegeDisplayName(null,
                    name,
                    builder, ref length,
                    out language);

                if (getok)
                {
                    result = builder.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// Given an LUID return the string name for that LUID.
        /// </summary>
        /// <param name="luid">LUID that we want the name of.</param>
        /// <returns>Name of the LUID provided as a privilege.</returns>
        public static 
            string LuidToName(LUID luid)
        {
            string result = null;

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool LookupPrivilegeName(
                string lpSystemName,
                IntPtr lpLuid,
                System.Text.StringBuilder lpName,
                ref int cchName);

            IntPtr ptrLuid = Marshal.AllocHGlobal(Marshal.SizeOf(luid));
            Marshal.StructureToPtr(luid, ptrLuid, true);

            try
            {
                int namelength = 0;
                bool lenok = LookupPrivilegeName(null, ptrLuid, null, ref namelength);

                if (namelength > 0)
                {
                    var builder = new StringBuilder();
                    builder.EnsureCapacity(namelength + 1);

                    bool getok = LookupPrivilegeName(null, ptrLuid, builder, ref namelength);

                    if (getok)
                    {
                        result = builder.ToString();
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrLuid);
            }

            return result;
        }
    }
}
