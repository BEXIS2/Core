using BExIS.IO.Transform.Input;
using BExIS.UI.Models;
using System;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class FileUploadModel
    {
        public FileUploader FileUploader { get; set; }

        // this value is a flag to check wheter all files from upload is readable or not
        // if not the ui should hide data description at all
        public bool AllFilesReadable { get; set; }
        public bool HasStructure { get; set; }

        public AsciiFileReaderInfo AsciiFileReaderInfo { get; set; }
        public DateTime? LastModification { get; set; }

        public FileUploadModel()
        {
            FileUploader = new FileUploader();
            AllFilesReadable = false;
            HasStructure = false;
            AsciiFileReaderInfo = null;
        }
    }
}