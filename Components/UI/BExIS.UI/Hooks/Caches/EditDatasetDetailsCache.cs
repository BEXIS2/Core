using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;

namespace BExIS.UI.Hooks.Caches
{
    public class EditDatasetDetailsCache
    {
        /// <summary>
        /// if true that means metadata of teh current edit version is valid
        /// </summary>
        public bool IsMetadataValid { get; set; }

        /// <summary>
        /// if true validation runs without errors
        /// </summary>
        public bool IsDataValid { get; set; }

        /// <summary>
        /// after a successful validation runs a hash will be genereated
        /// it combines the file-names, changedates and data description
        /// check the hash before you go on to be sure that nothings changed
        /// </summary>
        public byte[] ValidationHash { get; set; }

        /// <summary>
        /// contains all reader needed informations about the incoming file from type excel
        /// </summary>
        public ExcelFileReaderInfo ExcelFileReaderInfo { get; set; }

        /// <summary>
        /// contains all reader needed informations about the incoming file from type ascii
        /// </summary>
        public AsciiFileReaderInfo AsciiFileReaderInfo { get; set; }

        /// <summary>
        /// contains all filename that sould be imported
        /// </summary>
        public List<FileInfo> Files { get; set; }

        /// <summary>
        /// this dictionary store the last modifications of the changed hooks
        /// string = hook name 
        /// DateTime = Last Modification
        /// </summary>
        public Dictionary<string, DateTime> LastModifications { get; set; }

        /// <summary>
        /// Collect all informations to fine the data in the sheet
        /// </summary>
        public ExcelSetup ExcelSetup { get; set; }

        /// <summary>
        /// contains update informations like method, rows count, variable count
        /// </summary>
        public UpdateSetup UpdateSetup { get; set; }

        public EditDatasetDetailsCache()
        {
            Files = new List<FileInfo>();
        }

        /// <summary>
        /// when this function is called the LastModification Dicionary will be updated based on the type of a hook.
        /// the value, the date wiill be set to datetime now
        /// </summary>
        /// <param name="type"></param>
        public void UpdateLastModificarion(Type type)
        {
            //check if dictionary exist

            if (LastModifications == null) LastModifications = new Dictionary<string, DateTime>();

            string key = type.FullName;
            // if key exist update
            if (LastModifications.ContainsKey(key))
                LastModifications[key] = DateTime.Now;
            else // else create
                LastModifications.Add(key, DateTime.Now);
        }

        public DateTime? GetLastModificarion(Type type)
        {
            if (LastModifications != null && LastModifications.ContainsKey(type.FullName))
            {
                return LastModifications[type.FullName];
            }

            return null;
        }

    }

    public class ExcelSetup
    {
        /// <summary>
        /// the name of the current used sheet
        /// </summary>
        public string ActiveWooksheet { get; set; }

        /// <summary>
        /// defines the area in the sheet where the header is defined
        /// </summary>
        public string SheetHeaderArea { get; set; }

        /// <summary>
        /// defines the area in the sheet where the data is defined
        /// </summary>
        public string SheetDataArea { get; set; }

        /// <summary>
        /// ...
        /// </summary>
        public string SheetFormat { get; set; }
    }

    public class UpdateSetup
    {
        /// <summary>
        /// append means add rows
        /// update means update rows based on a primary key
        /// </summary>
        public UpdateMethod UpdateMethod { get; set; }

        public int RowsCount { get; set; }
        public int VariablesCount { get; set; }
        public int CurrentPackage { get; set; }
        public int CurrentPackageSize { get; set; }
    }

    public class FileInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Lenght { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// after a successful validation runs a hash will be genereated
        /// it combines the file-names, changedates and data description
        /// check the hash before you go on to be sure that nothings changed
        /// </summary>
        public string ValidationHash { get; set; }

        /// <summary>
        /// if a validation faild, then errors are stored here
        /// if no errors, validation sucess
        /// </summary>
        public List<Error> Errors{ get; set; }

        public FileInfo()
        {
            Name = string.Empty;
            Type = string.Empty; ;
            Lenght = 0 ;
            Description = string.Empty;
            Errors = new List<Error>();
        }

        public FileInfo(string name, string type, int length, string description)
        {
            Name = name;
            Type = type;
            Lenght = length;
            Description = description;
            Errors = new List<Error>();
        }
    }
}