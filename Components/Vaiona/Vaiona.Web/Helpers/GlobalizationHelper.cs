using System;
using System.Globalization;
using System.Threading;
using System.Web;

namespace Vaiona.Web.Helpers
{
    public class GlobalizationHelper
    {
        public static void SetSessionCulture(CultureInfo culture)
        {
            if (culture.Name.Equals("fa-IR", StringComparison.InvariantCultureIgnoreCase))
            {
                // perform calendar and etc settings
            }
            HttpContext.Current.Session["SessionCulture"] = culture;
            // check for correctness and completeness
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static CultureInfo GetSessionCulture()
        {
            return (HttpContext.Current.Session["SessionCulture"] as CultureInfo);
        }

        public static CultureInfo GetCurrentCulture()
        {
            return (Thread.CurrentThread.CurrentCulture);
        }
    }
}