using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Xml.Services.Mapping;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /DIM/Admin/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConvertMetadataToABCD()
        {
            string path_mapping_abcd = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "mapping_abcd.xml");

            XmlMapperManager xmlMapperManager = new XmlMapperManager();

            xmlMapperManager.Load(path_mapping_abcd);

            DatasetManager datasetManager = new DatasetManager();

            List<long> ids = datasetManager.GetDatasetLatestIds();


            foreach (long id in ids)
            {
                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset.MetadataStructure.Id.Equals(xmlMapperManager.xmlMapper.Id))
                {
                    XmlDocument metadata = datasetManager.GetDatasetLatestMetadataVersion(id);
                    xmlMapperManager.Export(metadata, id);
                }
            }

            return View("Index");
        }

        public ActionResult ConvertMetadataToEML()
        {
            string path_mapping_eml = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "mapping_eml.xml");

            XmlMapperManager xmlMapperManager = new XmlMapperManager();
            //xmlMapperManager.SearchForSequenceByDuplicatedElementNames = false;

            xmlMapperManager.Load(path_mapping_eml);

            DatasetManager datasetManager = new DatasetManager();

            List<long> ids = datasetManager.GetDatasetLatestIds();

            foreach (long id in ids)
            {
                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset.MetadataStructure.Id.Equals(xmlMapperManager.xmlMapper.Id))
                {
                    XmlDocument metadata = datasetManager.GetDatasetLatestMetadataVersion(id);
                    xmlMapperManager.Export(metadata, id);
                }
            }

            return View("Index");
        }


    }
}
