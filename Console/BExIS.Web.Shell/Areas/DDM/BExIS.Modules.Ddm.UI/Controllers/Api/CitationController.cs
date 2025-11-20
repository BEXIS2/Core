using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using NameParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Serialization;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.MCD.UI.Controllers.API
{
    public class CitationController : ApiController
    {
        [BExISApiAuthorize]
        [GetRoute("api/datasets/citations")]
        //[ResponseType(typeof(CitationModel))]
        public HttpResponseMessage Get([FromUri] CitationFormat format = CitationFormat.Bibtex)
        {
            return GetAllCitations();
        }

        [BExISApiAuthorize]
        [GetRoute("api/datasets/{datasetId}/citations")]
        [ResponseType(typeof(CitationModel))]
        public HttpResponseMessage GetCitationFromLatestVersion(long datasetId, [FromUri] CitationFormat format = CitationFormat.Bibtex)
        {
            try
            {
                return GetCitation(datasetId, format);              
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred while retrieving the citation: " + ex.Message);
            }
        }

        [BExISApiAuthorize]
        [GetRoute("api/datasets/{datasetId}/citations/{versionNumber}")]
        [ResponseType(typeof(CitationModel))]
        public HttpResponseMessage GetCitationFromSpecificVersionNumber(long datasetId, int versionNumber, [FromUri] CitationFormat format = CitationFormat.Bibtex)
        {
            return GetCitation(datasetId, format, versionNumber);
        }

        // GET api/Citation/GetCitations
        /// <summary>
        /// Get citation a set of datasets by ids.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        [BExISApiAuthorize]
        [PostRoute("api/Citation/GetCitations")]
        public HttpResponseMessage Post([FromBody] CitationDatasetIds data)
        {
            var datasetIds = data.DatasetIds.Distinct().ToArray();
            string citationString = "";

            foreach (string datasetid in datasetIds)
            {
                var temp = datasetid.Split(',');
                long id = long.Parse(temp[0].Trim());
                int version_number = 0;
                if (temp.Length > 1)
                    version_number = int.Parse(temp[1].Trim());

                DatasetVersion datasetVersion = null;

                using (DatasetManager dm = new DatasetManager())
                using (EntityManager entityManager = new EntityManager())
                using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                using (UserManager userManager = new UserManager())
                {
                    bool isPublic = false;
                    if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset id should be greater then 0.");

                    // try to get latest dataset version 
                    if (version_number <= 0)
                    {
                        datasetVersion = dm.GetDatasetLatestVersion(id);
                    }
                    // try to get dataset version by version number
                    else
                    {
                        if (version_number <= 0)
                            return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version name does not exist for this dataset");

                        try
                        {
                            int index = version_number - 1;
                            Dataset dataset = dm.GetDataset(id);

                            int versions = dataset.Versions.Count;

                            if (versions < version_number)
                                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version number does not exist for this dataset");

                            datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);

                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    // Check if a dataset version was set
                    if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.InternalServerError, "It is not possible to load the latest or given version.");

                    //entity permissions
                    if (id > 0)
                    {
                        Dataset dataset = dm.GetDataset(id);

                        // Check if dataset is public
                        long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                        entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                        isPublic = false;
                        isPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;

                        // If dataset is not public check if a valid token is provided
                        if (isPublic == false)
                        {
                            User user = ControllerContext.RouteData.Values["user"] as User;

                            // If user is registered pass
                            if (user == null)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and the token is not valid.");
                            }
                        }

                        if (dataset == null)
                            return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The dataset with the id (" + id + ") does not exist.");
                    }

                    var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                    var useTags = Convert.ToBoolean(moduleSettings.GetValueByKey("use_tags"));

                    CitationDataModel model = CitationsHelper.CreateCitationDataModel(datasetVersion, CitationFormat.Text);
                    string datasetCitationString = "";

                    if (CitationsHelper.IsCitationDataModelValid(model))
                        datasetCitationString = CitationsHelper.GetCitationString(model, CitationFormat.Text, isPublic, datasetVersion.Dataset.Id, useTags);
                    else
                        datasetCitationString = "Citation metadata for id " + datasetid + " is incomplete.";

                    citationString += datasetCitationString;

                }
            }

            HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(citationString, Encoding.UTF8, "application/text") };
            return response;
        }


        private HttpResponseMessage GetAllCitations()
        {
            IEnumerable<long> datasetIds;
            using (DatasetManager datasetManager = new DatasetManager())
            {
                datasetIds = datasetManager.DatasetRepo.Get().Select(d => d.Id);
            }

            List<DatasetCitationEntry> allDatasetCitations = new List<DatasetCitationEntry>();

            foreach (long id in datasetIds)
            {
                DatasetVersion datasetVersion = null;

                using (DatasetManager dm = new DatasetManager())
                using (EntityManager entityManager = new EntityManager())
                using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                using (UserManager userManager = new UserManager())
                {
                    bool isPublic = false;

                    try
                    {
                        Dataset dataset = dm.GetDataset(id);

                        datasetVersion = dataset.Versions.OrderByDescending(t => t.Id).Where(p => p.Timestamp <= dataset.LastCheckIOTimestamp).First();

                    }
                    catch (Exception ex)
                    {
                    }

                    //entity permissions
                    if (id > 0)
                    {
                        Dataset dataset = dm.GetDataset(id);

                        // Check if dataset is public
                        long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                        entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                        isPublic = false;
                        isPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;

                        // If dataset is not public check if a valid token is provided
                        if (isPublic == false)
                        {
                            User user = ControllerContext.RouteData.Values["user"] as User;

                            // If user is registered pass
                            if (user == null)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and the token is not valid.");
                            }
                        }

                        //if (dataset == null)
                        //return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The dataset with the id (" + id + ") does not exist.");
                    }

                    if (datasetVersion == null)
                        continue;
                    else
                    {
                        XmlDocument xmlDoc = datasetVersion.Metadata;
                        //get citation concept
                        using (var conceptManager = new ConceptManager())
                        {
                            var concept = conceptManager.MappingConceptRepository.Get().Where(c => c.Name.Equals("Citation")).FirstOrDefault();

                            if (concept == null)
                                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "In combination with the format subset - subsettype must not be empty");

                            long mdId = datasetVersion.Dataset.MetadataStructure.Id;

                            xmlDoc = MappingUtils.GetConceptOutput(mdId, concept.Id, xmlDoc);

                            CitationDataModel model = new CitationDataModel();

                            XmlSerializer serializer = new XmlSerializer(typeof(CitationDataModel), new XmlRootAttribute("data"));
                            using (XmlReader reader = new XmlNodeReader(xmlDoc))
                            {
                                model = (CitationDataModel)serializer.Deserialize(reader);
                                if (model != null)
                                    allDatasetCitations.Add(CreateCitationEntry(id, model, isPublic));
                                else
                                    continue;
                            }
                        }
                    }
                }
            }

            // create response and return as JSON
            var response = Request.CreateResponse(HttpStatusCode.OK);
            string resp = JsonConvert.SerializeObject(allDatasetCitations);

            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;
        }

        private HttpResponseMessage GetCitation(long id, CitationFormat format, int version_number = 0)
        {
            DatasetVersion datasetVersion = null;

            using (DatasetManager dm = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (UserManager userManager = new UserManager())
            {
                bool isPublic = false;
                if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset id should be greater then 0.");

                // try to get latest dataset version 
                if (version_number <= 0)
                {
                    datasetVersion = dm.GetDatasetLatestVersion(id);
                }
                // try to get dataset version by version number
                else
                {
                    if (version_number <= 0)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version name does not exist for this dataset");

                    try
                    {
                        int index = version_number - 1;
                        Dataset dataset = dm.GetDataset(id);

                        int versions = dataset.Versions.Count;

                        if (versions < version_number)
                            return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version number does not exist for this dataset");

                        datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);

                    }
                    catch (Exception ex)
                    {
                    }
                }

                // Check if a dataset version was set
                if (datasetVersion == null) 
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "It is not possible to load the latest or given version.");

                //entity permissions
                if (id > 0)
                {
                    Dataset dataset = dm.GetDataset(id);

                    // Check if dataset is public
                    long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                    entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                    isPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;

                    // If dataset is not public check if a valid token is provided
                    if (isPublic == false)
                    {
                        User user = ControllerContext.RouteData.Values["user"] as User;

                        // If user is registered pass
                        if (user == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and the token is not valid.");
                        }
                    }

                    if (dataset == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The dataset with the id (" + id + ") does not exist.");
                }

                var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                var useTags = Convert.ToBoolean(moduleSettings.GetValueByKey("use_tags"));

                CitationDataModel model = CitationsHelper.CreateCitationDataModel(datasetVersion, format);
                string citationString = "";
                if (CitationsHelper.IsCitationDataModelValid(model))
                    citationString =  CitationsHelper.GetCitationString(model, format, isPublic, datasetVersion.Dataset.Id, useTags);
                else
                    citationString = "Citation metadata is incomplete.";

                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(citationString, Encoding.UTF8, "application/text") };
                return response;
            }
        }

        private DatasetCitationEntry CreateCitationEntry(long datasetId, CitationDataModel model, bool isPublic)
        {
            DatasetCitationEntry datasetCitationEntry = new DatasetCitationEntry();
            var settings = ModuleManager.GetModuleSettings("ddm");

            string url = String.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host);

            if (isPublic)
                datasetCitationEntry.URL = url + "/ddm/data/Showdata/" + datasetId + "?version=" + model.Version + "";
            else
                datasetCitationEntry.URL = url;

            datasetCitationEntry.Publisher = model.Publisher;
            //datasetCitationEntry.InstanceName = settings.GetValueByKey("instanceName").ToString();

            if (model.Projects != null || model.Projects.Count > 0)
            {
                using (PartyManager partyManager = new PartyManager())
                {
                    foreach (string project in model.Projects)
                    {
                        Project p = new Project();
                        p.Id = partyManager.Parties.Where(c => c.Name == project).Select(c => c.Id).FirstOrDefault();
                        p.Name = project;
                        datasetCitationEntry.Projects.Add(p);
                    }
                }
            }

            datasetCitationEntry.DatasetId = datasetId.ToString();
            datasetCitationEntry.IsPublic = isPublic;
            datasetCitationEntry.Title = model.Title;
            datasetCitationEntry.Version = model.Version;
            if (!String.IsNullOrEmpty(model.DOI))
                datasetCitationEntry.DOI = model.DOI;
            datasetCitationEntry.Authors = model.Authors;
            datasetCitationEntry.Year = model.Year;

            //create authorname in the correct format
            List<string> authors = new List<string>();
            foreach (string author in model.Authors)
            {
                HumanName name = new HumanName(author);
                if (String.IsNullOrEmpty(name.Middle))
                    authors.Add(name.Last + ", " + name.First);
                else
                    authors.Add(name.Last + ", " + name.First + " " + name.Middle);
            }

            var useTags = Convert.ToBoolean(settings.GetValueByKey("use_tags"));
            if (CitationsHelper.IsCitationDataModelValid(model))
            {
                datasetCitationEntry.CitationStringTxt = CitationsHelper.GetCitationString(model, CitationFormat.Text, isPublic, datasetId, useTags);
                return datasetCitationEntry;
            }
            else
                return null;
        }
    }
}