using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Services.Administration;
using BExIS.Modules.Rpm.UI.Helpers.SeedData;
using BExIS.Security.Entities.Authorization;
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

                var dataPlanningFeature = featureManager.Find(f => f.Name == "Data Planning" && f.Parent == null).SingleOrDefault() ?? featureManager.Create("Data Planning", "Data Planning");

                var datastructureFeature = featureManager.Find(f => f.Name == "Datastructure Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Datastructure Management", "Datastructure Management", dataPlanningFeature);

                if (!operationManager.Exists("RPM", "DataStructureSearch", "*"))
                    operationManager.Create("RPM", "DataStructureSearch", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "DataStructureEdit", "*"))
                    operationManager.Create("RPM", "DataStructureEdit", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "Structures", "*"))
                    operationManager.Create("RPM", "Structures", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "DataStructureIO", "*"))
                    operationManager.Create("RPM", "DataStructureIO", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "DataStructure", "*")) operationManager.Create("RPM", "DataStructure", "*", datastructureFeature);

                var variableTemplatesFeature = featureManager.Find(f => f.Name == "Variable Templates Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Variable Templates Management", "Variable Templates Management", dataPlanningFeature);

                if (!operationManager.Exists("RPM", "DataAttribute", "*"))
                    operationManager.Create("RPM", "DataAttribute", "*", variableTemplatesFeature);

                if (!operationManager.Exists("RPM", "VariableTemplate", "*"))
                    operationManager.Create("RPM", "VariableTemplate", "*", variableTemplatesFeature);

                var unitsFeature = featureManager.Find(f => f.Name == "Units Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Units Management", "Units Management", dataPlanningFeature);

                if (!operationManager.Exists("RPM", "Unit", "*"))
                    operationManager.Create("RPM", "Unit", "*", unitsFeature);

                var dimensionsFeature = featureManager.Find(f => f.Name == "Dimensions Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Dimensions Management", "Dimensions Management", dataPlanningFeature);

                if (!operationManager.Exists("RPM", "Dimension", "*"))
                    operationManager.Create("RPM", "Dimension", "*", dimensionsFeature);

                var constraintsFeature = featureManager.Find(f => f.Name == "Constraints Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Constraints Management", "Constraints Management", dataPlanningFeature);

                if (!operationManager.Exists("RPM", "Constraints", "*"))
                    operationManager.Create("RPM", "Constraints", "*", constraintsFeature);

                var dataTypesFeature = featureManager.Find(f => f.Name == "Data Types Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Data Types Management", "Data Types Management", dataPlanningFeature);

                if (!operationManager.Exists("RPM", "Home", "*"))
                    operationManager.Create("RPM", "Home", "*", dataTypesFeature);

                if (!operationManager.Exists("RPM", "Help", "*"))
                    operationManager.Create("RPM", "Help", "*");

                var newDataTypesFeature = featureManager.Find(f => f.Name == "New Data Types Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("New Data Types Management", "New Data Types Management", dataPlanningFeature);

                //if (newDataTypeFeature == null)
                //    newDataTypeFeature = featureManager.Create("New Data Type Management", "New Data Type Management", dataPlanning);

                if (!operationManager.Exists("RPM", "DataType", "*"))
                    operationManager.Create("RPM", "DataType", "*", dataTypesFeature);

                var apiFeature = featureManager.Find(f => f.Name == "Api" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Api", "Api", dataPlanningFeature);

                if (!operationManager.Exists("API", "Structures", "*"))
                    operationManager.Create("API", "Structures", "*", apiFeature);

                //set api public
                FeaturePermission result_create;
                result_create = featurePermissionManager.Create(null, apiFeature.Id, PermissionType.Grant);

                //meanings features and security levels
                var dataMeaningsFeature = featureManager.Find(f => f.Name == "Meanings Management" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Meanings Management", "Meanings Management", dataPlanningFeature);

                if (!operationManager.Exists("Meaning", "MeaningsAdmin", "*"))
                    operationManager.Create("Meaning", "MeaningsAdmin", "*", dataMeaningsFeature);

                var dataMeaningsPublicFeature = featureManager.Find(f => f.Name == "Meanings Api" && f.Parent == dataPlanningFeature).SingleOrDefault() ?? featureManager.Create("Meanings Api", "Meanings Api Management", dataPlanningFeature);

                if (!operationManager.Exists("API", "Meanings", "*"))
                {
                    operationManager.Create("API", "Meanings", "*", dataMeaningsPublicFeature);
                    result_create = featurePermissionManager.Create(null, dataMeaningsPublicFeature.Id, PermissionType.Grant);
                }

                if (!operationManager.Exists("RPM", "Meaning", "*"))
                {
                    operationManager.Create("RPM", "Meaning", "*", dataMeaningsFeature);
                }

                if (!operationManager.Exists("RPM", "ExternalLink", "*"))
                {
                    operationManager.Create("RPM", "ExternalLink", "*", dataMeaningsFeature);
                }

                if (!operationManager.Exists("RPM", "Help", "*")) operationManager.Create("RPM", "Help", "*");

                if (!operationManager.Exists("RPM", "File", "*")) operationManager.Create("RPM", "File", "*");
            }
            catch
            {
                throw;
            }

            #endregion security

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