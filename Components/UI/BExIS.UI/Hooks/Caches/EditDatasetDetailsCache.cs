using BExIS.IO.Transform.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// if something is done or any result from a hook should generate a Message to show the user the result of the hook
        /// This list is the collection of all this messages
        /// </summary>
        public List<ResultMessage> Messages { get; set; }

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
            Messages = new List<ResultMessage>();
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

        public FileInfo()
        { }

        public FileInfo(string name, string type, int length, string description)
        {
            Name = name;
            Type = type;
            Lenght = length;
            Description = description;
        }
    }
}