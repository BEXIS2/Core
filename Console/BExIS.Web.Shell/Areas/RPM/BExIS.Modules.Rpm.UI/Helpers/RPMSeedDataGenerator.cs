﻿using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Classes;
using BExIS.Modules.Rpm.UI.Helpers.SeedData;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI.Helpers
{
    public class RPMSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void GenerateSeedData()
        {

            FeatureManager featureManager = null;
            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
            OperationManager operationManager = new OperationManager();
            try
            {
                featureManager = new FeatureManager();
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                Feature dataPlanning = features.FirstOrDefault(f => f.Name.Equals("Data Planning"));
                if (dataPlanning == null)
                    dataPlanning = featureManager.Create("Data Planning", "Data Planning Management");

                Feature datastructureFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Datastructure Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (datastructureFeature == null)
                    datastructureFeature = featureManager.Create("Datastructure Management", "Datastructure Management", dataPlanning);

                if (!operationManager.Exists("RPM", "DataStructureSearch", "*"))
                    operationManager.Create("RPM", "DataStructureSearch", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "DataStructureEdit", "*"))
                    operationManager.Create("RPM", "DataStructureEdit", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "Structures", "*"))
                    operationManager.Create("RPM", "Structures", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "DataStructureIO", "*"))
                    operationManager.Create("RPM", "DataStructureIO", "*", datastructureFeature);

                Feature atributeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Variable Template Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (atributeFeature == null)
                    atributeFeature = featureManager.Create("Variable Template Management", "Variable Template Management", dataPlanning); ;

                if (!operationManager.Exists("RPM", "DataAttribute", "*"))
                    operationManager.Create("RPM", "DataAttribute", "*", atributeFeature);

                Feature unitFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Unit Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (unitFeature == null)
                    unitFeature = featureManager.Create("Unit Management", "Unit Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Unit", "*"))
                    operationManager.Create("RPM", "Unit", "*", unitFeature);

                Feature dataTypeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Data Type Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (dataTypeFeature == null)
                    dataTypeFeature = featureManager.Create("Data Type Management", "Data Type Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Home", "*"))
                    operationManager.Create("RPM", "Home", "*", dataTypeFeature);

                if (!operationManager.Exists("RPM", "Help", "*"))
                    operationManager.Create("RPM", "Help", "*");


                Feature api = features.FirstOrDefault(f =>
                    f.Name.Equals("API") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (api == null)
                    api = featureManager.Create("API", "API", dataPlanning);

                if (!operationManager.Exists("API", "Structures", "*"))
                    operationManager.Create("API", "Structures", "*", api);

                //set api public
                featurePermissionManager.Create(null, api.Id, Security.Entities.Authorization.PermissionType.Grant);

            }
            finally
            {
                featureManager.Dispose();
                featurePermissionManager.Dispose();
                operationManager.Dispose();
            }

            //create seed data from csv files
            MappingReader mappingReader = new MappingReader();
            AttributeCreator attributeCreator = new AttributeCreator();
            string filePath = AppConfiguration.GetModuleWorkspacePath("RPM");

            // read data types from csv file
            DataTable mappedDataTypes = mappingReader.readDataTypes(filePath);
            // create read data types in bpp
            attributeCreator.CreateDataTypes(ref mappedDataTypes);

            //// read dimensions from csv file
            DataTable mappedDimensions = mappingReader.readDimensions(filePath);
            // create dimensions in bpp
            attributeCreator.CreateDimensions(ref mappedDimensions);

            //// read units from csv file
            DataTable mappedUnits = mappingReader.readUnits(filePath);
            // create read units in bpp
            attributeCreator.CreateUnits(ref mappedUnits);

            //// read attributes from csv file
            DataTable mappedAttributes = mappingReader.readAttributes(filePath);
            // free memory
            mappedDataTypes.Clear();
            mappedDimensions.Clear();
            // create read attributes in bpp
            attributeCreator.CreateAttributes(ref mappedAttributes);

            createResearchPlan();
            //createSeedDataTypes();
            //createSIUnits();
            //createEmlDatasetAdv();
            //createABCD();


            //ImportSchema("Basic ABCD", "ABCD_2.06.XSD","Dataset","BExIS.Dlm.Entities.Data.Dataset");
            //ImportSchema("Basic Eml", "eml.xsd","dataset","BExIS.Dlm.Entities.Data.Dataset");

            DataStructureManager dsm = null;
            try
            {
                dsm = new DataStructureManager();
                foreach (StructuredDataStructure sds in dsm.StructuredDataStructureRepo.Get())
                {
                    DataStructureIO.convertOrder(sds);
                }
            }
            finally
            {
                dsm.Dispose();
            }
        }

        private static void createResearchPlan()
        {
            //ResearchPlan
            ResearchPlanManager rpm = null;
            try
            {
                rpm = new ResearchPlanManager();
                ResearchPlan researchPlan = rpm.Repo.Get(r => r.Title.Equals("Research plan")).FirstOrDefault();
                if (researchPlan == null) rpm.Create("Research plan", "");
            }
            finally
            {
                rpm.Dispose();
            }

        }

        public void Dispose()
        {
            // release all the resources
        }



    }
}