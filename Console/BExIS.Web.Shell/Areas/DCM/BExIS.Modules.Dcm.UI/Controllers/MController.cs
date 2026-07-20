using BExIS.App.Bootstrap.Attributes;
using BExIS.Dcm.UploadWizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Helpers;
using BExIS.Utils.Data;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BEXIS.JSON.Helpers;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{

    [SessionState(SessionStateBehavior.ReadOnly)]
    public class MController : Controller
    {

        public ActionResult Index()
        {


            return View();
        }
        
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Edit(long id, int version = 0)
        {

            string module = "DCM";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);
                ViewData["saveWithErrors"] = dataset.EntityTemplate.MetadataInvalidSaveMode;
            }

            

            return View();
        }

        #region mapping

        /// <summary>
        /// load System mappings based on metadatastructure id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [JsonNetFilter]
        public JsonResult LoadSystemMappings(long id)
        {
            //LinkElementType.MetadataStructure;
            //LinkElementType.System;

            // PartyType = 9,
            // PartyCustomType = 10,
            // Key = 10

            /*
            //Automatic System Keys starts at 100
            Id = 100,
            Version = 101,
            DateOfVersion = 102,
            MetadataCreationDate = 103,
            MetadataLastModfied = 104,
            DataCreationDate = 105,
            DataLastModified = 106, // also for Dubline Core date
             */

            List<long> ints = new List<long>() { 100, 101, 102, 103, 104, 105, 106 };

            // not implemented
            using (var mappingManager = new MappingManager())
            {
                try
                {

                    var sourceLE = mappingManager.GetLinkElement(id, LinkElementType.MetadataStructure);
                    var targetLE = mappingManager.GetLinkElement(0, LinkElementType.System);

                    if(sourceLE == null || targetLE == null)
                    {
                        return Json("", JsonRequestBehavior.AllowGet);
                    }

                    var rootMapping = mappingManager.GetMapping(sourceLE, targetLE);

                    var mappings = mappingManager.GetChildMappingFromRoot(rootMapping.Id, 2);

                    // get party mappings 
                    var partymappings = mappings.Where(m => m.Target.Type == LinkElementType.PartyCustomType).GroupBy(m => m.Source.XPath).Select(group => group.First()).ToList();
                    //partymappings = partymappings.GroupBy(obj => obj.Target).Select(g => g.First()).ToList();
                    var keymappings = mappings.Where(m => m.Target.Type == LinkElementType.Key && ints.Contains(m.Target.ElementId)).ToList();

                    List<PartyMappingResultModel> presult = new List<PartyMappingResultModel>();

                    foreach (var m in partymappings)
                    {
                        PartyMappingResultModel x = new PartyMappingResultModel();
                        x.Path = cleanPath(m.Source.XPath);
                        x.ParentPath = getParentPath(cleanPath(m.Source.XPath));   // use tha path from the source and create tha parent path because parent is allways one level up.
                        x.LinkElementId = m.Source.Id;
                        x.Selector = MappingUtils.PartyAttrIsMain(m.Source.ElementId, m.Source.Type);
                        x.Complexity = m.Source.ElementId != m.Parent.Source.ElementId;
                        x.List = getList(m.Source.ElementId, m.Source.Type);

                        presult.Add(x);
                    }



                    var kresult = keymappings.Select(m => new
                    {
                        Path = cleanPath(m.Source.XPath),
                        SystemKeyName = m.Source.Name

                    }).ToList();

   
                    var result = new
                    {
                        PartyMappings = presult,
                        KeyMappings = kresult
                    };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch(Exception ex)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }

            }
        }

        private List<MappingPartyResultElemenet> getList(long id, LinkElementType type)
        {
            var x = new List<MappingPartyResultElemenet>();

            if (MappingUtils.PartyAttrIsMain(id, type))
            {
                x = MappingUtils.GetAllMatchesInSystem(id, type, "");
                x = x.GroupBy(e => e.PartyId).Select(g => g.First()).ToList();   
            }

            return x;
        }

        private string cleanPath(string path)
        {
            //Metadata/Metadata/ContentMetadata/Description/DescriptionXmlSchemaComplexType/Representation/MetadataDescriptionRepr/MetadataDescriptionRepr
            //Description/Representation/MetadataDescriptionRepr

            string root = path.Split('/').FirstOrDefault();
            string rest = string.Join("/", path.Split('/').Skip(1).Where((val, index) => index % 2 == 0));

            string n = rest;
            n = n.Replace("/", ".");

            return n;
        }

        private string getParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            // Split the path by '/'
            string[] parts = path.Split('.');

            // Join everything back together except the last item
            // (parts.Length - 1 ignores the last element)
            return string.Join(".", parts, 0, parts.Length - 1);
        }

        public JsonResult GetPartyValue(long partyId, long linkId)
        {
            // not implemented
            if(partyId > 0 && linkId > 0)
            {
                // Implement logic to get party value
                using (var mappingManager = new MappingManager())
                {

                    var value = MappingUtils.GetValueFromSystem(partyId, linkId);

                    return Json(value, JsonRequestBehavior.AllowGet);
                    
                }

            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}