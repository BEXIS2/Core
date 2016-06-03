using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BExIS.Xml.Services;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class ImportController : Controller
    {
        //
        // GET: /DIM/Import/
        public ActionResult Index()
        {

            //xml metadata for import
            string metadataForImportPath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "MetadataIDIV_EXAMPLE.xml");

            XmlDocument metadataForImport = new XmlDocument();
            metadataForImport.Load(metadataForImportPath);

            // metadataStructure DI
            long metadataStructureId = 3;

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            string metadataStructrueName = metadataStructureManager.Repo.Get(metadataStructureId).Name;

            // loadMapping file
            string path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(metadataStructureId, TransmissionType.mappingFileImport, metadataStructrueName));

            // XML mapper + mapping file
            XmlMapperManager xmlMapperManager = new XmlMapperManager();
            xmlMapperManager.Load(path_mappingFile, "IDIV");
            
            // generate intern metadata 
            XmlDocument metadataResult = xmlMapperManager.Generate(metadataForImport,1);

            // generate intern template
            XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
            XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId);
            XmlDocument metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

            XmlDocument completeMetadata = XmlMetadataImportHelper.FillInXmlAttributes(metadataResult, metadataXmlTemplate);

            // create Dataset

            //load datastructure
            DataStructureManager dsm = new DataStructureManager();
            ResearchPlanManager rpm = new ResearchPlanManager();
            MetadataStructureManager msm = new MetadataStructureManager();
                 
            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.CreateEmptyDataset(dsm.UnStructuredDataStructureRepo.Get(1), rpm.Repo.Get(1), msm.Repo.Get(3));

            if (dm.IsDatasetCheckedOutFor(dataset.Id, GetUsernameOrDefault()) || dm.CheckOutDataset(dataset.Id, GetUsernameOrDefault()))
            {
                DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);
                workingCopy.Metadata = completeMetadata;

                string title = XmlDatasetHelper.GetInformation(workingCopy, NameAttributeValues.title);
                if (String.IsNullOrEmpty(title)) title = "No Title available.";

                dm.EditDatasetVersion(workingCopy, null, null, null);
                dm.CheckInDataset(dataset.Id, "Metadata was submited.", GetUsernameOrDefault());

                // add security
                if (GetUsernameOrDefault() != "DEFAULT")
                {
                    PermissionManager pm = new PermissionManager();
                    SubjectManager sm = new SubjectManager();

                    BExIS.Security.Entities.Subjects.User user = sm.GetUserByName(GetUsernameOrDefault());

                    foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                    {
                        pm.CreateDataPermission(user.Id, 1, dataset.Id, rightType);
                    }
                }
            }

            

            return View();
        }

        #region helper

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        #endregion
    }

}