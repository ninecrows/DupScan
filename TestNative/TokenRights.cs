using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Events;

namespace C9Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;

        //SE_PRIVILEGE_ENABLED (1) or SE_PRIVILEGE_ENABLED_BY_DEFAULT (2)
        public UInt32 Attributes;
    }

    public class TokenRights
    {
        private IntPtr processhandle = IntPtr.Zero;

        private IntPtr tokenhandle = IntPtr.Zero;

        private LUID sebackupprivilege;

        private bool succeeded = true;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern
            bool
            OpenProcessToken(
                IntPtr ProcessHandle,
                UInt32 DesiredAccess,
                out IntPtr TokenHandle);

        private const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
        private const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const UInt32 TOKEN_DUPLICATE = 0x0002;
        private const UInt32 TOKEN_IMPERSONATE = 0x0004;
        private const UInt32 TOKEN_QUERY = 0x0008;
        private const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
        private const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
        private const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
        private const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        private const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                                                 TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY |
                                                 TOKEN_QUERY_SOURCE |
                                                 TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                                                 TOKEN_ADJUST_SESSIONID);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength,
            out uint ReturnLength);

        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
        }


        [DllImport("advapi32.dll")]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
            ref LUID lpLuid);

        public TokenRights()
        {
            processhandle = GetCurrentProcess();

            // If we successfully got the process handle then retrieve the process token.
            if (processhandle != IntPtr.Zero)
            {
                bool result = OpenProcessToken(processhandle, TOKEN_ALL_ACCESS, out tokenhandle);
                if (!result)
                {
                    succeeded = false;
                }
            }
            else
            {
                succeeded = false;
            }

            if (succeeded)
            {
                LUID backupluid = new LUID();
                bool result = LookupPrivilegeValue(null, "SeBackupPrivilege", ref backupluid);
                if (!result)
                {
                    succeeded = false;
                }
                else
                {
                    sebackupprivilege = backupluid;
                }
            }
        }

        struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges; // = new LUID_AND_ATTRIBUTES[10];
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeName(
            string lpSystemName,
            IntPtr lpLuid,
            System.Text.StringBuilder lpName,
            ref int cchName);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LookupPrivilegeDisplayName(
            string systemName,
            string privilegeName, //in
            System.Text.StringBuilder displayName, // out
            ref uint cbDisplayName,
            out uint languageId
        );

        public List<PrivilegeAndAttributes> GetTokenRightsList()
        {
            // Store token attributes here.
            List<PrivilegeAndAttributes> tokendata = new();

            uint TokenInfLength = 0;

            bool result = GetTokenInformation(tokenhandle,
                TOKEN_INFORMATION_CLASS.TokenPrivileges,
                IntPtr.Zero, TokenInfLength,
                out TokenInfLength);

            IntPtr information = Marshal.AllocHGlobal((int) TokenInfLength);

            result = GetTokenInformation(tokenhandle,
                TOKEN_INFORMATION_CLASS.TokenPrivileges,
                information, TokenInfLength,
                out TokenInfLength);

            if (result)
            {
                TOKEN_PRIVILEGES p = Marshal.PtrToStructure<TOKEN_PRIVILEGES>(information);
                int offset = Marshal.SizeOf(typeof(TOKEN_PRIVILEGES));

                if (p.PrivilegeCount > 0)
                {
                    tokendata.Add(new PrivilegeAndAttributes(p.Privileges[0]));
                }

                IntPtr nextitem = new IntPtr(information.ToInt64() + offset);
                int itemsize = Marshal.SizeOf(typeof(LUID_AND_ATTRIBUTES));
                for (int times = 1; times < p.PrivilegeCount; times++)
                {
                    // This should be the spot where the next item is (if any)
                    LUID_AND_ATTRIBUTES ll = Marshal.PtrToStructure<LUID_AND_ATTRIBUTES>(nextitem);

                    tokendata.Add(new PrivilegeAndAttributes(ll));

                    nextitem = new IntPtr(nextitem.ToInt64() + itemsize);
                }
            }

            Marshal.FreeHGlobal(information);

            return tokendata;
        }

        /// <summary>
        /// I*f the process has the privilege available but not enabled, enable it.
        /// </summary>
        /// <param name="name">Name of the privilege we want to enable.</param>
        /// <returns>true if the privilege is enabled after this call returns.</returns>
        public bool Activate(string name)
        {
            bool success = false;

            var list = GetTokenRightsList();


            PrivilegeAndAttributes ourentry = null;
            {
                foreach (var item in list)
                {
                    if (item.Name == name)
                    {
                        ourentry = item;
                    }
                }
            }

            if (ourentry != null)
            {
                var luid = ourentry.Luid;

                // Use this signature if you do not want the previous state
                [DllImport("advapi32.dll", SetLastError = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
                    [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
                    ref TOKEN_PRIVILEGES NewState,
                    UInt32 Zero,
                    IntPtr Null1,
                    IntPtr Null2);

                TOKEN_PRIVILEGES priv = new TOKEN_PRIVILEGES();
                priv.PrivilegeCount = 1;
                priv.Privileges = new LUID_AND_ATTRIBUTES[1];
                priv.Privileges[0].Attributes = 2;
                priv.Privileges[0].Luid = luid;
                bool ok = AdjustTokenPrivileges(tokenhandle, false, ref priv, 0, IntPtr.Zero, IntPtr.Zero);
                if (ok)
                {
                    success = true;
                }
            }

            return success;
        }
    }
}

