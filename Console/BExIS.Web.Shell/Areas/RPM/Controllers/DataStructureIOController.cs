using System;
using System.Web.Mvc;

using BExIS.Modules.Rpm.UI.Models;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Output;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using Vaiona.Utils.Cfg;
using System.Collections.Generic;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class DataStructureIOController : BaseController
    {
        public FileResult downloadTemplate(long id)
        {
            if (id != 0)
            {
                DataStructureManager dataStructureManager = null;
                try
                {
                    dataStructureManager = new DataStructureManager();

                    StructuredDataStructure dataStructure = new StructuredDataStructure();
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);

                    if (dataStructure != null)
                    {
                        ExcelTemplateProvider provider = new ExcelTemplateProvider("BExISppTemplate_Clean.xlsm");
                        provider.CreateTemplate(dataStructure);
                        string path = "";

                        XmlNode resources = dataStructure.TemplatePaths.FirstChild;

                        XmlNodeList resource = resources.ChildNodes;

                        foreach (XmlNode x in resource)
                        {
                            if (x.Attributes.GetNamedItem("Type").Value == "Excel")
                                path = x.Attributes.GetNamedItem("Path").Value;

                        }
                        string rgxPattern = "[<>?\":|\\\\/*]";
                        string rgxReplace = "-";
                        Regex rgx = new Regex(rgxPattern);

                        string filename = rgx.Replace(dataStructure.Name, rgxReplace);

                        if (filename.Length > 50)
                            filename = filename.Substring(0, 50);

                        return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", "Template_" + dataStructure.Id + "_" + filename + ".xlsm");
                    }
                }
                finally
                {
                    dataStructureManager.Dispose();
                }
            }
            return File(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template", "BExISppTemplate_Clean.xlsm"), "application/xlsm", "Template_" + id + "_No_Data_Structure.xlsm");
        }
    }
}