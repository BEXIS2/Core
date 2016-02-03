using BExIS.Web.Shell.Areas.MSM.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc.UI;

namespace BExIS.Web.Shell.Areas.MSM.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /MSM/Test/

        public ActionResult Index() {
            TestModel model = new TestModel();
           
            //Debug.Write(getXMLAsString(xmldoc));
            //Debug.Write(model.Left.JsonTree);
            return View(model);
            //return View("namederView",model);
        }




        [HttpPost]
        public PartialViewResult addTextField(List<AddTextFieldModel> textFields) {
            TestModel model = new TestModel();
            if (textFields != null && textFields.Count > 0) {
                foreach (AddTextFieldModel t in textFields) {
                    model.TextFields.Add(t);
                }
            }
            model.TextFields.Add(new AddTextFieldModel("hallo", "123"));
            return PartialView("_AddTextField", new AddTextFieldModel("hallo", "123"));
            //return PartialView("_AddTextField", model);
        }
        [HttpPost]
        public ActionResult formData(TestModel model) {
            
            return View(model);
        }

       /* public string getXMLAsString(XDocument xml) {
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            xml.WriteTo(tx);
            string formatXml = XElement.Parse(sw.ToString()).ToString();
            return formatXml;// 
        }*/

    }
}
