using System.Web.Mvc;

namespace BExIS.Web.Shell.Helpers
{

    public class CustomViewEngine : RazorViewEngine
    {
        public CustomViewEngine()
        {
            var viewLocations = new[] {

            "~/Areas/{2}/UI/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/UI/Views/Shared/{0}.cshtml",
            "~/Areas/{2}/BEXIS.Modules.{2}.UI/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/BEXIS.Modules.{2}.UI/Views/Shared/{0}.cshtml",

            // etc
        };

            this.AreaMasterLocationFormats = viewLocations;
            this.AreaPartialViewLocationFormats = viewLocations;
            this.AreaViewLocationFormats = viewLocations;
        }
    }
}
