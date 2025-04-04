using BExIS.IO.Transform.Input;
using BExIS.UI.Hooks.Caches;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class SubmitModel
    {
        // subject
        public long Id { get; set; }

        public string Title { get; set; }

        // infos about data
        /// <summary>
        /// if true validation runs without errors
        /// </summary>
        public bool IsDataValid { get; set; }

        // infos about files
        public bool AllFilesReadable { get; set; }

        public List<FileInfo> Files { get; set; }
        public List<FileInfo> DeleteFiles { get; set; }
        public List<FileInfo> ModifiedFiles { get; set; }

        public AsciiFileReaderInfo AsciiFileReaderInfo { get; set; }

        //infos about structre
        public bool HasStructrue { get; set; }

        public long StructureId { get; set; }
        public string StructureTitle { get; set; }

        public bool AsyncUpload { get; set; }
        public string AsyncUploadMessage { get; set; }

        public SubmitModel()
        {
            Id = 0;
            Title = string.Empty;
            IsDataValid = false;
            AllFilesReadable = false;
            Files = new List<FileInfo>();
            AsciiFileReaderInfo = new AsciiFileReaderInfo();
            StructureId = 0;
            StructureTitle = String.Empty;
        }
    }

    public class SubmitResponce
    {
        public bool Success { get; set; }
        public bool AsyncUpload { get; set; }
        public string AsyncUploadMessage { get; set; }
        public List<SortedError> Errors { get; set; }

        public SubmitResponce()
        {
            AsyncUpload = false;
            Errors = new List<SortedError>();
            AsyncUploadMessage = String.Empty;
        }
    }
}