using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using Vaiona.Utils.Cfg;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Objects;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Pms.Services;
using BExIS.Xml.Helpers.Mapping;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using System.Xml.Linq;
using BExIS.Pms.Entities.Publication;

namespace BExISMigration
{
    public class Migration
    {
        private string DataBase = "Database=BIODIV1;UserID=db2admin;Password=.javad.T410.;Server=localhost:50000;";
        public void UserMigration()
        {
            // data base of bexis1 user query
            string filePath = AppConfiguration.GetModuleWorkspacePath("BMM");
            // names of the features witch set to the group "bexisUser"
            string[] featureNames = { "Search", "Data Collection", "Research Plan", "Data Dissemination" };

            UserMigration userMigration = new UserMigration();

            // create or get a group named "bexisUser"
            long groupId = userMigration.bexisUserGroup();

            // set feature permission of the group "bexisUser"
            userMigration.SetFeaturePermissions(groupId, featureNames);

            // query bexis1 user from provider.users and generate a random password
            List<UserProperties> BExISUsers = userMigration.GetFromBExIS(DataBase);

            // transfer users to bpp; not all user data are provided in bpp
            // save usernames and passwords in file "passwords.txt"
            userMigration.CreateOnBPP(BExISUsers, filePath, groupId);
        }

        public string TransferDataPermission()
        {
            SubjectManager subjectManager = new SubjectManager();
            var securityMigration = new SecurityMigration();
            Dictionary<int, int> DataSetIDs = new Dictionary<int, int>();
            var groups = subjectManager.GetAllGroups();
            string DatasetMappingPath = Path.Combine(AppConfiguration.DataPath, "DatasetMapping.txt");
            //Key is last datasetId and value is the new one
            Dictionary<int, int> DatasetsMapping = File.ReadAllLines(DatasetMappingPath).AsEnumerable().Select(item => new { oldId = int.Parse(item.Split('\t')[0]), newId = int.Parse(item.Split('\t')[1]) }).ToDictionary(c => c.oldId, c => c.newId);
            DatasetManager dm = new DatasetManager();

            PermissionManager permissionManager = new PermissionManager();
            List<SecurityMigration.Right> rights = securityMigration.GetBexisRights(DataBase, DatasetsMapping);
            foreach (var group in groups)
            {
                var groupRights = rights.Where(item => item.RoleName == group.Name || item.RoleName == "_" + group.Name);

                foreach (var right in groupRights)
                {
                    int newDataSetId = DatasetsMapping.FirstOrDefault(item => item.Key == right.DataSetId).Value;

                    //each entity wich exists in this list has view and download feature
                    permissionManager.CreateDataPermission(group.Id, 1, newDataSetId, RightType.View);
                    permissionManager.CreateDataPermission(group.Id, 1, newDataSetId, RightType.Download);

                    if (right.CanEdit)
                        permissionManager.CreateDataPermission(group.Id, 1, newDataSetId, RightType.Update);

                }
            }
            foreach (var DatasetMapping in DatasetsMapping)
            {
                //extract grant user from the last version and add it to new ver
                if (dm.GetDataset(DatasetMapping.Value) == null)
                    continue;
                DatasetVersion dsv = dm.GetDatasetLatestVersion(DatasetMapping.Value);
                string grantUserEmailAddress = dsv.Metadata.SelectSingleNode("Metadata/general/general/designatedDatasetManager/contactType/email/email").InnerText;
                if (!string.IsNullOrEmpty(grantUserEmailAddress))
                {
                    var grantUser = subjectManager.GetUserByEmail(grantUserEmailAddress);
                    permissionManager.CreateDataPermission(grantUser.Id, 1, DatasetMapping.Value, RightType.Grant);
                }
            }
            return "All of permissions transfered successfully.";
        }



