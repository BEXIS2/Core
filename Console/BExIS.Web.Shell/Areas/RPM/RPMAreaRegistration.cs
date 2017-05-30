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

            // BUG: There is something going wrong inside the seed data creation of RPM. See bug report #1472
            // WORKAROUND: The only solution right now is to comment the seed data creation. But it could causes other problems
            // after the init of BEXIS2 because other modules probably rely on RPM seed data.
            //if (AppConfiguration.CreateDatabase) RPM.Helpers.RPMSeedDataGenerator.GenerateSeedData();
            // TODO: refactor
            // BUG:
            //if (AppConfiguration.CreateDatabase) RPM.Helpers.RPMSeedDataGenerator.GenerateSeedData();

            // manage Seed data
            string seedDataOption = System.Configuration.ConfigurationManager.AppSettings["UpdateSeedData"];
            if (seedDataOption.ToLower() == "true" && !AppConfiguration.CreateDatabase)
            {
                RPM.Helpers.RPMSeedDataGenerator.GenerateSeedData();
            }
        }
    }
}