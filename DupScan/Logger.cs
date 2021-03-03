using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace C9DupScan
{
    class Logger : IDisposable
    {
        private StreamWriter logTo;

        public Logger(string prefix)
        {
            DateTime stamp = DateTime.Now;
            string description = stamp.ToUniversalTime().ToString("yyyyMMddTHHmmss.fffZ");
            string descriptionLocal = stamp.ToString("yyyyMMddTHHmmss.fffzz");
            long myTicks = stamp.Ticks;

            string logName = prefix + "-" + description + ".log";

            logTo = new StreamWriter(logName);
            logTo.AutoFlush = true;
        }

        public void Add(string message)
        {
            logTo.WriteLine(message);
        }

        public void Dispose()
        {
            if (logTo != null)
            {
                logTo.Dispose();
                logTo = null;
            }
        }
    }
}
