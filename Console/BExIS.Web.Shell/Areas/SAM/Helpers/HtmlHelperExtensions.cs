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
            

            return new MvcHtmlString($"<image src='data:{mime};charset=utf-8;base64, {Convert.ToBase64String(image)}' alt='{name}' style='height: 40px; margin-top: -10px;' />");
        }

        public static MvcHtmlString Logo(this HtmlHelper htmlHelper, string path)
        {
            byte[] image = File.ReadAllBytes(path);
            string mime = MimeMapping.GetMimeMapping(path);


            return new MvcHtmlString($"<image src='data:{mime};charset=utf-8;base64, {Convert.ToBase64String(image)}' style='width: 300px;' />");
        }

        public static MvcHtmlString Favicon(this HtmlHelper htmlHelper, string path)
        {
            byte[] image = File.ReadAllBytes(path);
            string mime = MimeMapping.GetMimeMapping(path);

            return new MvcHtmlString($"<link rel='shortcut icon' href='data:{mime};charset=utf-8;base64, {Convert.ToBase64String(image)}'/>");
        }

        public static MvcHtmlString RenderHtml(this HtmlHelper htmlHelper, string path)
        {
            var reader = new StreamReader(path);
            var contents = reader.ReadToEnd();
            reader.Close();

            return new MvcHtmlString(contents);
        }
    }
}