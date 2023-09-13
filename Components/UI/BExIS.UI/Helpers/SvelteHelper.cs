using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Helpers
{
    public class SvelteHelper
    {
        public static string GetApp(string module)
        {
            return getScript(module, ".svelte-kit/generated/client-optimized/app.js");
        }

        public static string GetAppPath(string module)
        {
            string appDomain = AppDomain.CurrentDomain.BaseDirectory;
            string svelteBuildPath = "/Areas/" + module + "/BExIS.Modules." + module + ".UI/Scripts/svelte";

            return svelteBuildPath;
        }

        public static string GetLayoutCss(string module)
        {
            return getCss(module, "src/routes/+layout.css");
        }

        public static string GetLayoutScript(string moduleId)
        {
            return getScript(moduleId, "src/routes/+layout.js");
        }

        public static string GetLayoutSvelteScript(string moduleId)
        {
            return getScript(moduleId, "src/routes/+layout.svelte");
        }

        public static string GetPageCss(string module, string pageId)
        {
            string appDomain = AppDomain.CurrentDomain.BaseDirectory;
            string svelteBuildPath = "/Areas/" + module + "/BExIS.Modules." + module + ".UI/Scripts/svelte/";
            string manifestJson = "vite-manifest.json";
            string key = "src/routes/" + pageId + "/+page.css";

            string manifestJsonPath = appDomain + svelteBuildPath + manifestJson;

            using (StreamReader r = new StreamReader(manifestJsonPath))
            {
                string json = r.ReadToEnd();
                Dictionary<string, Dictionary<string, object>> manifest = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
                if (manifest.ContainsKey(key))
                {
                    var page = manifest[key];

                    string pagehash = page["file"].ToString();//"_page.svelte-2b7e1fbb.js";

                    return svelteBuildPath + pagehash;
                }
            }

            return "";
        }

        public static string GetPageScript(string module, string pageId)
        {
            string appDomain = AppDomain.CurrentDomain.BaseDirectory;
            string svelteBuildPath = "/Areas/" + module + "/BExIS.Modules." + module + ".UI/Scripts/svelte/";
            string manifestJson = "vite-manifest.json";
            string key = "src/routes/" + pageId + "/+page.svelte";

            string manifestJsonPath = appDomain + svelteBuildPath + manifestJson;

            using (StreamReader r = new StreamReader(manifestJsonPath))
            {
                string json = r.ReadToEnd();
                Dictionary<string, Dictionary<string, object>> manifest = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                var page = manifest[key];

                string pagehash = page["file"].ToString();//"_page.svelte-2b7e1fbb.js";

                return svelteBuildPath + pagehash;
            }

            return "";
        }

        public static string GetStart(string module)
        {
            return getScript(module, "node_modules/@sveltejs/kit/src/runtime/client/start.js");
        }

        private static string getCss(string module, string key)
        {
            string appDomain = AppDomain.CurrentDomain.BaseDirectory;
            string svelteBuildPath = "/Areas/" + module + "/BExIS.Modules." + module + ".UI/Scripts/svelte/";
            string manifestJson = "vite-manifest.json";
            //string key = "src/routes/+layout.js";

            string manifestJsonPath = appDomain + svelteBuildPath + manifestJson;

            using (StreamReader r = new StreamReader(manifestJsonPath))
            {
                string json = r.ReadToEnd();
                Dictionary<string, Dictionary<string, object>> manifest = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                var page = manifest[key];

                string pagehash = page["file"].ToString();//"_page.svelte-2b7e1fbb.js";

                return svelteBuildPath + pagehash;
            }

            return "";
        }

        private static string getScript(string module, string key)
        {
            string appDomain = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
            string debugPathAdditionals = (string.IsNullOrEmpty(module) || module.ToLower() == "shell") ? "/BExIS.Web.Shell" : "/BExIS.Modules." + module + ".UI";
#endif

            string svelteBuildPath = (string.IsNullOrEmpty(module) || module.ToLower() == "shell") ? "/Scripts/svelte/" : $"/Areas/{module}{debugPathAdditionals}/Scripts/svelte/";
            string manifestJson = "vite-manifest.json";
            //string key = "src/routes/" + pageId + "/+page.svelte";

            string manifestJsonPath = appDomain + svelteBuildPath + manifestJson;

            using (StreamReader r = new StreamReader(manifestJsonPath))
            {
                string json = r.ReadToEnd();
                Dictionary<string, Dictionary<string, object>> manifest = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                var page = manifest[key];

                string pagehash = page["file"].ToString();//"_page.svelte-2b7e1fbb.js";

                return svelteBuildPath + pagehash;
            }

            return "";
        }
    }
}