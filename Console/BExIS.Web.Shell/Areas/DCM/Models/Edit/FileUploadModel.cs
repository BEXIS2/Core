using BExIS.UI.Hooks.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class FileUploadModel
    {
        /// <summary>
        /// Context is , Data Upload or Attachments
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// based on structured or unstructured Datasets the list of supported filetypes is defined by tenants
        /// </summary>
        public List<string> AcceptedExtentions { get; set; }

        /// <summary>
        /// allready uploaded files in the temp folder
        /// </summary>
        public List<FileInfo> ExistingFiles { get; set; }

        public FileUploadModel()
        {
            AcceptedExtentions = new List<string>();
            ExistingFiles = new List<FileInfo>();
        }
    }
}