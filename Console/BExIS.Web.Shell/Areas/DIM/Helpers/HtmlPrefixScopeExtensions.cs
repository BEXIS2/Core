using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public static class HtmlPrefixScopeExtensions
    {
        private const string IdsToReuseKey = "__htmlPrefixScopeExtensions_IdsToReuse_";

        public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName)
        {
            return BeginCollectionItem(html, collectionName, html.ViewContext.Writer);
        }

        public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName, TextWriter writer)
        {
            /*
             * added Nested collection support for newly added collection items
             * as per this http://stackoverflow.com/questions/33916004/nested-list-of-lists-with-begincollectionitem
             * and this http://www.joe-stevens.com/2011/06/06/editing-and-binding-nested-lists-with-asp-net-mvc-2/
            */
            if (html.ViewData["ContainerPrefix"] != null)
                collectionName = string.Concat(html.ViewData["ContainerPrefix"], ".", collectionName);

            var idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
            var itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : Guid.NewGuid().ToString();

            string htmlFieldPrefix = $"{collectionName}[{itemIndex}]";
            html.ViewData["ContainerPrefix"] = htmlFieldPrefix;

            /*
             * html.Name(); has been removed
             * because of incorrect naming of collection items
             * e.g.
             * let collectionName = "Collection"
             * the first item's name was Collection[0].Collection[<GUID>]
             * instead of Collection[<GUID>]
             */
            string indexInputName = $"{collectionName}.index";

            // autocomplete="off" is needed to work around a very annoying Chrome behaviour
            // whereby it reuses old values after the user clicks "Back", which causes the
            // xyz.index and xyz[...] values to get out of sync.
            writer.WriteLine($@"<input type=""hidden"" name=""{indexInputName}"" autocomplete=""off"" value=""{html.Encode(itemIndex)}"" />");

            return BeginHtmlFieldPrefixScope(html, htmlFieldPrefix);
        }

        public static IDisposable BeginHtmlFieldPrefixScope(this HtmlHelper html, string htmlFieldPrefix)
        {
            return new HtmlFieldPrefixScope(html.ViewData.TemplateInfo, htmlFieldPrefix);
        }

        //private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
        //{
        //    // We need to use the same sequence of IDs following a server-side validation failure,
        //    // otherwise the framework won't render the validation error messages next to each item.
        //    var key = IdsToReuseKey + collectionName;
        //    var queue = (Queue<string>)httpContext.Items[key];
        //    if (queue == null)
        //    {
        //        httpContext.Items[key] = queue = new Queue<string>();

        //        if (httpContext.Request.HttpMethod == "POST")
        //        {
        //            var previouslyUsedIds = httpContext.Request.Form[collectionName + ".index"];
        //            if (!string.IsNullOrEmpty(previouslyUsedIds))
        //                foreach (var previouslyUsedId in previouslyUsedIds)
        //                    queue.Enqueue(previouslyUsedId);
        //        }
        //    }
        //    return queue;
        //}

        private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
        {
            // We need to use the same sequence of IDs following a server-side validation failure,
            // otherwise the framework won't render the validation error messages next to each item.
            string key = IdsToReuseKey + collectionName;
            var queue = (Queue<string>)httpContext.Items[key];
            if (queue == null)
            {
                httpContext.Items[key] = queue = new Queue<string>();
                var previouslyUsedIds = httpContext.Request[collectionName + ".index"];
                if (!string.IsNullOrEmpty(previouslyUsedIds))
                    foreach (string previouslyUsedId in previouslyUsedIds.Split(','))
                        queue.Enqueue(previouslyUsedId);
            }
            return queue;
        }

        internal class HtmlFieldPrefixScope : IDisposable
        {
            internal readonly TemplateInfo TemplateInfo;
            internal readonly string PreviousHtmlFieldPrefix;

            public HtmlFieldPrefixScope(TemplateInfo templateInfo, string htmlFieldPrefix)
            {
                TemplateInfo = templateInfo;

                PreviousHtmlFieldPrefix = TemplateInfo.HtmlFieldPrefix;
                TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            }

            public void Dispose()
            {
                TemplateInfo.HtmlFieldPrefix = PreviousHtmlFieldPrefix;
            }
        }
    }

    //public static class HtmlPrefixScopeExtensions
    //{
    //    private const string idsToReuseKey = "__htmlPrefixScopeExtensions_IdsToReuse_";

    //    public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName)
    //    {
    //        var htmlFieldPrefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;
    //        if (htmlFieldPrefix.Contains(collectionName))
    //        {
    //            collectionName = htmlFieldPrefix.Substring(0, htmlFieldPrefix.LastIndexOf(collectionName) + collectionName.Length);
    //        }

    //        var idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
    //        string itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : Guid.NewGuid().ToString();

    //        // autocomplete="off" is needed to work around a very annoying Chrome behaviour whereby it reuses old values after the user clicks "Back", which causes the xyz.index and xyz[...] values to get out of sync.
    //        html.ViewContext.Writer.WriteLine(string.Format("<input type=\"hidden\" name=\"{0}.index\" autocomplete=\"off\" value=\"{1}\" />", collectionName, html.Encode(itemIndex)));

    //        return BeginHtmlFieldPrefixScope(html, string.Format("{0}[{1}]", collectionName, itemIndex));
    //    }

    //    public static IDisposable BeginHtmlFieldPrefixScope(this HtmlHelper html, string htmlFieldPrefix)
    //    {
    //        return new HtmlFieldPrefixScope(html.ViewData.TemplateInfo, htmlFieldPrefix);
    //    }

    //    private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
    //    {
    //        // We need to use the same sequence of IDs following a server-side validation failure,
    //        // otherwise the framework won't render the validation error messages next to each item.
    //        string key = idsToReuseKey + collectionName;
    //        var queue = (Queue<string>)httpContext.Items[key];
    //        if (queue == null)
    //        {
    //            httpContext.Items[key] = queue = new Queue<string>();
    //            var previouslyUsedIds = httpContext.Request[collectionName + ".index"];
    //            if (!string.IsNullOrEmpty(previouslyUsedIds))
    //                foreach (string previouslyUsedId in previouslyUsedIds.Split(','))
    //                    queue.Enqueue(previouslyUsedId);
    //        }
    //        return queue;
    //    }

    //    private class HtmlFieldPrefixScope : IDisposable
    //    {
    //        private readonly TemplateInfo templateInfo;
    //        private readonly string previousHtmlFieldPrefix;

    //        public HtmlFieldPrefixScope(TemplateInfo templateInfo, string htmlFieldPrefix)
    //        {
    //            this.templateInfo = templateInfo;

    //            previousHtmlFieldPrefix = templateInfo.HtmlFieldPrefix;
    //            templateInfo.HtmlFieldPrefix = htmlFieldPrefix;
    //        }

    //        public void Dispose()
    //        {
    //            templateInfo.HtmlFieldPrefix = previousHtmlFieldPrefix;
    //        }
    //    }
    //}
}