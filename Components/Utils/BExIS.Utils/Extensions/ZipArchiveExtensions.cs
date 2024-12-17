using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Extensions
{
    public static class ZipArchiveExtensions
    {
        public static void AddAllFilesFromDirectory(this ZipArchive archive, string folderPath)
        {
            var files = Directory.GetFiles(folderPath);

            foreach (var filePath in files)
            {
                // Get the file name (not the full path)
                string fileName = Path.GetFileName(filePath);

                // Create a new entry for the file in the ZIP archive
                var entry = archive.CreateEntry(fileName);

                // Open the entry stream and copy the file content into it
                using (var entryStream = entry.Open())
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }

        public static void AddFile(this ZipArchive archive, string filePath)
        {
            // Open the file from disk
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                string fileName = Path.GetFileName(filePath);
                // Create an entry in the archive for the file
                var entry = archive.CreateEntry(fileName);

                // Open the entry stream and copy the file content into it
                using (var entryStream = entry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}
