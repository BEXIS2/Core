using BExIS.Dlm.Services.Party;
using BExIS.Modules.Ato.UI.Models;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Ato.UI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
  
            return View();
        }

        public ActionResult Information()
        {
            InformationModel model = new InformationModel();
            model.CategoryModels = load();


            return View(model);
        }

        public ActionResult Calendar()
        {
            return View("ExternalCalendar");
        }

        public ActionResult Download(string fileName)
        {
            string dataPath = AppConfiguration.DataPath;
            string addPath = @"ATO\Documents";

            string path = Path.Combine(dataPath, addPath, fileName);
      
            string mimeType = MimeMapping.GetMimeMapping(fileName);

            return File(path, mimeType);
        }

        private List<CategoryModel> load()
        {
            List<CategoryModel> ModelList = new List<CategoryModel>();

            string dataPath = AppConfiguration.GetModuleWorkspacePath("ATO");
            string fileName = "Documentation.xml";
            string filePath = Path.Combine(dataPath, fileName);

            XmlDocument documentationXml = new XmlDocument();
            documentationXml.Load(filePath);

            if (documentationXml.DocumentElement != null && documentationXml.DocumentElement.HasChildNodes)
            {
                foreach (XmlNode node in documentationXml.DocumentElement.ChildNodes)
                {
                    XmlElement cElement = (XmlElement)node;
                    ModelList.Add(convert2CategoryModel(cElement));
                }
            }

            return ModelList;
        }

        private CategoryModel convert2CategoryModel(XmlElement element)
        {
            CategoryModel temp = new CategoryModel();

            if (element == null) return temp;

            foreach (XmlNode c in element.ChildNodes)
            {
                XmlElement cElement = (XmlElement)c;
                if (cElement != null)
                {
                    if (cElement.LocalName == "name")
                    {
                        temp.Name = cElement.InnerText;
                    }

                    if (cElement.LocalName == "description")
                    {
                        temp.Description = cElement.InnerText;
                    }

                    if (cElement.LocalName == "files")
                    {
                        if (cElement.HasChildNodes)
                        {
                            foreach (var child in cElement.ChildNodes)
                            {
                                XmlElement childElement = (XmlElement)child;
                                if (childElement != null)
                                {

                                    FileModel tmpFileModel = convert2FileModel(childElement);

                                    temp.FileModels.Add(tmpFileModel);
                                }
                            }

                        }
                    }
                }
            }

            return temp;

        }

        private FileModel convert2FileModel(XmlElement element)
        {
            FileModel temp = new FileModel();

            if (element == null) return temp;

            if (element.HasAttribute("header"))
            {
                temp.Header = element.Attributes["header"].Value;
            }
                if (element.HasAttribute("infotext"))
            {
                temp.InfoText = element.Attributes["infotext"].Value;
            }
            if (element.HasAttribute("name"))
            {
                temp.Name = element.Attributes["name"].Value;
            }
            if (element.HasAttribute("filename"))
            {
             temp.FileName = element.Attributes["filename"].Value;
            }
            return temp;

        }

        public ActionResult UserOverview(string gname)
        {
            UserManager userManager = new UserManager();
            GroupManager groupManager = new GroupManager();
            PartyManager partyManager = new PartyManager();

            var group = groupManager.Groups.Where(g => g.Name.Equals(gname)).FirstOrDefault();

            Debug.WriteLine("*****************USERS*********************");
            StringBuilder sb = new StringBuilder("User overview <br>");

            foreach (var user in group.Users)
            {
                var x = partyManager.GetPartyByUser(user.Id);
                var institute = x.CustomAttributeValues.Where(c => c.CustomAttribute.DisplayName.Equals("Institute")).FirstOrDefault();
                var fname = x.CustomAttributeValues.Where(c => c.CustomAttribute.DisplayName.Equals("FirstName")).FirstOrDefault();
                var lname = x.CustomAttributeValues.Where(c => c.CustomAttribute.DisplayName.Equals("LastName")).FirstOrDefault();

                var message = String.Format("Name = {0} {1}, Email = {2}, Insitute = {3} <br>", fname.Value,lname.Value , user.Email, institute.Value);
                Debug.WriteLine(message);
                sb.AppendLine(message);
            }

            Debug.WriteLine("*****************End*********************");
            var es = new EmailService();
            //send email
            es.Send("Nutzer Übersicht",
                sb.ToString()
                ,
                ConfigurationManager.AppSettings["SystemEmail"]);


            return View("Index");
        }
    }
}