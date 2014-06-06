using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BExIS.Io
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
    }
}
