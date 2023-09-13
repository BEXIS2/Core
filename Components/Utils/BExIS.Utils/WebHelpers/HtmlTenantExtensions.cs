using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace BExIS.Utils.WebHelpers
{
    public static class HtmlTenantExtensions
    {
        public static MvcHtmlString Brand(this HtmlHelper htmlHelper, string path, string name, int height = 40)
        {
            byte[] image = File.ReadAllBytes(path);
            string mime = MimeMapping.GetMimeMapping(path);


            return new MvcHtmlString($"<image src='data:{mime};charset=utf-8;base64, {Convert.ToBase64String(image)}' alt='{name}' style='height: {height}px; margin-top: -10px;' />");
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

            return new MvcHtmlString($"<link rel='icon' href='data:{mime};charset=utf-8;base64, {Convert.ToBase64String(image)}'/>");
        }

        public static MvcHtmlString RenderHtml(this HtmlHelper htmlHelper, string path)
        {
            var reader = new StreamReader(path);
            var contents = reader.ReadToEnd();
            reader.Close();

            return new MvcHtmlString(contents);
        }

        public static MvcHtmlString RenderExtendedMenu(this HtmlHelper htmlHelper, XElement xElement)
        {
            string menu = $"<a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'>{xElement.Attribute("label").Value}<span class='caret'></span></a>";

            string submenu = $"";
            foreach (var child in xElement.Elements())
            {
                submenu += $"<li><a href='{child.Attribute("url").Value}' target='{child.Attribute("target").Value}'>{child.Attribute("label").Value}</a></li>";
            }

            return new MvcHtmlString("<li class='dropdown'>" + menu + "<ul class='dropdown-menu'>" + submenu + "</ul></li>");
        }
    }

}
