using System.Web.Mvc;

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
        }
    }
}
