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
using System.Web;

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
                        
                        string path = Path.Combine(AppConfiguration.DataPath, provider.CreateTemplate(dataStructure));
                        return File(path, MimeMapping.GetMimeMapping(path), Path.GetFileName(path));
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