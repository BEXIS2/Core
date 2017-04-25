using System.Configuration;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.RPM
{
    public class RPMAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "RPM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "RPM_default",
                "RPM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            // TODO: refactor
            // BUG:
            //if (AppConfiguration.CreateDatabase) RPM.Helpers.RPMSeedDataGenerator.GenerateSeedData();

            // manage Seed data
            string seedDataOption = ConfigurationManager.AppSettings["UpdateSeedData"];
            if (seedDataOption.ToLower() == "true" && !AppConfiguration.CreateDatabase)
            {
                RPM.Helpers.RPMSeedDataGenerator.GenerateSeedData();
            }
        }
    }
}