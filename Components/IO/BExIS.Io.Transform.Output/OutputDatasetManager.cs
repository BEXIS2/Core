using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;

namespace BExIS.IO.Transform.Output
{
    public class OutputDatasetManager
    {
        public static GFBIODataCenterFormularObject GetGFBIODataCenterFormularObject(long datasetId)
        {
            DatasetManager datasetManager = new DatasetManager();
            datasetManager.GetDataset(datasetId); 

            Dataset dataset = datasetManager.GetDataset(datasetId);
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

            GFBIODataCenterFormularObject gfbioDataCenterFormularObject = new GFBIODataCenterFormularObject();

            gfbioDataCenterFormularObject.ProjectId = 1;
            gfbioDataCenterFormularObject.ProjectTitle = "Test Poject title";
            gfbioDataCenterFormularObject.ProjectLabel = "Test Poject label";
            gfbioDataCenterFormularObject.ProjectAbstract = "";

            gfbioDataCenterFormularObject.UserId = 1;
            gfbioDataCenterFormularObject.UserName = "TestUser";
            gfbioDataCenterFormularObject.UserEmail = "testEmail";
            gfbioDataCenterFormularObject.DatasetAuthor = "TestAuthor";

            gfbioDataCenterFormularObject.DatasetId = datasetId;
            gfbioDataCenterFormularObject.DatasetVersion = datasetVersion.Id;
            gfbioDataCenterFormularObject.DatasetTitle = XmlDatasetHelper.GetInformation(datasetId,NameAttributeValues.title);
            gfbioDataCenterFormularObject.DatasetLabel = XmlDatasetHelper.GetInformation(datasetId,NameAttributeValues.title);
            gfbioDataCenterFormularObject.DatasetDescription = XmlDatasetHelper.GetInformation(datasetId,NameAttributeValues.description);

            gfbioDataCenterFormularObject.DatasetCollectionDate = datasetVersion.Dataset.LastCheckIOTimestamp;
            
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(dataset.MetadataStructure.Id);

            gfbioDataCenterFormularObject.MetadataSchemaName = metadataStructure.Name;


            return gfbioDataCenterFormularObject;
        }

        public static GFBIOPangaeaFormularObject GetGFBIOPangaeaFormularObject(long datasetId)
        {
            DatasetManager datasetManager = new DatasetManager();
            datasetManager.GetDataset(datasetId);

            Dataset dataset = datasetManager.GetDataset(datasetId);
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(dataset.MetadataStructure.Id);

            GFBIOPangaeaFormularObject gfbioPangaeaFormularObject = new GFBIOPangaeaFormularObject();

            string title = XmlDatasetHelper.GetInformation(datasetId, NameAttributeValues.title);
            string description = XmlDatasetHelper.GetInformation(datasetId, NameAttributeValues.description);

            return gfbioPangaeaFormularObject;
        }

        public static string GetDynamicDatasetStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            return IoHelper.GetDynamicStorePath(datasetId, datasetVersionOrderNr, title, extention);
        }

        public static XmlDocument GenerateManifest(long datasetId, long versionId)
        {
            XmlDocument root = new XmlDocument();
            XmlNode manifest = XmlUtility.CreateNode("manifest", root);

            XmlElement elem = root.CreateElement("datasetId");
            elem.InnerText = datasetId.ToString();
            manifest.AppendChild(elem);

            elem = root.CreateElement("datasetversionid");
            elem.InnerText = versionId.ToString();
            manifest.AppendChild(elem);

            elem = root.CreateElement("generationtime");
            elem.InnerText = DateTime.UtcNow.ToString();
            manifest.AppendChild(elem);
            root.AppendChild(manifest);
            return root;
        }
    }

    public class OutputFormularObject
    {

    }

    #region GFBIO

    public class GFBIODataCenterFormularObject:OutputFormularObject
    {
        //project
        public long ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectLabel { get; set; }
        public string ProjectAbstract { get; set; }

        //User
        public IEnumerable<string> ResponsiblePersons { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string DatasetAuthor { get; set; }

        //Dataset
        public long DatasetId { get; set; }
        public long DatasetVersion { get; set; }
        public string DatasetTitle { get; set; }
        public string DatasetLabel { get; set; }
        public string DatasetDescription { get; set; }
        public DateTime DatasetCollectionDate { get; set; }

        //MetadataSchema
        public string MetadataSchemaName { get; set; }
        public IEnumerable<string> Keywords { get; set; }

        //Publications
        public string Publications { get; set; }
        
        //Embargos
        public string Embargos { get; set; }

    }

    public class GFBIOPangaeaFormularObject: OutputFormularObject
    {

    }

    #endregion

}
