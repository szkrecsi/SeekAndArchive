using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SeekAndArchive
{
    class Program
    {
        private static List<FileInfo> FoundFiles;
        // 2nd part
        private static List<FileSystemWatcher> watchers;
        // 3rd part
        private static List<DirectoryInfo> archiveDirs;

        static void Main(string[] args)
        {
            string fileName = args[0];
            string directoryName = args[1];
            FoundFiles = new List<FileInfo>();
            // 2nd part
            watchers = new List<FileSystemWatcher>();

            //examine if the given directory exists at all 
            DirectoryInfo rootDir = new DirectoryInfo(directoryName);
            if (!rootDir.Exists)
            {
                Console.WriteLine("The specified directory does not exist.");
                return;
            }

            //search recursively for the mathing files
            RecursiveSearch(FoundFiles, fileName, rootDir);

            // 3rd part
            archiveDirs = new List<DirectoryInfo>();
            //create archive directories
            for (int i = 0; i < FoundFiles.Count; i++)
            {
                archiveDirs.Add(Directory.CreateDirectory("archive" + i.ToString()));
            }

            //list the found files
            Console.WriteLine("Found {0} files.", FoundFiles.Count);
            foreach (FileInfo file in FoundFiles)
            {
                Console.WriteLine("{0}", file.FullName);
            }
            foreach (FileInfo file in FoundFiles)
            {
                FileSystemWatcher newWatcher = new FileSystemWatcher(file.DirectoryName, file.Name);
                newWatcher.Changed += new FileSystemEventHandler(WatcherChanged);
                newWatcher.EnableRaisingEvents = true;
                watchers.Add(newWatcher);
            }

            Console.ReadKey();
        }

        // 2nd part
        static void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Console.WriteLine("{0} has been changed!", e.FullPath);
                //find the the index of the changed file 
                FileSystemWatcher senderWatcher = (FileSystemWatcher)sender;
                int index = watchers.IndexOf(senderWatcher, 0);
                //now that we have the index, we can archive the file 
                ArchiveFile(archiveDirs[index], FoundFiles[index]);
            }
        }

        static void RecursiveSearch(List<FileInfo> foundFiles, string fileName, DirectoryInfo currentDirectory)
        {
            foreach (FileInfo file in currentDirectory.GetFiles())
            {
                if (file.Name == fileName)
                    foundFiles.Add(file);
            }

            //continue the search recursively
            foreach (DirectoryInfo dir in currentDirectory.GetDirectories())
            {
                RecursiveSearch(foundFiles, fileName, dir);
            }
        }
        
        static void ArchiveFile(DirectoryInfo archiveDir, FileInfo fileToArchive)
        {
            FileStream input = fileToArchive.OpenRead();
            FileStream output = File.Create(archiveDir.FullName + @"\" + fileToArchive.Name + ".gz");
            GZipStream Compressor = new GZipStream(output, CompressionMode.Compress);
            int b = input.ReadByte();
            while (b != -1)
            {
                Compressor.WriteByte((byte)b);

                b = input.ReadByte();
            }
            Compressor.Close();
            input.Close();
            output.Close();
        }
    }
}
