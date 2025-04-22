namespace BExIS.Utils.Helpers
{
    public class ManualHelper
    {
        public static string GetUrl(string url)
        {
            string docsUrl = "/home/docs/";

            // 3 different usecases
            if (string.IsNullOrEmpty(url)) return docsUrl+"general";

            //2. URL is set and no url -> generate link to internal documentation

            if (url.Contains("http")) return url;


            //3. empty URL -> generate link to internal documentation
            return docsUrl+url;
        }
    }
}