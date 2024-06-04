using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace BExIS.Utils.WebHelpers
{
    public static partial class HtmlExtensions
    {
        //private const string urlRegEx = @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)";
        private const string urlRegEx =
            @"((?:(?:https?|ftp|file):\/\/|www\.|ftp\.)(?:\([-A-Z0-9+&@#/%=~_|$?!:,.]*\)|[-A-Z0-9+&@#/%=~_|$?!:,.])*(?:\([-A-Z0-9+&@#/%=~_|$?!:,.]*\)|[A-Z0-9+&@#/%=~_|$])|((doi):[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?))";

        public static MvcHtmlString DisplayWithLinksFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            string content = GetContent<TModel, TProperty>(htmlHelper, expression);
            string result = ReplaceUrlsWithLinks(content);
            return MvcHtmlString.Create(result);
        }

        private static string ReplaceUrlsWithLinks(string input)
        {
            Regex rx = new Regex(urlRegEx, RegexOptions.IgnoreCase);
            string result = rx.Replace(input, delegate (Match match)
            {
                string url = "";

                if (IsDoi(match.ToString()))
                {
                    string name = match.ToString();
                    url = ConvertDoiToDoiUrl(name);
                    return String.Format("<a href=\"{0}\" target=\"_blank\" >{1}</a>", url, name);
                }
                else
                {
                    url = match.ToString();
                    return String.Format("<a href=\"{0}\" target=\"_blank\" >{0}</a>", url);
                }
            });
            return result;
        }

        private static bool IsDoi(string url)
        {
            if (url.ToLower().StartsWith("doi")) return true;

            return false;
        }

        private static string ConvertDoiToDoiUrl(string doi)
        {
            //https://doi.org/10.1109/5.771073
            //doi: 10.15468 / 9gsaev

            string doiId = doi.Split(':').Last();

            return "https://doi.org/" + doiId;
        }

        private static string GetContent<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            Func<TModel, TProperty> func = expression.Compile();
            return func(htmlHelper.ViewData.Model).ToString();
        }
    }
}