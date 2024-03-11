using BExIS.UI.Hooks.Caches;
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

        /// <summary>
        /// if true you should be aböle to select a structre frim the system or import from file
        /// </summary>
        public bool IsStructured { get; set; }

        /// <summary>
        /// choose only from predefiend structres
        /// </summary>
        public bool IsRestricted { get; set; }

        /// <summary>
        /// data in combination with this structure exist
        /// </summary>
        public bool HasData { get; set; }

        /// <summary>
        /// current user has editright
        /// </summary>
        public bool EnableEdit { get; set; }


        public DataDescriptionModel()
        {
            Title = "";
            Description = "";
            StructureId = 0;

            ReadableFiles = new List<FileInfo>();
            Variables = new List<VariableModel>();

            IsStructured = false;
            IsRestricted = false;
            HasData = false;
            EnableEdit = false;

        }

    }

    public class VariableModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }
        public bool IsKeys { get; set; }

    }
}