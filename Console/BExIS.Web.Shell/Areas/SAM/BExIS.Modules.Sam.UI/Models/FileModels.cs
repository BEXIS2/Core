using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Linq;

namespace BExIS.Modules.Sam.UI.Models
{
    public class FileOrFolderModel
    {
        public FileOrFolderModel()
        {
            Type = "UNKNOWN";
        }

        [Display(Name = "Name")]
        [Required]
        [StringLength(100, ErrorMessage = "The description must be {2} - {1} characters long.", MinimumLength = 1)]
        public string Name { get; set; }

        [Display(Name = "Display Name")]
        [Required]
        [StringLength(100, ErrorMessage = "The display name must be {2} - {1} characters long.", MinimumLength = 1)]
        public string DisplayName { get; set; }

        [Display(Name = "Description")]
        [Required]
        [StringLength(250, ErrorMessage = "The description must be {2} - {1} characters long.", MinimumLength = 3)]
        public string Description { get; set; }

        /// <summary>
        /// Fully qualifiled dot separated virtual path to the item starting from the tenant
        /// In case of files, the path contains the file name and its extension too.
        /// </summary>
        /// <example>BExIS.Howtos.Printing.posterprinting.pdf</example>
        [Display(Name = "Path")]
        [Editable(false)]
        [Required]
        public string Path { get; set; }

        public string Type { get; protected set; }

        /// <summary>
        /// Length of file in bytes
        /// Folders have size of zero
        /// </summary>
        [Display(Name = "Size")]
        public long Size { get; set; }

        public string XPath
        {
            get
            {
                //if (Type == "FILE" && Name.Contains(".") && !string.IsNullOrEmpty(Path))
                //{
                //    var path = Path.Replace(Name, "").Replace(".","/") + Name;
                //    return string.Concat("./", path);
                //}
                //else
                return !string.IsNullOrEmpty(Path) ? string.Concat("./", Path.Replace("|", "/")) : string.Empty;
            }
        }

        public static string InferPath(XElement element)
        {
            return string.Join("|", (from el in element.AncestorsAndSelf() select el.Attribute("name").Value).Reverse());

            // debug version
            //string path = string.Empty;
            //IEnumerable<XElement> anssestorReverseList = from el in element.AncestorsAndSelf() select el;
            //path = string.Join(".", anssestorReverseList.ToList().Select(e => e.Name));
            //return path;
        }

        public static FileOrFolderModel Convert(XElement element)
        {
            string name = element.Attribute("name").Value;
            string displayName = element.Attribute("displayName").Value;
            string description = element.Attribute("description").Value;

            return new FolderModel()
            {
                Name = name,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName,
                Description = string.IsNullOrWhiteSpace(description) ? displayName : description,
                Path = InferPath(element)
            };
        }
    }

    public class FileModel : FileOrFolderModel
    {
        public FileModel()
        {
            Type = "FILE";
        }

        [Display(Name = "Mime Type")]
        [Required]
        [StringLength(50, ErrorMessage = "The display name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string MimeType { get; set; }

        public static FileModel Convert(XElement element)
        {
            if (!element.Attribute("type").Value.Equals("FILE", System.StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("Element type is not valid, an element of type 'FILE' was expected.");

            string name = element.Attribute("name").Value;
            string displayName = element.Attribute("displayName").Value;
            string description = element.Attribute("description").Value;
            string mimeType = element.Attribute("mimeType").Value;
            string size = element.Attribute("size").Value;
            return new FileModel()
            {
                Name = name,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName,
                Description = string.IsNullOrWhiteSpace(description) ? displayName : description,
                MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/text" : mimeType,
                Path = FileOrFolderModel.InferPath(element),
                Size = string.IsNullOrWhiteSpace(size) ? 0 : long.Parse(size)
            };
        }
    }

    public class FolderModel : FileOrFolderModel
    {
        public FolderModel()
        {
            Type = "FOLDER";
        }

        //public List<FileOrFolderModel> Children { get; set; }
        public List<FolderModel> Children { get; set; }

        public static FolderModel Convert(XElement element, bool includeChildren = true)
        {
            if (!element.Attribute("type").Value.Equals("FOLDER", System.StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("Element type is not valid, an element of type 'FOLDER' was expected.");

            string name = element.Attribute("name").Value;
            string displayName = element.Attribute("displayName").Value;
            string description = element.Attribute("description").Value;

            return new FolderModel()
            {
                Name = name,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName,
                Description = string.IsNullOrWhiteSpace(description) ? displayName : description,
                Path = FileOrFolderModel.InferPath(element),
                Children = includeChildren == true ? ConvertChildren(element.Elements()) : new List<FolderModel>(), //(from c in element.Elements().Select(p=> ConvertChild(p)).ToList() where (c != null) select c).ToList(), // recursively converts all files and folders belonging to the current folder.
            };
        }

        public static List<FolderModel> ConvertChildren(IEnumerable<XElement> children)
        {
            List<FolderModel> childList = new List<FolderModel>();
            foreach (var child in children)
            {
                if (child.Attribute("type").Value.Equals("FOLDER", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    childList.Add(FolderModel.Convert(child));
                }
            }
            return childList;
        }
    }
}