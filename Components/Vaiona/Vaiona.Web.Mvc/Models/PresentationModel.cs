using BExIS.Utils.Config;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Vaiona.Model.MTnt;

namespace Vaiona.Web.Mvc.Models
{
    public class PresentationModel : Dictionary<string, object>
    {
        private string viewTitle = string.Empty;

        internal string ViewTitle // needs a proper design. internal for the time being...
        {
            get
            {
                return viewTitle;
            }
            set
            {
                viewTitle = value;
            }
        }

        public static string GetGenericViewTitle(string viewTitle)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(viewTitle));

            string appInfo = GeneralSettings.ApplicationInfo;
            if (!string.IsNullOrWhiteSpace(appInfo))
            {
                return string.Format("{0} - {1}", appInfo, viewTitle);
            }
            return viewTitle;
        }

        public static string GetViewTitleForTenant(string viewTitle, Tenant tenant)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(viewTitle));
            Contract.Requires(tenant != null);

            string appInfo = tenant.ShortName;
            if (!string.IsNullOrWhiteSpace(appInfo))
            {
                return string.Format("{0} - {1}", appInfo, viewTitle);
            }
            return viewTitle;
        }
    }
}