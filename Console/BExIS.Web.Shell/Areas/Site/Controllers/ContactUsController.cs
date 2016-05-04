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
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.Site.Controllers
{
    public class ContactUsController : Controller
    {
        // GET: Site/ContactUs
        public ActionResult Index()
        {
            // This method of injecting content from an htm file to a view is problematic!!!
            // It is used here and in the impressum, privacy policy, and potentially help!
            // Talk to Javad to justify the reasons and possibly improve it. Javad 04.05.2016
            ViewBag.Title = PresentationModel.GetViewTitle("Contact Us");
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Areas", "Site", "Views\\ContactUs\\Contact_Us.htm");
            ContactUsModel model = new ContactUsModel();

            FileInfo f = new FileInfo(filePath);

            if (f.Exists)
            {
                WebClient Hdoc = new WebClient();
                string helpFile = Hdoc.DownloadString(filePath);

                string BPP = "Start:";

                int startBPP = helpFile.IndexOf(BPP) + 6;
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
                    model.content = "Information is not indexed.";
                }
            }

            // if the file does not exist
            else
            {
                model.content = "Any Contact is not found.";
            }

            return View(model);
        }
    }
}