using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Xml.Services;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class ManageMetadataStructureController : Controller
    {
        // GET: DCM/ManageMetadataStructure
        public ActionResult Index()
        {
            return View(GetDefaultModel());
        }

        [HttpPost]
        [GridAction]
        public ActionResult Update(string id)
        {

            return View(new GridModel(GetDefaultModel().MetadataStructureModels));
        }

        private MetadataStructureManagerModel GetDefaultModel()
        {
            MetadataStructureManagerModel tmp = new MetadataStructureManagerModel();

           //load all metadatastructure
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            IEnumerable<MetadataStructure> metadataStructures = metadataStructureManager.Repo.Get();

            foreach (var metadataStructure in metadataStructures)
            {
                tmp.MetadataStructureModels.Add(ConvertToMetadataStructureModel(metadataStructure));
            }


            return tmp;
        }

        #region helper

        private MetadataStructureModel ConvertToMetadataStructureModel(MetadataStructure metadataStructure)
        {
            MetadataStructureModel metadataStructureModel = new MetadataStructureModel();
            metadataStructureModel.Id = metadataStructure.Id;
            metadataStructureModel.Name = metadataStructure.Name;

            //get all informaions from xml
            metadataStructureModel.EntityClassPath = XmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStructure.Id);
            metadataStructureModel.TitlePath = XmlDatasetHelper.GetInformationPath(metadataStructure,
                NameAttributeValues.title);
            metadataStructureModel.DescriptionClassPath = XmlDatasetHelper.GetInformationPath(metadataStructure,
                NameAttributeValues.description);

            metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructureModel.Id);
            metadataStructureModel.EnitiesClassPaths = GetEntityClassPathList();

            return metadataStructureModel;
        }

        /// <summary>
        /// Get all simple attributes of a metadata Structure
        /// </summary>
        /// <param name="metadatastructureId"></param>
        /// <returns></returns>
        private List<SearchMetadataNode> GetAllXPath(long metadatastructureId)
        {
            SearchDesigner searchDesigner = new SearchDesigner();
            return searchDesigner.GetAllXPathsOfSimpleAttributes(metadatastructureId);
        }

        private List<string> GetEntityClassPathList()
        {
            EntityManager entityManager = new EntityManager();

            IEnumerable<string> tmp = entityManager.GetAllEntities().Select(e => e.ClassPath);

            return tmp.ToList();
        }

        #endregion



    }
}