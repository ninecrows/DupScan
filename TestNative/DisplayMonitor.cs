using System;

namespace C9Native
{
    /// <summary>
    /// Store the information we know about a single display.
    /// </summary>
    public class DisplayMonitor
    {
        /// <summary>
        /// Construct an object to store the information related to a display.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="aRectangle"></param>
        /// <param name="aWorkArea"></param>
        /// <param name="aPrimary"></param>
        /// <param name="aName"></param>
        public DisplayMonitor(IntPtr handle, DisplayRectangle aRectangle, DisplayRectangle aWorkArea, bool aPrimary, string aName)
        {
            // Grab the monitor handle...these don't seem to need to be closed.
            MonitorHandle = handle;

            Display = aRectangle;

            Working = aWorkArea;

            Primary = aPrimary;

            Name = aName;
        }

        /// <summary>
        /// Handle to the native monitor object.
        /// </summary>
        public IntPtr MonitorHandle { get; }

        /// <summary>
        /// Retrieve the boundaries of this monitor.
        /// </summary>
        public DisplayRectangle Display { get; }

        /// <summary>
        /// Working area of the display.
        /// </summary>
        public DisplayRectangle Working { get; }

        /// <summary>
        /// If true then this is the primary display.
        /// </summary>
        public bool Primary { get; }

        /// <summary>
        /// Name of the monitor.
        /// </summary>
        public string Name { get; }
    }
}
