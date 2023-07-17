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
            try
            {

                if (File.Exists(tempFile))
                {
                    // check if directoy exist otherwhise create
                    string directory = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Move(tempFile, destinationPath);

                    if (File.Exists(destinationPath))
                    {
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
            catch
            {
                return false;
            }
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

        public static bool WaitForFile(string fullPath, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, fileAccess,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        // If we got this far the file is ready
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (numTries > 10000)
                    {
                        return false;
                    }

                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(10);
                    //Console.WriteLine("Waiting : " + ex.Message);
                }
            }

            return true;
        }

        /// <summary>
        /// remove all files and folders from the given folder
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static bool ClearFolder(string fullPath)
        {
            // https://stackoverflow.com/a/1288747/1169798

            // get file/folder listing
            System.IO.DirectoryInfo di = new DirectoryInfo(fullPath);

            // delete all files
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // delete all folders (including their content)
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                Directory.Delete(dir.FullName, true);
            }

            return true;
        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
    }
}