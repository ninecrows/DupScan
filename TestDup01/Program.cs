using System;
using System.IO;
using C9FileHelpers;
using C9Native;

namespace TestDup01
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Get our list of volumes on the current machine.
            var volumes = new Volumes();

            var whereAreWe = Directory.GetCurrentDirectory();
            var files = new FindFiles(whereAreWe);

            foreach (var file in files)
            {
                var hashes = new FileHashes(file);
                var fileInfo = new FileInfo(file);
                var fileId = new FileInformationFileId(file);
                var moreInfo = new FileInformation(file);
                var pathInfoExtension = Path.GetExtension(file);
                var pathBaseName = Path.GetFileNameWithoutExtension(file);
                var pathDirectoryName = Path.GetDirectoryName(file);
                var pathFileName = Path.GetFileName(file);
                var pathFullPath = Path.GetFullPath(file);

                // Peel apart windows specific path to get drive letter and path elements.
                var frags = pathDirectoryName.Split(":");
                var ofrags = pathDirectoryName.Split("\\");
            }
        }
    }
}
