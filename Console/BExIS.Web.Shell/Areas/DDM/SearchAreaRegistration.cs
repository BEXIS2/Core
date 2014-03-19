using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DDM
{
    public class SearchAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DDM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Search_default",
                "DDM/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
