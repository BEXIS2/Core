using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.SAM.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString Brand(this HtmlHelper htmlHelper, string path, string name)
        {
            byte[] image = File.ReadAllBytes(path);
            string mime = MimeMapping.GetMimeMapping(path);
            

            return new MvcHtmlString(string.Format("<image src='data:{0};charset=utf-8;base64, {1}' alt='{2}' style='height: 40px; margin-top: -10px;' />", mime, Convert.ToBase64String(image), name));
        }
    }
}