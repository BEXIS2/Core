using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Vaiona.Utils.IO
{
    public class FileHelper
    {
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private static bool IsFileLocked(Exception exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        // wait until the catalog file is released. It may need a timer to avoid deadlocks
        public static void WaitForFile(string filePath)
        {
            while (!CanReadFile(filePath)) ;
        }

        public static bool CanReadFile(string filePath)
        {
            //Try-Catch so we dont crash the program and can check the exception
            try
            {
                //The "using" is important because FileStream implements IDisposable and
                //"using" will avoid a heap exhaustion situation when too many handles
                //are left undisposed.
                using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    if (fileStream != null) fileStream.Close();  //This line is me being overly cautious, fileStream will never be null unless an exception occurs... and I know the "using" does it but its helpful to be explicit - especially when we encounter errors - at least for me anyway!
                }
            }
            catch (IOException ex)
            {
                //THE FUNKY MAGIC - TO SEE IF THIS FILE REALLY IS LOCKED!!!
                if (IsFileLocked(ex))
                {
                    // do something, eg File.Copy or present the user with a MsgBox - I do not recommend Killing the process that is locking the file
                    return false;
                }
            }
            finally
            { }
            return true;
        }

        public static void MoveAndReplace(string sourceDirectoty, string targetDirectory)
        {
            var sourcePath = sourceDirectoty.TrimEnd('\\', ' ');
            var targetPath = targetDirectory.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (System.IO.File.Exists(targetFile))
                        System.IO.File.Delete(targetFile);
                    System.IO.File.Move(file, targetFile);
                }
            }
            Directory.Delete(sourceDirectoty, true);
        }
    }
}