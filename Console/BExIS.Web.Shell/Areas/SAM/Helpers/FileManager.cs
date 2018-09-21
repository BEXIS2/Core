using BExIS.Modules.Sam.UI.Models;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Sam.UI.Helpers
{
    public class FileManager : IDisposable
    {
        private string catalogPath = "";
        private XElement catalog = null;
        private FolderModel treeRoot = null;

        public FolderModel TreeRoot { get { return treeRoot; } }

        public FileManager(string tenantId)
        {
            try
            {
                catalogPath = Path.Combine(AppConfiguration.WorkspaceTenantsRoot, tenantId, "files", "catalog.xml");
                catalog = XElement.Load(catalogPath, LoadOptions.PreserveWhitespace);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not load catalog of files and folders for '{tenantId}'.", ex);
            }

        }
        public void Load()
        {
            try
            {
                treeRoot = FolderModel.Convert(catalog);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not load catalog of files and folders.", ex);
            }
        }

        public XElement GetFileByName(string name)
        {
            return null;
        }

        public XElement GetFileByPath(string path)
        {
            return null;
        }

        public XElement GetElementByPath(string path, bool pathOfFoldersOnly = true)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new DirectoryNotFoundException($"Path is empty.");

            // decompose the path
            List<string> tokens = path.Split('|').Skip(1).ToList(); // skip the root, as the catalog object already refers to the root

            //if (tokens.Count <=1)
            //    throw new DirectoryNotFoundException($"Path {path.Replace(".", "/")} does not exist.");

            // the first token should be equal to the tenant name as in the root of the catalog
            if (!path.Split('|').Take(1).Single().Equals(catalog.Attribute("name").Value, StringComparison.InvariantCultureIgnoreCase))
                throw new DirectoryNotFoundException($"Path {path.Replace("|", "/")} is not correct.");

            var folderElement = catalog;
            for (int i = 0; i < tokens.Count(); i++)
            {
                if (folderElement == null)
                    return null;
                if (pathOfFoldersOnly)
                {
                    folderElement = folderElement.Elements()
                                            .Where(p =>
                                                       p.Attribute("name").Value.Equals(tokens[i], StringComparison.InvariantCultureIgnoreCase)
                                                    && p.Attribute("type").Value.Equals("FOLDER", StringComparison.InvariantCultureIgnoreCase)
                                                )
                                            .SingleOrDefault();
                }
                else
                {
                    folderElement = folderElement.Elements()
                                            .Where(p =>
                                                       p.Attribute("name").Value.Equals(tokens[i], StringComparison.InvariantCultureIgnoreCase)
                                                )
                                            .SingleOrDefault();
                }
            }
            return folderElement;
        }

        public List<FileOrFolderModel> GetDirectChildrenByFolderPath(string path)
        {
            // ontain its direct children (files and folders)
            List<XElement> childrenElements = this.GetElementByPath(path).Elements().ToList();

            List<FileOrFolderModel> children = new List<FileOrFolderModel>();
            foreach (var item in childrenElements)
            {
                if (item.Attribute("type").Value.Equals("FOLDER", StringComparison.InvariantCultureIgnoreCase))
                {
                    children.Add(FolderModel.Convert(item, false));
                }
                else if (item.Attribute("type").Value.Equals("FILE", StringComparison.InvariantCultureIgnoreCase))
                {
                    children.Add(FileModel.Convert(item));
                }
            }

            // retuen the obtaind list
            return children;
        }

        public byte[] GetCompressedFile(string directoryPath)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(directoryPath);
                MemoryStream output = new MemoryStream();
                zip.Save(output);
                return output.ToArray();
            }
        }

        public void AddFolder(string name, string displayName, string description, string path)
        {
            XElement parent = this.GetElementByPath(path);
            if (parent == null)
                throw new DirectoryNotFoundException($"Path {path.Replace("|", "/")} does not exist.");
            // ctrate the folder if does not exist
            try
            {
                string physicalPath = Path.Combine(AppConfiguration.DataPath, "tenantsData", path.Replace('|', Path.DirectorySeparatorChar), name);
                if (Directory.Exists(physicalPath))
                    throw new Exception($"Folder '{name}' is already exist.");
                // Creates all directories and subdirectories as specified by path
                Directory.CreateDirectory(physicalPath);

                XElement folder = new XElement("Item",
                        new XAttribute("type", "FOLDER"),
                        new XAttribute("name", name),
                        new XAttribute("displayName", displayName),
                        new XAttribute("description", description)
                    );
                parent.Add(folder);
                catalog.Save(catalogPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not create folder '{name}' or one of its parents.", ex);
            }
        }

        public void AddFile(string name, string displayName, string description, string mimeType, string path, HttpPostedFileBase file)
        {
            XElement parent = this.GetElementByPath(path);
            if (parent == null)
                throw new DirectoryNotFoundException($"Path {path.Replace("|", "/")} does not exist.");
            // ctrate the folder if does not exist
            try
            {
                string physicalPath = Path.Combine(AppConfiguration.DataPath, "tenantsData", path.Replace('|', Path.DirectorySeparatorChar));
                // make sure all parent path components exist
                Directory.CreateDirectory(physicalPath);
                physicalPath = Path.Combine(physicalPath, name);
                // check for file exsistence
                if (File.Exists(physicalPath))
                    throw new InvalidOperationException($"File '{name}' already exists.");
                //save the file
                file.SaveAs(physicalPath);
                XElement fileElement = new XElement("Item",
                        new XAttribute("type", "FILE"),
                          new XAttribute("name", name),
                        new XAttribute("displayName", displayName),
                        new XAttribute("description", description),
                        new XAttribute("mimeType", mimeType),
                        new XAttribute("size", file.ContentLength)
                    );
                parent.Add(fileElement);
                catalog.Save(catalogPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not persist file '{name}'.", ex);
            }
        }

        /// <summary>
        /// Deletes the file od the folder specified by path
        /// </summary>
        /// <param name="path">the full dor separated path from the tenant to the target element (file or folder)</param>
        public void Delete(string path)
        {
            var element = GetElementByPath(path, false);
            if (element != null)
            {
                if (element.Attribute("type").Value.Equals("FILE", StringComparison.InvariantCultureIgnoreCase))
                {
                    string physicalPath = Path.Combine(AppConfiguration.DataPath, "tenantsData", path.Replace('|', Path.DirectorySeparatorChar));

                    File.Delete(physicalPath);
                    element.Remove();
                    catalog.Save(catalogPath);
                }
                else if (element.Attribute("type").Value.Equals("FOLDER", StringComparison.InvariantCultureIgnoreCase))
                {
                    string physicalPath = Path.Combine(AppConfiguration.DataPath, "tenantsData", path.Replace('|', Path.DirectorySeparatorChar));

                    Directory.Delete(physicalPath);
                    element.Remove();
                    catalog.Save(catalogPath);

                }
            }
        }
        /// <summary>
        /// Deletes the file od the folder specified by path
        /// </summary>
        /// <param name="path">the full dor separated path from the tenant to the target element (file or folder)</param>
        public void DeleteFileByXPath(string path)
        {
            var element = GetElementByPath(path, false);
            if (element != null)
            {
                if (element.Attribute("type").Value.Equals("FILE", StringComparison.InvariantCultureIgnoreCase))
                {
                    string physicalPath = Path.Combine(AppConfiguration.DataPath, "tenantsData", path);

                    File.Delete(physicalPath);
                    element.Remove();
                    catalog.Save(catalogPath);
                }
                else if (element.Attribute("type").Value.Equals("FOLDER", StringComparison.InvariantCultureIgnoreCase))
                {
                    Directory.Delete(path.Replace('|', Path.DirectorySeparatorChar));
                    element.Remove();
                    catalog.Save(catalogPath);
                }
            }
        }
        /// <summary>
        /// Renames the file or folder specified by path to the new string specified by name
        /// </summary>
        /// <param name="path">the full dot separated path of the lement conatining the name</param>
        /// <param name="name">the name only component that should be replaced in the path and on the file system</param>
        private void Rename(string path, string name)
        {
            var newFileNamePath = Path.Combine(AppConfiguration.DataPath, "tenantsData", Path.GetDirectoryName(path), name);
            path = Path.Combine(AppConfiguration.DataPath, "tenantsData", path);
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
                Directory.Move(path, newFileNamePath);
            else
                File.Move(path, newFileNamePath);
        }

        /// <summary>
        /// updates the file or folder specified by path using the information in model
        /// </summary>
        /// <param name="path">the full dot separated path of the lement conatining the name</param>
        /// <param name="model">a file or folder model object containing new values of dispaly name, description, etc.</param>
        /// <remarks>Does not update the path, hence does not change the name.</remarks>
        public void Update(string path, FileOrFolderModel model)
        {
            var element = GetElementByPath(path,false);
            if (element == null)
                throw new DirectoryNotFoundException($"Path {path.Replace("|", "/")} does not exist.");
            if (!element.Attribute("name").Value.Equals(model.Name))
            {
                element.SetAttributeValue("name", model.Name);
                Rename(model.XPath, model.Name);
            }
            element.SetAttributeValue("description", model.Description);
            element.SetAttributeValue("displayName", model.DisplayName);
            catalog.Save(catalogPath);
        }
        public void Dispose()
        {
            catalog = null;
            treeRoot = null;
        }

    }
}