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
           if (AppConfiguration.CreateDatabase)
                Helpers.BAMSeedDataGenerator.GenerateSeedData();
        }
    }
}