using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Objects;

namespace BExISMigration
{
    public class Observation
    {
        public long obsid { get; set; }
        public XmlDocument data { get; set; }
        public char deleted { get; set; }
        public char newest { get; set; }
    }


    public class DatasetCreator
    {
        public long createDataset(string dataSetID, XmlDocument metadataXml, long metadataStructureId, DataStructure dataStructure, string researchPlanName)
        {
            Dataset dataset = new Dataset();

            DatasetManager datasetManager = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            ResearchPlanManager researchPlanManager = new ResearchPlanManager();
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            // get existing researchPlan
            ResearchPlan researchPlan = researchPlanManager.Repo.Get(r => researchPlanName.Equals(r.Title)).FirstOrDefault();

            // get existing metadataStructure (created manualy by using edited Bexis1_XSD)
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(metadataStructureId);

            // update verwendet, da bei unstructured structures im dataset constructor dataset(dataStructure) nullexception
            dataset = datasetManager.CreateEmptyDataset(dataStructure, researchPlan, metadataStructure);
            long datasetId = dataset.Id;

            // dataPermission
            List<string> usersWithDataPerm = new List<string>();

            // create dataPermission for originalDatasetManager
            string originalDatasetManager = metadataXml.SelectSingleNode("Metadata/general/general/originalDatasetManager/originalDatasetManager").InnerText;
            User userODM = subjectManager.UsersRepo.Get(u => originalDatasetManager.Equals(u.FullName)).FirstOrDefault();
            permissionManager.CreateDataPermission(userODM.Id, 1, dataset.Id, RightType.Create);
            permissionManager.CreateDataPermission(userODM.Id, 1, dataset.Id, RightType.View);
            permissionManager.CreateDataPermission(userODM.Id, 1, dataset.Id, RightType.Update);
            permissionManager.CreateDataPermission(userODM.Id, 1, dataset.Id, RightType.Download);
            usersWithDataPerm.Add(originalDatasetManager);

            // create dataPermissions for metadataAuthor
            string metadataAuthor = metadataXml.SelectSingleNode("Metadata/general/general/metadataCreator/metadataCreator").InnerText;
            if (!usersWithDataPerm.Contains(metadataAuthor))
            {
                User userMA = subjectManager.UsersRepo.Get(u => metadataAuthor.Equals(u.FullName)).FirstOrDefault();
                permissionManager.CreateDataPermission(userMA.Id, 1, dataset.Id, RightType.Create);
                permissionManager.CreateDataPermission(userMA.Id, 1, dataset.Id, RightType.View);
                permissionManager.CreateDataPermission(userMA.Id, 1, dataset.Id, RightType.Update);
                permissionManager.CreateDataPermission(userMA.Id, 1, dataset.Id, RightType.Download);
                usersWithDataPerm.Add(metadataAuthor);
            }

            // create dataPermissions for designatedDatasetManager
            string designatedDatasetManager = metadataXml.SelectSingleNode("Metadata/general/general/designatedDatasetManager/contactType/designatedDatasetManagerName/designatedDatasetManagerName").InnerText;
            if (!usersWithDataPerm.Contains(designatedDatasetManager))
            {
                User userDDM = subjectManager.UsersRepo.Get(u => designatedDatasetManager.Equals(u.FullName)).FirstOrDefault();
                permissionManager.CreateDataPermission(userDDM.Id, 1, dataset.Id, RightType.Create);
                permissionManager.CreateDataPermission(userDDM.Id, 1, dataset.Id, RightType.View);
                permissionManager.CreateDataPermission(userDDM.Id, 1, dataset.Id, RightType.Update);
                permissionManager.CreateDataPermission(userDDM.Id, 1, dataset.Id, RightType.Download);
                usersWithDataPerm.Add(designatedDatasetManager);
            }

            // create dataPermissions for owners
            XmlNodeList ownerNodes = metadataXml.SelectNodes("Metadata/general/general/owners/ownerType/owner/owner");
            foreach (XmlNode ownerNode in ownerNodes)
            {
                string owner = ownerNode.InnerText;
                if (!usersWithDataPerm.Contains(owner))
                {
                    User userO = subjectManager.UsersRepo.Get(u => owner.Equals(u.FullName)).FirstOrDefault();
                    if (userO != null)
                    {
                        permissionManager.CreateDataPermission(userO.Id, 1, dataset.Id, RightType.Create);
                        permissionManager.CreateDataPermission(userO.Id, 1, dataset.Id, RightType.View);
                        permissionManager.CreateDataPermission(userO.Id, 1, dataset.Id, RightType.Update);
                        permissionManager.CreateDataPermission(userO.Id, 1, dataset.Id, RightType.Download);
                        usersWithDataPerm.Add(owner);
                    }
                }
            }

            // integrate metadataXml to dataset
            // checkOut
            if (datasetManager.IsDatasetCheckedOutFor(datasetId, userODM.Name) || datasetManager.CheckOutDataset(datasetId, userODM.Name))
            {
                DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(datasetId); // get dataset
                workingCopy.Metadata = metadataXml; // set metadata to dataset
                datasetManager.EditDatasetVersion(workingCopy, null, null, null); // edit dataset
                datasetManager.CheckInDataset(datasetId, "Metadata was submited.", userODM.Name); // check in
            }

            return dataset.Id;
        }


        public long DatasetExistsByOldDatasetId(string dataSetID, long dataStructureId)
        {
            DatasetManager datasetManager = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            List<Dataset> datasetsWithCurrentDataStructure = new List<Dataset>();

            datasetsWithCurrentDataStructure = datasetManager.DatasetRepo.Get(d => dataStructureId.Equals(d.DataStructure.Id)).ToList();
            if (datasetsWithCurrentDataStructure != null && datasetsWithCurrentDataStructure.Count > 0)
            {
                foreach (Dataset datasetWCDS in datasetsWithCurrentDataStructure)
                {
                    string oldDatasetId = "";
                    try
                    {
                        XmlNode extraID = datasetWCDS.Versions.FirstOrDefault().Metadata.SelectSingleNode("Metadata/general/general/id/id");
                        oldDatasetId = extraID.InnerText;
                    }
                    catch
                    {
                    }
                    if (oldDatasetId == dataSetID)
                    {
                        return datasetWCDS.Id;
                    }
                }
            }
            return -999;
        }
    }
}
