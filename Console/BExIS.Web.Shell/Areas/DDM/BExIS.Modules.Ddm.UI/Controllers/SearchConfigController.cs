using BExIS.App.Bootstrap.Attributes;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.UI.Helpers;
using Newtonsoft.Json;
using System.IO;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class SearchConfigController : Controller
    {
        public ActionResult Index(long id = 0)
        {
            string module = "DDM";
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;
            return View();
        }


        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetMetadataNodes()
        {
            // get BExIS.Ddm.Providers.LuceneProvider.GetMetadataNodes()
            var provider = new SearchDesigner();
            var metadataNode = provider.GetMetadataNodes();

            return Json(metadataNode, JsonRequestBehavior.AllowGet);

        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public bool SaveConfig(Data data)
        {

            string filename = "searchConfig.json";

            string directory = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "SearchConfig");
            // combine datapath + path + filename
            string filepath = Path.Combine(directory, filename);

            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath); // check if file exist, delete maybe? }
            }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory); // create directory if not exist

            System.IO.File.WriteAllText(filepath, data.Content);

            return true;
        }

        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult LoadConfig()
        {

            string filename = "searchConfig.json";

            string directory = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "SearchConfig");
            // combine datapath + path + filename
            string filepath = Path.Combine(directory, filename);

            // if file not exist return empty data
            if (!System.IO.File.Exists(filepath))
            {
                return Json(new Data(), JsonRequestBehavior.AllowGet);
            }

            var content = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(filepath));

            return Json(content, JsonRequestBehavior.AllowGet);
        }

        public class Data
        {

            public string Content { get; set; }


            public Data()
            {
                Content = "";

            }
        }


    }

}