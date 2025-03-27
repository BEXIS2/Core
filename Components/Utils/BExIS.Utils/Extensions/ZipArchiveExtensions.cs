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
        public static void AddAllFilesFromDirectory(this ZipArchive archive, string folderPath, string zipPath = null)
        {
            var files = Directory.GetFiles(folderPath);

            foreach (var filePath in files)
            {
                // Get the file name (not the full path)
                string fileName = Path.GetFileName(filePath);

                var entry = !string.IsNullOrEmpty(zipPath) ? archive.CreateEntry($"{zipPath}{fileName}") : archive.CreateEntry($"{fileName}");              

                // Open the entry stream and copy the file content into it
                using (var entryStream = entry.Open())
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }

        public static void AddFile(this ZipArchive archive, string filePath, string zipPath = null)
        {
            // Open the file from disk
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                string fileName = Path.GetFileName(filePath);
                // Create an entry in the archive for the file
                var entry = !string.IsNullOrEmpty(zipPath) ? archive.CreateEntry($"{zipPath}{fileName}") : archive.CreateEntry($"{fileName}");

                // Open the entry stream and copy the file content into it
                using (var entryStream = entry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }

        public static void AddFolderToArchive(this ZipArchive archive, string folderPath, string entryFolderName)
        {
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                foreach (var filePath in files)
                {
                    var relativePath = Path.Combine(entryFolderName, Path.GetFileName(filePath));
                    AddFileToArchive(archive, filePath, relativePath);
                }
            }
        }

        public static void AddFileToArchive(this ZipArchive archive, string filePath, string entryName)
        {
            if (File.Exists(filePath))
            {
                var fileEntry = archive.CreateEntry(entryName);
                using (var entryStream = fileEntry.Open())
                using (var fileStream = File.OpenRead(filePath))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}
