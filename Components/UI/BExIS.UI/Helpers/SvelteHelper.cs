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
        public static string GetPageScript(string module, string pageId)
        {
            string appDomain = AppDomain.CurrentDomain.BaseDirectory;
            string svelteBuildPath = "/Areas/"+module+"/BExIS.Modules."+ module + ".UI/Scripts/svelte/";
            string manifestJson = "vite-manifest.json";
            string key = "src/routes/"+pageId+"/+page.svelte";

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
