using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace C9FIleWatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Object myLock = new Object();

        // List of folders that we're pushing content to...
        private Dictionary<string, string> ToList = new Dictionary<string, string>();

        // List of folders that we're pulling content from...
        private Dictionary<string, string> FromList = new Dictionary<string, string>();

        public bool AddFrom(string path)
        {
            bool newItem = false;
            lock (myLock)
            {
                newItem = !FromList.ContainsKey(path);
                FromList[path] = path;
            }
            return newItem;
        }

        public bool AddTo(string path)
        {
            bool alreadyPresent = false;
            lock (myLock)
            {
                alreadyPresent = !ToList.ContainsKey(path);
                ToList[path] = path;
            }
            return alreadyPresent;
        }
    }
}
