using BExIS.Security.Entities.Authorization;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Extensions
{
    public static class ZipArchiveExtensions
    {
        public static void AddAllFilesFromDirectory(this ZipOutputStream zipStream, string folderPath, string zipPath = null)
        {
            var files = Directory.GetFiles(folderPath);

            foreach (var filePath in files)
            {
                // Get the file name (not the full path)
                string fileName = Path.GetFileName(filePath);
                string entryName = !string.IsNullOrEmpty(zipPath) ? $"{zipPath}{fileName}" : fileName;

                var entry = new ZipEntry(entryName);
                entry.DateTime = File.GetLastWriteTime(filePath);
                entry.Size = new FileInfo(filePath).Length;

                zipStream.PutNextEntry(entry);

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(zipStream);
                }

                zipStream.CloseEntry();
            }
        }

        public static void AddFile(this ZipOutputStream zipStream, string filePath, string zipPath = null)
        {
            if (!File.Exists(filePath))
                return;

            string fileName = Path.GetFileName(filePath);
            string entryName = !string.IsNullOrEmpty(zipPath) ? $"{zipPath}{fileName}" : fileName;

            var entry = new ZipEntry(entryName);
            entry.DateTime = File.GetLastWriteTime(filePath);
            entry.Size = new FileInfo(filePath).Length;

            zipStream.PutNextEntry(entry);

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(zipStream);
            }

            zipStream.CloseEntry();
        }

        public static void AddFolderToArchive(this ZipOutputStream zipStream, string folderPath, string entryFolderName)
        {
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                foreach (var filePath in files)
                {
                    var relativePath = Path.Combine(entryFolderName, Path.GetFileName(filePath)).Replace('\\', '/');
                    AddFileToArchive(zipStream, filePath, relativePath);
                }
            }
        }

        public static void AddFileToArchive(this ZipOutputStream zipStream, string filePath, string entryName)
        {
            if (File.Exists(filePath))
            {
                // Normalize path separators for ZIP format
                entryName = entryName.Replace('\\', '/');

                var entry = new ZipEntry(entryName);
                entry.DateTime = File.GetLastWriteTime(filePath);
                entry.Size = new FileInfo(filePath).Length;

                zipStream.PutNextEntry(entry);

                using (var fileStream = File.OpenRead(filePath))
                {
                    fileStream.CopyTo(zipStream);
                }

                zipStream.CloseEntry();
            }
        }
    }
}
