using BExIS.App.Bootstrap.Attributes;
using BExIS.Dcm.UploadWizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Helpers;
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
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
   

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
                    var partymappings = mappings.Where(m => m.Target.Type == LinkElementType.PartyType || m.Target.Type == LinkElementType.PartyCustomType).ToList();
                    var keymappings = mappings.Where(m => m.Target.Type == LinkElementType.Key && ints.Contains(m.Target.ElementId)).ToList();


                    var presult = partymappings.Select(m => new
                    {
                        Path = cleanPath(m.Source.XPath),
                        ParentPath = cleanPath(m.Parent.Source.XPath),
                        LinkElementId = m.Source.Id,    
                        Selector = MappingUtils.PartyAttrIsMain(m.Source.ElementId, m.Source.Type),
                        Complexity = m.Source.ElementId != m.Parent.Source.ElementId,
                        List = getList(m.Source.ElementId, m.Source.Type)

                    }).ToList();

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

        #region import

        [HttpPost]
        public JsonResult Import(long id)
        {
            #region check incomming metadata
            string errorMessage = "";

            if (Request.Files.Count > 0)
            {
                using (var datasetManager = new DatasetManager())
                {
                    Dataset dataset = datasetManager.GetDataset(id);
                    long metadataStructureId = dataset.MetadataStructure.Id;

                    Stream requestStream;
                    HttpFileCollectionBase files = Request.Files;
                    var file = files[0]; // one file only
                    requestStream = file.InputStream;
                    #endregion check incomming metadata
                    string contentType = file.ContentType;
                    XmlDocument completeMetadata = null;
                    JSchema schema;
                    XmlMetadataConverter converter = new XmlMetadataConverter();
                    MetadataStructureConverter metadataStructureConverter = new MetadataStructureConverter();


                    if (contentType.Contains("xml"))
                    {
                        #region application/xml

                        XmlDocument metadataForImport = new XmlDocument();
                        metadataForImport.Load(requestStream);

                        // metadataStructure ID

                        var metadataStructrueName = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId).Name;

                        // loadMapping file
                        var path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(metadataStructureId, TransmissionType.mappingFileImport, metadataStructrueName));

                        // XML mapper + mapping file
                        var xmlMapperManager = new XmlMapperManager(TransactionDirection.ExternToIntern);
                        xmlMapperManager.Load(path_mappingFile, "IDIV");

                        // generate intern metadata without internal attributes
                        var metadataResult = xmlMapperManager.Generate(metadataForImport, 1, true);

                        // generate intern template metadata xml with needed attribtes
                        var xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
                        var metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId,
                            XmlUtility.ToXDocument(metadataResult));

                        var metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

                        // set attributes FROM metadataXmlTemplate TO metadataResult
                        completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult,
                            metadataXmlTemplate);

                        #endregion application/xml
                    }
                    else
                    if (contentType.Contains("json"))
                    {
                        #region application/json

                        using (var streamReader = new StreamReader(requestStream))
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            JsonSerializer serializer = new JsonSerializer();

                            try
                            {
                                JObject metadataJson = serializer.Deserialize<JObject>(jsonReader);

                                long mdid = 0;
                                if (metadataJson.ContainsKey("@id"))
                                {
                                    if (Int64.TryParse(metadataJson.Property("@id").Value.ToString(), out mdid))
                                    {
                                        schema = metadataStructureConverter.ConvertToJsonSchema(mdid);

                                        List<string> notAllowedElements = new List<string>();
                                        if (converter.HasValidStructure(metadataJson, mdid, out notAllowedElements))
                                        {
                                            completeMetadata = converter.ConvertTo(metadataJson);
                                            ;
                                        }
                                        else
                                        {
                                            HttpStatusCode statusCode = HttpStatusCode.ExpectationFailed;
                                            errorMessage = "the json does not have the expected structure";
                                        }
                                    }
                                    else
                                    {
                                        HttpStatusCode statusCode = HttpStatusCode.ExpectationFailed;
                                        errorMessage = "the json does not have the expected structure";
                                    }

                                }
                                else
                                {
                                    HttpStatusCode statusCode = HttpStatusCode.ExpectationFailed;
                                    errorMessage = "the json does not contain any information about the metadata structure";
                                }
                            }
                            catch (JsonReaderException)
                            {
                                Console.WriteLine("Invalid JSON.");
                            }
                        }

                        #endregion application/json
                    }

                    if (completeMetadata != null)
                    {
                        HttpStatusCode statusCode = HttpStatusCode.OK;

                        string json = "";

                        json = OutputMetadataManager.GetMetadataAsJson(completeMetadata, 1);
                        return Json(json);

                    }

                }
            }
            #endregion 

            return Json(errorMessage);
        }

        #region download

        //html

        public ActionResult DownloadAsHtml(long id, int version )
        {
         

            return Content("not implemented.");
        }

        //flatten


        //json
        public ActionResult DownloadAsJson(long id, int version)
        {
            try
            {
                string metadata = OutputMetadataManager.GetMetadataAsJson(id, version, 2);

                byte[] bytes = Encoding.ASCII.GetBytes(metadata);

                return File(bytes, "application/json");
        
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }


        //xml
        public ActionResult DownloadAsXml(long id, int version)
        {


            return Content("no metadata xml file is loaded.");
        }


        #endregion


        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult View(long id, int version = 0)
        {

            string module = "DCM";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }
    }
}