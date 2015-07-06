using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BExIS.Xml.Helpers;
using BExIS.Web.Shell.Areas.DCM.Models;
using System.Windows;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Vaiona.Utils.Cfg;
using System.IO;
using BExIS.IO.Transform.Input;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Areas", "DCM", "Views\\Help\\UserGuides\\DCM_UserGuide.htm");
            string imagePath = "/Areas/DCM/Images/";

            ShowHelpModel model = new ShowHelpModel();

            FileInfo f = new FileInfo(filePath);

            if (f.Exists)
            {
                WebClient Hdoc = new WebClient();
                string helpFile = Hdoc.DownloadString(filePath);

                // find the start of table of contents and main context
                string TC = "Start Contents";
                string CX = "Start Context";
                int startTC = helpFile.IndexOf(TC);
                int startCX = helpFile.IndexOf(CX);
                int i;

                if (startTC > 0 && startCX > 0)
                {
                    //add length of TC to i
                    for (i = startTC + 14; i < startCX; ++i)
                    {
                        model.Title = model.Title + helpFile[i];
                    }

                    //find length of main context
                    StreamReader reader = new StreamReader(filePath);
                    int endFile = (int)reader.BaseStream.Length;


                    string text = "";

                    for (i = startCX + 13; i < endFile; ++i)
                    {
                        text = text + helpFile[i];
                    }

                    //add the url folder of Images to all images to show
                    model.Description = text.Replace("src=\"", "src=\"" + imagePath);
                }
                else
                {
                    model.Description = "The help file is not indexed.";
                }
            }

            // if the file does not exist
            else
            {
                model.Description = "The help file is not found.";
            }

            return View(model);
        }
    }
}

