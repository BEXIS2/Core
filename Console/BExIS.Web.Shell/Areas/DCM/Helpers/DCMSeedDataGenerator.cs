
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers.Mapping;
using BExIS.Xml.Services;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class DcmSeedDataGenerator
    {
        public static void GenerateSeedData()
        {

            #region create none researchPlan

            ResearchPlanManager researchPlanManager = new ResearchPlanManager();

            if (!researchPlanManager.Repo.Get().Any(r => r.Title.Equals("none")))
            {
                researchPlanManager.Create("none", "If no research plan is used.");

            }



            #endregion

            #region create none structure

            DataStructureManager dataStructureManager = new DataStructureManager();

            if (!dataStructureManager.AllTypesDataStructureRepo.Get().Any(d => d.Name.Equals("none")))
            {
                dataStructureManager.CreateUnStructuredDataStructure("none", "If no data strutcure is used.");

            }



            #endregion

            #region create none unit

            UnitManager unitManager = new UnitManager();
            Dimension dimension = null;

            if (!unitManager.DimensionRepo.Get().Any(d => d.Name.Equals("none")))
            {
                dimension = unitManager.Create("none", "none", "If no unit is used."); // the null dimension should be replaced bz a proper valid one. Javad 11.06
            }
            else
            {
                dimension = unitManager.DimensionRepo.Get().Where(d => d.Name.Equals("none")).FirstOrDefault();
            }

            if (!unitManager.Repo.Get().Any(u => u.Name.Equals("none")))
            {
                unitManager.Create("none", "none", "If no unit is used.", dimension, MeasurementSystem.Unknown);
            }

            #endregion

            #region create entities

            // Entities
            EntityManager entityManager = new EntityManager();

            Entity entity = new Entity();
            entity.Name = "Dataset";
            entity.AssemblyPath = "BExIS.Dlm.Entities";
            entity.ClassPath = "BExIS.Dlm.Entities.Data.Dataset";
            entity.UseMetadata = true;
            entity.Securable = true;

            entityManager.Create(entity);


            #endregion

            #region SECURITY
            //TaskManager taskManager = new TaskManager();
            //////generic form for metadata
            //taskManager.CreateTask("DCM", "Form", "*");
            //taskManager.CreateTask("DCM", "Help", "*");

            //Feature f1 = new Feature()
            //{
            //    Name = "Data Collection",
            //    Description = "Data Collection"
            //};

            //FeatureManager featureManager = new FeatureManager();
            //featureManager.Create(f1);
            //Feature f11 = featureManager.CreateFeature("Dataset Creation", "Dataset Creation", f1.Id);
            //Feature f12 = featureManager.CreateFeature("Dataset Upload", "Dataset Upload", f10.Id);
            //Feature f17 = featureManager.CreateFeature("Metadata Management", "Metadata Management", f10.Id);


            //#region Create
            //Task t9 = taskManager.CreateTask("DCM", "CreateDataset", "*");
            //t9.Feature = f11;
            //taskManager.UpdateTask(t9);
            //Task t10 = taskManager.CreateTask("DCM", "CreateSelectDatasetSetup", "*");
            //t10.Feature = f11;
            //taskManager.UpdateTask(t10);
            //Task t11 = taskManager.CreateTask("DCM", "CreateSetMetadataPackage", "*");
            //t11.Feature = f11;
            //taskManager.UpdateTask(t11);
            //Task t12 = taskManager.CreateTask("DCM", "CreateSummary", "*");
            //t12.Feature = f11;
            //taskManager.UpdateTask(t12);

            //#endregion

            //#region Upload

            //Task t15 = taskManager.CreateTask("DCM", "Push", "*");
            //t15.Feature = f12;
            //taskManager.UpdateTask(t15);
            //Task t16 = taskManager.CreateTask("DCM", "Submit", "*");
            //t16.Feature = f12;
            //taskManager.UpdateTask(t16);
            //Task t17 = taskManager.CreateTask("DCM", "SubmitDefinePrimaryKey", "*");
            //t17.Feature = f12;
            //taskManager.UpdateTask(t17);
            //Task t18 = taskManager.CreateTask("DCM", "SubmitGetFileInformation", "*");
            //t18.Feature = f12;
            //taskManager.UpdateTask(t18);
            //Task t19 = taskManager.CreateTask("DCM", "SubmitSelectAFile", "*");
            //t19.Feature = f12;
            //taskManager.UpdateTask(t19);
            //Task t20 = taskManager.CreateTask("DCM", "SubmitSpecifyDataset", "*");
            //t20.Feature = f12;
            //taskManager.UpdateTask(t20);
            //Task t21 = taskManager.CreateTask("DCM", "SubmitSummary", "*");
            //t21.Feature = f12;
            //taskManager.UpdateTask(t21);
            //Task t22 = taskManager.CreateTask("DCM", "SubmitValidation", "*");
            //t22.Feature = f12;
            //taskManager.UpdateTask(t22);

            //#endregion

            //#region Metadata 

            //Task t28 = taskManager.CreateTask("DCM", "ImportMetadataStructureReadSource", "*");
            //t28.Feature = f17;
            //taskManager.UpdateTask(t28);

            //Task t29 = taskManager.CreateTask("DCM", "ImportMetadataStructureSelectAFile", "*");
            //t29.Feature = f17;
            //taskManager.UpdateTask(t29);

            //Task t30 = taskManager.CreateTask("DCM", "ImportMetadataStructureSetParameters", "*");
            //t30.Feature = f17;
            //taskManager.UpdateTask(t30);

            //Task t31 = taskManager.CreateTask("DCM", "ImportMetadataStructureSummary", "*");
            //t31.Feature = f17;
            //taskManager.UpdateTask(t31);



            //Task t38 = taskManager.CreateTask("DCM", "ManageMetadataStructure", "*");
            //t38.Feature = f17;
            //taskManager.UpdateTask(t38);

            //#endregion



            #endregion

            #region Add Metadata 
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            if (!metadataStructureManager.Repo.Get().Any(m => m.Name.Equals("Basic ABCD")))
                ImportSchema("Basic ABCD", "ABCD_2.06.XSD", "Dataset", "BExIS.Dlm.Entities.Data.Dataset");

            #endregion
        }


        #region METADATA

        private static void ImportSchema(string name, string filename, string root, string entity)
        {
            long metadataStructureid = 0;
            string schemaName = name;

            string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", name,
                filename);

            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();

            //load
            try
            {
                xmlSchemaManager.Load(filepath, "application");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //generate
            try
            {
                metadataStructureid = xmlSchemaManager.GenerateMetadataStructure("Dataset", schemaName);
            }
            catch (Exception ex)
            {
                xmlSchemaManager.Delete(schemaName);
            }

            try
            {
                //set parameters
                string titleXPath = "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Title/TitleType";
                string descriptionXpath = "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Details/DetailsType";
                string mappingFileImport = xmlSchemaManager.mappingFileNameImport;
                string mappingFileExport = xmlSchemaManager.mappingFileNameExport;

                StoreParametersToMetadataStruture(
                    metadataStructureid,
                    titleXPath,
                    descriptionXpath,
                    entity,
                    mappingFileImport,
                    mappingFileExport);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        #region helper

        #region extra xdoc
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="titlePath"></param>
        /// <param name="descriptionPath"></param>
        /// <param name="mappingFilePath"></param>
        /// <param name="direction"></param>
        private static void StoreParametersToMetadataStruture(long id, string titlePath, string descriptionPath, string entity, string mappingFilePathImport, string mappingFilePathExport)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = mdsManager.Repo.Get(id);

            XmlDocument xmlDoc = new XmlDocument();

            if (metadataStructure.Extra != null)
            {
                xmlDoc = (XmlDocument)metadataStructure.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure("title", titlePath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);
            // add Description
            xmlDoc = AddReferenceToMetadatStructure("description", descriptionPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);

            xmlDoc = AddReferenceToMetadatStructure("entity", entity, AttributeType.entity.ToString(), "extra/entity", xmlDoc);

            // add mappingFilePath
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathImport, "mappingFileImport", "extra/convertReferences/convertRef", xmlDoc);
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathExport, "mappingFileExport", "extra/convertReferences/convertRef", xmlDoc);

            //set active
            xmlDoc = AddReferenceToMetadatStructure(NameAttributeValues.active.ToString(), true.ToString(), AttributeType.parameter.ToString(), "extra/parameters/parameter", xmlDoc);

            metadataStructure.Extra = xmlDoc;
            mdsManager.Update(metadataStructure);

        }

        private static XmlDocument AddReferenceToMetadatStructure(string nodeName, string nodePath, string nodeType, string destinationPath, XmlDocument xmlDoc)
        {

            XmlDocument doc = XmlDatasetHelper.AddReferenceToXml(xmlDoc, nodeName, nodePath, nodeType, destinationPath);

            return doc;

        }

        #endregion

        #endregion

        #endregion


    }
}