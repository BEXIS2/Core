using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Meanings;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public void GenerateSeedData()
        {
            #region security
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

                if (!operationManager.Exists("RPM", "DataStructure", "*"))  operationManager.Create("RPM", "DataStructure", "*", datastructureFeature);

                Feature atributeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Variables Template Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (atributeFeature == null)
                    atributeFeature = featureManager.Create("Variables Template Management", "Variables Template Management", dataPlanning); ;

                if (!operationManager.Exists("RPM", "DataAttribute", "*"))
                    operationManager.Create("RPM", "DataAttribute", "*", atributeFeature);

                if (!operationManager.Exists("RPM", "VariableTemplate", "*"))
                    operationManager.Create("RPM", "VariableTemplate", "*", atributeFeature);

                Feature unitFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Unit Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (unitFeature == null)
                    unitFeature = featureManager.Create("Unit Management", "Unit Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Unit", "*"))
                    operationManager.Create("RPM", "Unit", "*", unitFeature);

                Feature dimensionFeature = features.FirstOrDefault(f =>
                   f.Name.Equals("Dimension Management") &&
                   f.Parent != null &&
                   f.Parent.Id.Equals(dataPlanning.Id));

                if (dimensionFeature == null)
                    dimensionFeature = featureManager.Create("Dimension Management", "Dimension Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Dimension", "*"))
                    operationManager.Create("RPM", "Dimension", "*", dimensionFeature);

                Feature constraintsFeature = features.FirstOrDefault(f =>
                   f.Name.Equals("Constraint Management") &&
                   f.Parent != null &&
                   f.Parent.Id.Equals(dataPlanning.Id));

                if (constraintsFeature == null)
                    constraintsFeature = featureManager.Create("Constraint Management", "Constraint Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Constraints", "*"))
                    operationManager.Create("RPM", "Constraints", "*", constraintsFeature);

                Feature dataTypeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Data Type Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (dataTypeFeature == null)
                    dataTypeFeature = featureManager.Create("Data Type Management", "Data Type Management", dataPlanning);;

                if (!operationManager.Exists("RPM", "Home", "*"))
                    operationManager.Create("RPM", "Home", "*", dataTypeFeature);

                if (!operationManager.Exists("RPM", "Help", "*"))
                    operationManager.Create("RPM", "Help", "*");


                Feature newDataTypeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("New Data Type Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (newDataTypeFeature == null)
                    newDataTypeFeature = featureManager.Create("New Data Type Management", "New Data Type Management", dataPlanning);

                if (!operationManager.Exists("RPM", "DataType", "*"))
                    operationManager.Create("RPM", "DataType", "*", newDataTypeFeature);




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


                //meanings features and security levels
                Feature dataMeaning = features.FirstOrDefault(f =>
                    f.Name.Equals("Data Meaning Manager") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));
                if (dataMeaning == null)
                    dataMeaning = featureManager.Create("Data Meaning", "Data Meaning Management", dataPlanning);
                if (!operationManager.Exists("API", "MeaningsAdmin", "*"))
                {
                    operationManager.Create("API", "MeaningsAdmin", "*", dataMeaning);
                }

                Feature dataMeaning_pub = features.FirstOrDefault(f =>
                    f.Name.Equals("Data Meaning (public)") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));
                if (dataMeaning_pub == null)
                    dataMeaning_pub = featureManager.Create("Data Meaning (public)", "Data Meaning Management", dataPlanning);
                if (!operationManager.Exists("API", "Meanings", "*"))
                {
                    operationManager.Create("API", "Meanings", "*", dataMeaning_pub);
                    featurePermissionManager.Create(null, dataMeaning_pub.Id, Security.Entities.Authorization.PermissionType.Grant);
                }

                if (!operationManager.Exists("RPM", "Meaning", "*"))
                {
                    operationManager.Create("RPM", "Meaning", "*", dataMeaning_pub);
                    operationManager.Create("RPM", "ExternalLink", "*", dataMeaning_pub);

                }

                if (!operationManager.Exists("RPM", "Help", "*"))operationManager.Create("RPM", "Help", "*");

            }
            finally
            {
                featureManager.Dispose();
                featurePermissionManager.Dispose();
                operationManager.Dispose();
            }

            #endregion

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
            DataTable mappedTemplates = mappingReader.readAttributes(filePath);

            // free memory
            mappedDataTypes.Clear();
            mappedDimensions.Clear();
            // create read attributes in bpp
            attributeCreator.CreateTemplates(ref mappedTemplates);

            createResearchPlan();

            // Add seeddata or Meanings and Externel Links Maybe

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