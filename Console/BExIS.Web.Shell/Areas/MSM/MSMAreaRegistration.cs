using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.MSM
{
    public class MSMAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MSM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "MSM_default",
                "MSM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
