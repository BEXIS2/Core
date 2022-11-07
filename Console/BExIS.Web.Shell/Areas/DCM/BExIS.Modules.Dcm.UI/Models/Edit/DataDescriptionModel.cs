﻿using BExIS.UI.Hooks.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class DataDescriptionModel
    {
        public long Id { get; set; }
        public long StructureId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? LastModification { get; set; }

        public List<VariableModel> Variables { get; set; }

        // this value is a flag to check wheter all files from upload is readable or not
        // if not the ui should hide data description at all
        public bool AllFilesReadable { get; set; }

        /// <summary>
        /// allready uploaded files in the temp folder
        /// </summary>
        public List<FileInfo> ReadableFiles { get; set; }
        public DataDescriptionModel()
        {
            Title = "";
            Description = "";
            StructureId = 0;

            ReadableFiles = new List<FileInfo>();
            Variables = new List<VariableModel>();

        }

    }

    public class VariableModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }

    }
}