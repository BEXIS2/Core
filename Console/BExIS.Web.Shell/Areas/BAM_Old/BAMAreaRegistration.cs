using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.BAM
{
    public class BAMAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BAM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BAM_default",
                "BAM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
            // Javad 09.05.2017, this should be changed to the new modualirty mechanism
            // also all other modules' seed data generation
            //if (AppConfiguration.CreateDatabase) BAM.Helpers.BAMSeedDataGenerator.GenerateSeedData();
        }
    }
}