using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.AccessControl;

namespace ChannelSurfCli.Utils
{
    public class Files
    {
        public static string DecompressSlackArchiveFile(string zipFilePath, string tempPath)
        {

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
                Console.WriteLine("Deleting pre-existing temp directory");
            }

            Directory.CreateDirectory(tempPath);
            Console.WriteLine("Creating temp directory for Slack archive decompression");
            Console.WriteLine("Temp path is " + tempPath);
            using (ZipArchive archive  = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string pathToExtractTo = Path.Combine(tempPath, entry.FullName);
                    string directoryName = Path.GetDirectoryName(pathToExtractTo);
                    if (entry.Name == "") // This is a directory, not a file
                    {
                        Directory.CreateDirectory(directoryName);
                        continue;
                    }

                    // Sometimes there are files with the same name (our temp path is unique, so that that's not a reason for a file by the same name to already exist.)
                    // In our use case, these files were all identical, so overwriting is ok. I think the duplicates might be a bug with the tool I used to add attachments.
                    bool shouldOverwrite = false;
                    if (File.Exists(pathToExtractTo))
                    {
                        while (true) {
                            Console.WriteLine("Found two file attachments by the same name in the same directory. Overwrite the older one, keep the older one, or abort? (o|k|a):");
                            if (Console.ReadLine().StartsWith("o", StringComparison.CurrentCultureIgnoreCase)) shouldOverwrite = true;
                            else if (Console.ReadLine().StartsWith("k", StringComparison.CurrentCultureIgnoreCase)) shouldOverwrite = false;
                            else if (Console.ReadLine().StartsWith("a", StringComparison.CurrentCultureIgnoreCase)) Environment.Exit(0);
                            else continue;
                            break;
                        }
                    }

                    if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName); // Sometimes the higher level directories don't exist

                    entry.ExtractToFile(pathToExtractTo, shouldOverwrite);
                }
            }
            Console.WriteLine("Slack archive decompression done");

            return tempPath;
        }

        public static void CleanUpTempDirectoriesAndFiles(string tempPath)
        {
            Console.WriteLine("\n");
            Console.WriteLine("Cleaning up Slack archive temp directories and files");
            Directory.Delete(tempPath, true);
            File.Delete(tempPath);
            Console.WriteLine("Deleted " + tempPath + " and subdirectories");
        }
    }
}