        public string TransferPublications()
        {
            string xsdPath = Path.Combine(AppConfiguration.DataPath, "Temp", "Administrator");
            PublicationManager pms = new PublicationManager();
            MetadataCreator metadataCreator = new MetadataCreator();
            //Import MetaDataStructure
            string schemaFile = xsdPath + @"\publication.xsd";
            //open schema
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            string userName = "Administrator";
            xmlSchemaManager.Load(schemaFile, userName);
            long metadataStructureId = 0;
            try
            {
                metadataStructureId = metadataCreator.importMetadataStructure(schemaFile, userName, schemaFile, "Publication", "Metadata/publicationDetails/publicationDetails/title/title", "Metadata/publicationDetails/publicationDetails/information/information");// xmlSchemaManager.GenerateMetadataStructure("publication", "publication");
            }
            catch (Exception ex)
            {
                xmlSchemaManager.Delete("publication");
                return "Couldn't create publication metadatastructure!";
            }
            //Create an empty publication
            MetadataStructureManager msm = new MetadataStructureManager();
            MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);
            XmlDocument metadataXmlTemplate = BExIS.Xml.Helpers.XmlMetadataWriter.ToXmlDocument(metadataCreator.createXmlTemplate(metadataStructureId));
            var publicationsMetaData = metadataCreator.getPublicationsMetadataXml(DataBase);
            var xmlMapperManager = new XmlMapperManager();
            string filePath = AppConfiguration.GetModuleWorkspacePath("BMM");
            string path_mappingFile = filePath + @"\bexis_publication_metadata_mapping.xml";
            foreach (var publicationMetaData in publicationsMetaData)
            {
                var pc = pms.CreateEmptyPublication(metadataStructure);
                var publicationId = pc.Id;
                if (pms.IsPublicationCheckedOutFor(publicationId, userName) || pms.CheckOutPublication(publicationId, userName))
                {

                    PublicationVersion workingCopy = pms.GetPublicationWorkingCopy(publicationId);
                    // XML mapper + mapping file
                    xmlMapperManager.Load(path_mappingFile, "Publication");
                    XmlDocument metadataBpp = xmlMapperManager.Generate(publicationMetaData.MetaDataXml, 99);
                    metadataBpp = metadataCreator.fillInXmlAttributes(metadataBpp, metadataXmlTemplate);
                    workingCopy.Metadata = metadataBpp;

                    foreach (var pbContent in publicationMetaData.PublicationContentDescriptors)
                    {
                        string storePath = Path.Combine(AppConfiguration.DataPath, "Publications", publicationId.ToString());
                        storePath = Path.Combine(storePath, publicationId.ToString() + "_" + workingCopy.VersionNo.ToString() + "_" + pbContent.Name);
                        File.Move(pbContent.URI, storePath);
                        pbContent.URI = storePath;
                        pbContent.PublicationVersion = workingCopy;
                        workingCopy.PublicationContentDescriptors.Add(pbContent);
                    }
                    pms.CheckInPublication(publicationId, "Metadata was submited.", userName);
                }
            }
            return "";
        }

        public string TransferIPMLastFiles()
        {
            var lastFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BMM"), "LastIPMFilePath.txt");
            var destinationFolders = Path.Combine(AppConfiguration.DataPath, "IPM/LastIPMFiles");
            if (File.Exists(lastFilePath))
            {

                if (!Directory.Exists(destinationFolders))
                    Directory.CreateDirectory(destinationFolders);
                DirectoryCopy(File.ReadAllText(lastFilePath), destinationFolders, true);


            }
            return destinationFolders;
        }

