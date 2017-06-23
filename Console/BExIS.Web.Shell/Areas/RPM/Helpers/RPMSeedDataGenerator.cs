using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Services.Administration;
using BExIS.Web.Shell.Areas.RPM.Helpers.SeedData;
using System.Data;
using System.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.RPM.Helpers
{
    public class RPMSeedDataGenerator
    {

        public static void GenerateSeedData()
        {
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
        }

        private static void createResearchPlan()
        {
            //ResearchPlan
            ResearchPlanManager rpm = new ResearchPlanManager();
            ResearchPlan researchPlan = rpm.Repo.Get(r => r.Title.Equals("Research plan")).FirstOrDefault();
            if (researchPlan == null) rpm.Create("Research plan", "");

        }

    }
}