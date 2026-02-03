using BExIS.App.Bootstrap.Attributes;
using BExIS.UI.Helpers;
using Newtonsoft.Json;
using System.IO;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;


namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ComponentConfigController : Controller
    {
        public ActionResult Index(long id = 0)
        {
            string module = "DCM";
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;
            return View();
        }


        [BExISApiAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public bool SavConfige(Data data)
        {

            string filename = data.Id + "_" + data.Type + ".json";

            string directory = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ComponentConfig");
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
        public JsonResult LoadConfig(string id, string type)
        {

            string filename = id + "_" + type + ".json";

            string directory = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ComponentConfig");
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
            public string Id { get; set; }
            public string Content { get; set; }
            public string Type { get; set; }

            public Data()
            {
                Id = "";
                Content = "";
                Type = "";
            }
        }

    }
}