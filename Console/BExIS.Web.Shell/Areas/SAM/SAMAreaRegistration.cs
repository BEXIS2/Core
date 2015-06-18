using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Sam
{
    public class SAMAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SAM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SAM_default",
                "SAM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
