using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace C9Native
{
    /// <summary>
    /// RAII get information on all display monitors that this system knows about.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class DisplayMonitors
    {
        // Store list of monitors we have information on.
        private readonly List<DisplayMonitor> _monitors = new();

        /// <summary>
        /// Retrieve the information about one of the monitors we know about.
        /// </summary>
        /// <param name="index">Which item do we want information on.</param>
        /// <returns>Immutable block of information on this monitor.</returns>
        public DisplayMonitor this[int index] => _monitors[index];

        /// <summary>
        /// Return the number of monitors that we have information on.
        /// </summary>
        public int Count => _monitors.Count;

        /// <summary>
        /// RAII enumerate available monitors on this system and make their information available. 
        /// </summary>
        public DisplayMonitors()
        {
            // ReSharper disable once UnusedVariable
            var result = EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                // ReSharper disable once IdentifierTypo
                delegate(IntPtr hMonitor, IntPtr _, ref Rect _, IntPtr _)
                {
                    MonitorInfoEx informationRaw = new MonitorInfoEx();
                    informationRaw.Size = Marshal.SizeOf(informationRaw);
                    bool success = GetMonitorInfo(hMonitor, ref informationRaw);
                    if (success)
                    {
                        var screen = new DisplayRectangle(informationRaw.Monitor.left, 
                            informationRaw.Monitor.top,
                            informationRaw.Monitor.right,
                            informationRaw.Monitor.bottom);

                        var working = new DisplayRectangle(informationRaw.Work.left,
                            informationRaw.Work.top,
                            informationRaw.Work.right,
                            informationRaw.Work.bottom);

                        // Check the primary monitor flag.
                        bool primary = (informationRaw.Flags & MonitorInformationPrimary) != 0;

                        // Assemble the immutable monitor information that we'll provide to external clients.
                        var information = new DisplayMonitor(hMonitor, screen, working, primary, informationRaw.DeviceName);

                        _monitors.Add(information);
                    }

                    return true;
                },IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        // ReSharper disable once IdentifierTypo
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        // ReSharper disable once IdentifierTypo
        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumDelegate enumerationDelegate, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Rect
        {
            public readonly int left;
            public readonly int top;
            public readonly int right;
            public readonly int bottom;
        }

        // size of a device name string
        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        private const int CCHDEVICENAME = 32;

        // Bit mask for MonitorInfEx flags indicating this monitor is the primary monitor.
        private const int MonitorInformationPrimary = 0x1;

        /// <summary>
        /// The MONITORINFOEX structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
        /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name 
        /// for the display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [SuppressMessage("ReSharper", "CommentTypo")]
        private struct MonitorInfoEx
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function. 
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public readonly Rect Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications, 
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor. 
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public readonly Rect Work;

            /// <summary>
            /// The attributes of the display monitor.
            /// 
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
            public readonly uint Flags;

            /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name, 
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;

            // ReSharper disable once UnusedMember.Global
            // ReSharper disable once UnusedMember.Local
            public void Init()
            {
                this.Size = 40 + 2 * CCHDEVICENAME;
                this.DeviceName = string.Empty;
            }
        }
    }
}
