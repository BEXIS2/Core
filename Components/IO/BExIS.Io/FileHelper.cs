using System;
using System.IO;

namespace BExIS.IO
{
    public class FileHelper
    {
        public static bool FileExist(string path)
        {
            if (File.Exists(path))
                return true;
            else
                return false;
        }

        public static bool MoveFile(string tempFile, string destinationPath)
        {
            if (File.Exists(tempFile))
            {
                File.Move(tempFile, destinationPath);

                if (File.Exists(destinationPath))
                {
                    return true;
                }
                else return false;
            }
            else return false;

        }

        public static FileStream Create(string filepath)
        {

            string path = Path.GetDirectoryName(filepath);
            // if folder not exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!File.Exists(filepath))
            {
                return File.Create(filepath);
            }

            return null;
        }

        public static void CreateDicrectoriesIfNotExist(string directoryPath)
        {
            // if folder not exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static bool Delete(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);

                if (!File.Exists(file)) return true;

            }

            return false;
        }

        public static bool WaitForFile(string fullPath)
        {


            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        // If we got this far the file is ready
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (numTries > 10)
                    {
                        return false;
                    }

                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(300);
                    Console.WriteLine("Waiting : " + ex.Message);
                }
            }

            return true;
        }
    }
}
