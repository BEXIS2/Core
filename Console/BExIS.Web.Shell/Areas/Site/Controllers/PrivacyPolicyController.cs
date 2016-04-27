using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
using System.IO;
using BExIS.IO.Transform.Input;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using BExIS.Web.Shell.Areas.Site.Models;

namespace BExIS.Web.Shell.Areas.Site.Controllers
{
    public class PrivacyPolicyController : Controller
    {
        // GET: Site/PrivacyPolicy
        public ActionResult Index()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Areas", "Site", "Views\\PrivacyPolicy\\Bexis_Privacy_Policy.htm");
            PrivacyModel model = new PrivacyModel();

            FileInfo f = new FileInfo(filePath);

            if (f.Exists)
            {
                WebClient Hdoc = new WebClient();
                string helpFile = Hdoc.DownloadString(filePath);

                string BPP = "Start:";

                int startBPP = helpFile.IndexOf(BPP)+6;
                int i;

                //find length of main context
                StreamReader reader = new StreamReader(filePath);
                int endFile = (int)reader.BaseStream.Length;

                if (startBPP > 0)
                {
                    //add length of TC to i
                    for (i = startBPP; i < endFile; ++i)
                    {
                        model.content = model.content + helpFile[i];
                    }
                }
                else
                {
                    model.content = "The Privacy policy is not indexed.";
                }
            }

            // if the file does not exist
            else
            {
                model.content = "The Privacy policy is not found.";
            }

            return View(model);
        }

    }
}