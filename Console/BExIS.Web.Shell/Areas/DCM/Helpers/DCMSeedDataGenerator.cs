﻿
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class DcmSeedDataGenerator : IModuleSeedDataGenerator
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


        public void GenerateSeedData()
        {

            ResearchPlanManager researchPlanManager = new ResearchPlanManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            UnitManager unitManager = new UnitManager();
            EntityManager entityManager = new EntityManager();
            FeatureManager featureManager = new FeatureManager();
            OperationManager operationManager = new OperationManager();

            try
            {
                #region create none researchPlan

                if (!researchPlanManager.Repo.Get().Any(r => r.Title.Equals("none")))
                {
                    researchPlanManager.Create("none", "If no research plan is used.");

                }



                #endregion

                #region create none structure


                if (!dataStructureManager.AllTypesDataStructureRepo.Get().Any(d => d.Name.Equals("none")))
                {
                    dataStructureManager.CreateUnStructuredDataStructure("none", "If no data strutcure is used.");

                }



                #endregion

                #region create none unit

                Dimension dimension = null;

                if (!unitManager.DimensionRepo.Get().Any(d => d.Name.ToLower().Equals("none")))
                {
                    dimension = unitManager.Create("none", "none", "If no unit is used."); // the null dimension should be replaced bz a proper valid one. Javad 11.06
                }
                else
                {
                    dimension = unitManager.DimensionRepo.Get().Where(d => d.Name.ToLower().Equals("none")).FirstOrDefault();
                }

                if (!unitManager.Repo.Get().Any(u => u.Name.ToLower().Equals("none")))
                {
                    unitManager.Create("none", "none", "If no unit is used.", dimension, MeasurementSystem.Unknown);
                }

                #endregion

                #region create entities

                // Entities
                Entity entity = entityManager.Entities.Where(e => e.Name.ToUpperInvariant() == "Dataset".ToUpperInvariant()).FirstOrDefault();

                if (entity == null)
                {
                    entity = new Entity();
                    entity.Name = "Dataset";
                    entity.EntityType = typeof(Dataset);
                    entity.EntityStoreType = typeof(Xml.Helpers.DatasetStore);
                    entity.UseMetadata = true;
                    entity.Securable = true;

                    entityManager.Create(entity);
                }



                #endregion

                #region SECURITY
                //workflows = größere sachen, vielen operation
                //operations = einzelne actions

                //1.controller-> 1.Operation




                Feature DataCollectionFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Collection"));
                if (DataCollectionFeature == null) DataCollectionFeature = featureManager.Create("Data Collection", "Data Collection");

                Feature DatasetCreationFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Creation"));
                if (DatasetCreationFeature == null) DatasetCreationFeature = featureManager.Create("Data Creation", "Data Creation", DataCollectionFeature);

                Feature DatasetUploadFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Dataset Upload"));
                if (DatasetUploadFeature == null) DatasetUploadFeature = featureManager.Create("Dataset Upload", "Dataset Upload", DataCollectionFeature);

                Feature MetadataManagementFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Metadata Management"));
                if (MetadataManagementFeature == null) MetadataManagementFeature = featureManager.Create("Metadata Management", "Metadata Management", DataCollectionFeature);


                #region Help Workflow

                operationManager.Create("DCM", "Help", "*");

                #endregion

                #region Create Dataset Workflow

                operationManager.Create("DCM", "CreateDataset", "*", DatasetCreationFeature);
                operationManager.Create("DCM", "Form", "*");

                #endregion

                #region Update Dataset Workflow

                operationManager.Create("DCM", "Push", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "Submit", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "SubmitDefinePrimaryKey", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "SubmitGetFileInformation", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "SubmitSelectAFile", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "SubmitSpecifyDataset", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "SubmitSummary", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "SubmitValidation", "*", DatasetUploadFeature);

                #endregion

                #region Easy Upload

                operationManager.Create("DCM", "EasyUpload", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "EasyUploadSelectAFile", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "EasyUploadSelectAreas", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "EasyUploadSheetDataStructure", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "EasyUploadSheetSelectMetaData", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "EasyUploadSummary", "*", DatasetUploadFeature);
                operationManager.Create("DCM", "EasyUploadVerification", "*", DatasetUploadFeature);

                #endregion

                #region Metadata Managment Workflow

                operationManager.Create("DCM", "ImportMetadataStructure", "*", MetadataManagementFeature);
                operationManager.Create("DCM", "ImportMetadataStructureReadSource", "*", MetadataManagementFeature);
                operationManager.Create("DCM", "ImportMetadataStructureSelectAFile", "*", MetadataManagementFeature);
                operationManager.Create("DCM", "ImportMetadataStructureSetParameters", "*", MetadataManagementFeature);
                operationManager.Create("DCM", "ImportMetadataStructureSummary", "*", MetadataManagementFeature);
                operationManager.Create("DCM", "ManageMetadataStructure", "*", MetadataManagementFeature);
                operationManager.Create("DCM", "SubmitSpecifyDataset", "*", MetadataManagementFeature);

                #endregion

                #region public available

                operationManager.Create("DCM", "Form", "*");

                #endregion

                #endregion

                #region Add Metadata 
                MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

                if (!metadataStructureManager.Repo.Get().Any(m => m.Name.Equals("Basic ABCD")))
                {
                    string titleXPath =
                        "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Title/TitleType";
                    string descriptionXpath =
                        "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Details/DetailsType";


                    ImportSchema("Basic ABCD", "ABCD_2.06.XSD", "DataSet", entity.Name, entity.EntityType.FullName,
                        titleXPath, descriptionXpath);

                }

                //if (!metadataStructureManager.Repo.Get().Any(m => m.Name.Equals("Full ABCD")))
                //{
                //    string titleXPath =
                //        "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Title/TitleType";
                //    string descriptionXpath =
                //        "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Details/DetailsType";


                //    ImportSchema("Full ABCD", "ABCD_2.06.XSD", "DataSet", entity.Name, entity.EntityType.FullName,
                //        titleXPath, descriptionXpath);

                //}

                if (!metadataStructureManager.Repo.Get().Any(m => m.Name.Equals("GBIF")))
                {

                    string titleXPath = "Metadata/Basic/BasicType/title/titleType";
                    string descriptionXpath = "Metadata/abstract/abstractType/para/paraType";

                    ImportSchema("GBIF", "eml.xsd", "Dataset", entity.Name, entity.EntityType.FullName, titleXPath,
                        descriptionXpath);
                }
                //if (!metadataStructureManager.Repo.Get().Any(m => m.Name.Equals("Basic Eml")))
                //    ImportSchema("Basic Eml", "eml-dataset.xsd", entity.Name, entity.Name, entity.EntityType.FullName);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                researchPlanManager.Dispose();
                dataStructureManager.Dispose();
                unitManager.Dispose();
                entityManager.Dispose();
                featureManager.Dispose();
                operationManager.Dispose();
            }
        }


        #region METADATA

        private void ImportSchema(string name, string filename, string root, string entity, string entityFullName, string titlePath, string descriptionPath)
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
                metadataStructureid = xmlSchemaManager.GenerateMetadataStructure(root, schemaName);

                try
                {
                    //set parameters
                    string mappingFileImport = xmlSchemaManager.mappingFileNameImport;
                    string mappingFileExport = xmlSchemaManager.mappingFileNameExport;

                    StoreParametersToMetadataStruture(
                        metadataStructureid,
                        titlePath,
                        descriptionPath,
                        entity,
                        entityFullName,
                        mappingFileImport,
                        mappingFileExport);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            catch (Exception ex)
            {
                xmlSchemaManager.Delete(schemaName);
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
        private void StoreParametersToMetadataStruture(long id, string titlePath, string descriptionPath, string entity, string entityFullName, string mappingFilePathImport, string mappingFilePathExport)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(id);
            EntityManager entityManager = new EntityManager();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                if (metadataStructure != null)
                {
                    if (metadataStructure.Extra != null)
                    {
                        xmlDoc = (XmlDocument)metadataStructure.Extra;
                    }

                    // add title Node
                    xmlDoc = AddReferenceToMetadatStructure("title", titlePath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);
                    // add Description
                    xmlDoc = AddReferenceToMetadatStructure("description", descriptionPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);

                    xmlDoc = AddReferenceToMetadatStructure(entity, entityFullName, AttributeType.entity.ToString(), "extra/entity", xmlDoc);

                    // add mappingFilePath
                    xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathImport, "mappingFileImport", "extra/convertReferences/convertRef", xmlDoc);
                    xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathExport, "mappingFileExport", "extra/convertReferences/convertRef", xmlDoc);

                    //set active
                    xmlDoc = AddReferenceToMetadatStructure(NameAttributeValues.active.ToString(), true.ToString(), AttributeType.parameter.ToString(), "extra/parameters/parameter", xmlDoc);

                    metadataStructure.Extra = xmlDoc;
                    mdsManager.Update(metadataStructure);
                }
            }
            finally
            {
                mdsManager.Dispose();
                entityManager.Dispose();
            }
        }

        private XmlDocument AddReferenceToMetadatStructure(string nodeName, string nodePath, string nodeType, string destinationPath, XmlDocument xmlDoc)
        {

            XmlDocument doc = xmlDatasetHelper.AddReferenceToXml(xmlDoc, nodeName, nodePath, nodeType, destinationPath);

            return doc;

        }

        #endregion

        #endregion

        #endregion

        public void Dispose()
        {
            // nothing to do for now...
        }

    }
}