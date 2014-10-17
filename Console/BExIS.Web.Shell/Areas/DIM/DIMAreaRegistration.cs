using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DIM
{
    public class DIMAreaRegistration:AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DIM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DIM_default",
                "DIM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
}
    }