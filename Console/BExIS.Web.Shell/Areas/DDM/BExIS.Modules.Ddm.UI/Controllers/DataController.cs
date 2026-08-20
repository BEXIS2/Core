using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Vaiona.Web.Mvc;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : BaseController
    {
        public ActionResult ShowData(long id, int version = 0, bool asPartial = false, string versionName = "", double tag = 0)
        {

            string url = $"/dcm/view/?id={id}";
            var queryParams = new List<string>();
            if (version > 0) queryParams.Add($"version={version}");
            if (!string.IsNullOrEmpty(versionName)) queryParams.Add($"versionName={Uri.EscapeDataString(versionName)}");
            if (tag > 0) queryParams.Add($"tag={tag}");
            if (queryParams.Count > 0) url += "&" + string.Join("&", queryParams);
            return Redirect(url);
        }
    }
}

         