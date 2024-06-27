using BExIS.UI.Hooks.Caches;
using System.Collections.Generic;

namespace BExIS.UI.Models
{
    public class FileUploader
    {
        /// <summary>
        /// based on structured or unstructured Datasets the list of supported filetypes is defined by tenants
        /// </summary>
        public List<string> Accept { get; set; }

        /// <summary>
        /// allready uploaded files in the temp folder
        /// </summary>
        public List<FileInfo> ExistingFiles { get; set; }

        /// <summary>
        /// setup description to each file
        /// </summary>
        public DescriptionType DescriptionType { get; set; }

        /// <summary>
        /// if true, multiple files can be selected at once
        /// </summary>
        public bool Multiple { get; set; }

        /// <summary>
        /// max size of the file in ???
        /// </summary>
        public int MaxSize { get; set; }

        public FileUploader()
        {
            Accept = new List<string>();
            ExistingFiles = new List<FileInfo>();
            DescriptionType = DescriptionType.none;
            Multiple = true;
        }
    }

    public enum DescriptionType
    {
        none, // no description per file
        active, // add desctipion to each file
        required // add description text to each file is required
    }
}