        public void DataSetTransfer()
        {


            string filePath = AppConfiguration.GetModuleWorkspacePath("BMM");
            //string xsdPath = @"C:\Data\Temp\Administrator";
            string xsdPath = Path.Combine(AppConfiguration.DataPath, "Temp", "Administrator");
            string xsdUserName = "Administrator";
            string researchPlanName = "Research plan";

            MappingReader mappingReader = new MappingReader();
            AttributeCreator attributeCreator = new AttributeCreator();
            MetadataCreator metadataCreator = new MetadataCreator();
            MigrationHelpers migrationHelpers = new MigrationHelpers();

            #region create datatypes, unit, dimensions for datastructure from mapping file
            // read data types from excel mapping file
            DataTable mappedDataTypes = mappingReader.readDataTypes(filePath);
            // create read data types in bpp
            attributeCreator.CreateDataTypes(ref mappedDataTypes);

            // read dimensions from excel mapping file
            DataTable mappedDimensions = mappingReader.readDimensions(filePath);
            // create dimensions in bpp
            attributeCreator.CreateDimensions(ref mappedDimensions);

            // read units from excel mapping file
            DataTable mappedUnits = mappingReader.readUnits(filePath, mappedDimensions);
            // create read units in bpp
            attributeCreator.CreateUnits(ref mappedUnits, mappedDataTypes);

            // read attributes from excel mapping file
            DataTable mappedAttributes = mappingReader.readAttributes(filePath, mappedUnits, mappedDataTypes);
            // Speicher freigeben
            mappedDataTypes.Clear();
            mappedDimensions.Clear();
            // create read attributes in bpp
            attributeCreator.CreateAttributes(ref mappedAttributes);

            // read variables from XLSX or TSV (tab seperated values) mapping file
            //DataTable mappedVariables = mappingReader.readVariables(filePath, mappedAttributes);
            DataTable mappedVariables = mappingReader.readVariablesTSV(filePath, mappedAttributes, mappedUnits);
            // Speicher freigeben
            mappedAttributes.Clear();
            mappedUnits.Clear();

            #endregion

            #region create metadatastructure and prepare a xmltemplate based on the structure

            // if not existing: import METADATA structure, using file "schema.xsd" similar to bexis1 schema
            long metadataStructureId = metadataCreator.importMetadataStructure(xsdPath, xsdUserName);
            // create BppmetadataXmlTemplate to get XmlAttributes
            XmlDocument metadataXmlTemplate = BExIS.Xml.Helpers.XmlMetadataWriter.ToXmlDocument(metadataCreator.createXmlTemplate(metadataStructureId));

            #endregion

            #region create Dataset(Metadata,Datastructure)
            // the dataStructure, metadata and dataset of these bexis1 datasets will be transfered
            DataStructureCreator dataStructureCreator = new DataStructureCreator();
            DatasetCreator datasetCreator = new DatasetCreator();
            List<string> datasetIds = migrationHelpers.getDatasets(filePath);
            foreach (string dataSetID in datasetIds)
            {
                dataStructureCreator = new DataStructureCreator();
                datasetCreator = new DatasetCreator();
                XmlDocument metadataXml = new XmlDocument();
                List<string> variableNames = new List<string>();
                string fileType = "";

                // get metadata from Bexis1 DB and restructure it for Bpp using xml-mapping file
                metadataXml = metadataCreator.createMetadata(dataSetID, filePath, DataBase, ref variableNames, ref fileType);

                //long dataStructureId = -1;
                BExIS.Dlm.Entities.DataStructure.DataStructure dataStructure = null;
                if (fileType == "structuredData" || fileType == "lookupTable")
                {
                    // create dataStructure on bpp
                    dataStructure = dataStructureCreator.CreateDataStructure(dataSetID, mappedVariables, variableNames);
                }
                else if (fileType.Length > 0)
                {
                    // create unstructured dataStructure on bpp
                    dataStructure = dataStructureCreator.CreateDataStructure(fileType);
                }

                long datasetId = -1;
                if (dataStructure.Id > 0)
                {
                    datasetId = datasetCreator.DatasetExistsByOldDatasetId(dataSetID, dataStructure.Id);
                }

                if (datasetId <= 0) // dataset not exists => create dataset
                {
                    // integrate BppXmlAttributes by using metadataXmlTemplate
                    metadataXml = metadataCreator.fillInXmlAttributes(metadataXml, metadataXmlTemplate);

                    // create dataset with Bexis1_metadata
                    datasetId = datasetCreator.createDataset(dataSetID, metadataXml, metadataStructureId, dataStructure, researchPlanName);
                }

                string DatasetMappingPath = Path.Combine(AppConfiguration.DataPath, "DatasetMapping.txt");
                string[] DatasetMappingtexts = File.ReadAllLines(DatasetMappingPath);
                string DatasetMappingtext = DatasetMappingtexts.FirstOrDefault(item => item.Split('\t')[0] == dataSetID);
                //to prevent of repeating data
                if (DatasetMappingtext != null)
                    File.WriteAllLines(DatasetMappingPath, DatasetMappingtexts.Where(item => item != DatasetMappingtext));
                File.AppendAllText(DatasetMappingPath, dataSetID + "\t" + datasetId + "\r\n");


            }
            #endregion

        }


        public void PrimaryDataTransfer()
        {
            string filePath = AppConfiguration.GetModuleWorkspacePath("BMM");

            PrimaryData primaryData = new PrimaryData();
            MigrationHelpers migrationHelpers = new MigrationHelpers();

            List<string> datasets = migrationHelpers.getDatasets(filePath);
            foreach (string dataSetID in datasets)
            {
                // transfer primary data to bpp
                primaryData.uploadData(dataSetID, DataBase);
            }
        }

        public string TransferRoles()
        {
            SecurityMigration securityMigration = new SecurityMigration();
            List<string> roles = securityMigration.GetBexisRoles(DataBase);
            SubjectManager subjectManager = new SubjectManager();
            int newGroups = 0;
            foreach (string role in roles)
            {
                string roleName = role;
                if (role.IndexOf('_') == 0)
                    roleName = role.Substring(1, role.Length - 1);
                if (subjectManager.GetGroupByName(roleName) == null)
                {
                    newGroups++;
                    subjectManager.CreateGroup(roleName, roleName);
                    List<string> usersInRole = securityMigration.GetBexisUsersInRole(DataBase, roleName);
                    foreach (string userName in usersInRole)
                    {
                        if (subjectManager.GetUserByName(userName) != null)
                            subjectManager.AddUserToGroup(userName, roleName);
                    }

                }
            }
            return "Groups was successfully transfered and old bexis users added to them";
        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (!File.Exists(temppath))
                    file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
