using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.RPM
{
    public class MMMAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MMM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "MMM_default",
                "MMM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}