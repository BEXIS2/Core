using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Vaiona.Web.Mvc
{
    public static class TemplateHelper
    {
        public static void RenderClientTemplate(this HtmlHelper helper, Type type, object parameter, string _partialViewName)
        {
            object model = Activator.CreateInstance(type, parameter);
            helper.RenderPartial(_partialViewName, model);
        }
    }
}