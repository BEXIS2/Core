using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Serialization;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Vaiona.Web.Mvc.Modularity;
using BExIS.Dlm.Services.Party;
using NameParser;
using BExIS.Dim.Services;

namespace BExIS.Modules.MCD.UI.Controllers.API
{
    public class CitationController : ApiController
    {
        // GET api/Citation/{id}
        /// <summary>
        /// Get citation of a dataset by id and version.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Citation/{id}")]
        [ResponseType(typeof(CitationModel))]
        public HttpResponseMessage Get(long id, int versionNumber = 0, [FromUri] Format format = Format.Bibtex)
        {

            return GetCitation(id, format, versionNumber);

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

                    CitationDataModel model = GetModel(datasetVersion);
                    citationString += generateCitation(id.ToString(), model, isPublic, Format.Text) + "\n\n";

                }
            }

            HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(citationString, Encoding.UTF8, "application/text") };
            return response;
        }

        /// <summary>
        /// Get citation string of all datasets
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Citation/Datasets")]
        [ResponseType(typeof(CitationModel))]
        public HttpResponseMessage Get()
        {

            return GetAllCitations();

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

        private HttpResponseMessage GetCitation(long id, Format format, int version_number = 0)
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

                CitationDataModel model = GetModel(datasetVersion);
                string citationString = generateCitation(id.ToString(), model, isPublic, format);
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

            datasetCitationEntry.Publisher = settings.GetValueByKey("publisher").ToString();
            datasetCitationEntry.InstanceName = settings.GetValueByKey("instanceName").ToString();

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
            datasetCitationEntry.DatasetId = datasetId.ToString();
            datasetCitationEntry.IsPublic = isPublic;
            datasetCitationEntry.Title = model.Title;
            datasetCitationEntry.Version = model.Version;
            if (!String.IsNullOrEmpty(model.DOI))
                datasetCitationEntry.DOI = model.DOI;
            datasetCitationEntry.Authors = model.Authors;
            datasetCitationEntry.Year = getYear(model.Date);

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

            datasetCitationEntry.CitationStringTxt = generateText(datasetCitationEntry.DatasetId, model.Title, datasetCitationEntry.Year, model.Version, model.DOI, datasetCitationEntry.URL, datasetCitationEntry.Publisher, datasetCitationEntry.InstanceName, authors, isPublic);

            return datasetCitationEntry;
        }

        private string generateCitation(string datasetId, CitationDataModel model, bool isPublic, Format format)
        {
            string citationString = "";
            var settings = ModuleManager.GetModuleSettings("ddm");
            string url = HttpContext.Current.Request.Url.Host;

            string publisher = settings.GetValueByKey("publisher").ToString();
            string instanceName = settings.GetValueByKey("instanceName").ToString();

            string year = getYear(model.Date);

            //create authorname in the correct format
            List<string> authors = new List<string>();
            foreach (string author in model.Authors)
            {
                HumanName name = new HumanName(author);
                if (String.IsNullOrEmpty(name.Middle))
                    authors.Add(name.Last + ", " + name.First);
                else
                    authors.Add(name.Last + ", " + name.Middle + " " + name.First);
            }

            switch (format)
            {
                case Format.Bibtex:
                    citationString = generateBibtex(datasetId, model.Title, year, model.Version, model.DOI, url, publisher, instanceName, authors, isPublic);
                    break;
                case Format.RIS:
                    citationString = generateRis(datasetId, model.Title, year, model.Version, model.DOI, url, publisher, instanceName, authors, isPublic);
                    break;
                case Format.Text:
                    citationString = generateText(datasetId, model.Title, year, model.Version, model.DOI, url, publisher, instanceName, authors, isPublic);
                    break;
            }

            return citationString;
        }

        private CitationDataModel GetModel(DatasetVersion datasetVersion)
        {
            CitationDataModel model = new CitationDataModel();

            XmlDocument xmlDoc = datasetVersion.Metadata;
            string citationString = "";
            //get citation concept
            using (var conceptManager = new ConceptManager())
            {
                var concept = conceptManager.MappingConceptRepository.Get().Where(c => c.Name.Equals("Citation")).FirstOrDefault();

                long mdId = datasetVersion.Dataset.MetadataStructure.Id;

                xmlDoc = MappingUtils.GetConceptOutput(mdId, concept.Id, xmlDoc);

                XmlSerializer serializer = new XmlSerializer(typeof(CitationDataModel), new XmlRootAttribute("data"));
                using (XmlReader reader = new XmlNodeReader(xmlDoc))
                {
                    model = (CitationDataModel)serializer.Deserialize(reader);
                }

                if(String.IsNullOrEmpty(model.Version))
                    model.Version = datasetVersion.VersionNo.ToString();
                if(String.IsNullOrEmpty(model.Date))
                {
                    if(String.IsNullOrEmpty(datasetVersion.PublicAccessDate.ToString()))
                        model.Date = datasetVersion.PublicAccessDate.ToString();
                    else
                        model.Date = datasetVersion.Timestamp.ToString();
                }
                if(String.IsNullOrEmpty(model.DOI))
                {
                    using (var publicationManager = new PublicationManager())
                    {
                        var pub = publicationManager.GetPublication(datasetVersion.Dataset.Id);
                        if(pub != null)
                        {
                            if(!String.IsNullOrEmpty(pub.Doi))
                                model.DOI = pub.Doi;    
                        }
                    }
                }
                    
            }

            return model;
        }

        private string generateBibtex(string datasetId, string title, string year, string version, string doi, string url, string publisher, string instanceName, List<string> authors, bool isPublic)
        {
            string bibtex = "@misc{" + instanceName + "_" + datasetId + "_v" + version + "\n";
            bibtex += "title ={" + title + "},\n";
            bibtex += "author ={";
            var lastAuthor = authors.Last();
            foreach (string author in authors)
            {
                if (author == lastAuthor)
                    bibtex += author + "";
                else
                    bibtex += author + " and ";
            }
            bibtex += "},\n";
            bibtex += "version ={" + version + "},\n";
            bibtex += "year ={" + year + "},\n";
            bibtex += "publisher ={" + publisher + "},\n";
            if (isPublic)
            {
                url += "/ddm/data/Showdata/" + datasetId + "?version=" + version + "";
                bibtex += "url ={" + url + "},\n";
            }
            else
                bibtex += "url ={" + url + "},\n";

            if (!String.IsNullOrEmpty(doi))
                bibtex += "doi ={" + doi + "},\n";

            if (isPublic)
                bibtex += "type ={Dataset. Published.},\n";
            else
                bibtex += "type ={Dataset. Unpublished.},\n";

            bibtex += "note ={Dataset ID: " + datasetId + "},\n";
            bibtex += "}";

            return bibtex;
        }

        private string generateRis(string datasetId, string title, string year, string version, string doi, string url, string publisher, string instanceName, List<string> authors, bool isPublic)
        {
            string ris = "TY  - DATA \n";
            ris += "T1 - " + title + "\n";
            foreach (string author in authors)
            {
                ris += "AU - " + author + "\n";
            }
            ris += "PY - " + year + "/// \n";
            ris += "PB - " + publisher + " \n";

            if (!String.IsNullOrEmpty(doi))
                ris += "DO - " + doi + "\n";

            if (isPublic)
            {
                url += "/ddm/data/Showdata/" + datasetId + "?version=" + version + "";
                ris += "UR - " + url + "\n";
            }
            else
                ris += "UR - " + url + "\n";

            if (isPublic)
                ris += "N1 - Dataset ID: " + datasetId + ", Published. \n";
            else
                ris += "N1 - Dataset ID: " + datasetId + ", Unpublished. \n`";
            ris += "ER  -";

            return ris;
        }

        private string generateText(string datasetId, string title, string year, string version, string doi, string url, string publisher, string instanceName, List<string> authors, bool isPublic)
        {
            string text = "";
            var lastAuthor = authors.Last();
            foreach (string author in authors)
            {
                if (author.Equals(lastAuthor))
                    text += author;
                else
                    text += author + "; ";
            }
            text += " (" + year + "): ";
            text += title + ". ";
            text += "Version " + version + ". ";
            text += publisher + ". ";
            text += "Dataset. ";
            if (!String.IsNullOrEmpty(doi))
            {
                text += doi;
            }
            else
            {
                if (isPublic)
                {
                    url += "/ddm/data/Showdata/" + datasetId + "?version=" + version + "";
                    text += url + ". ";
                }
                else
                    text += url + ". ";

                text += "Dataset ID= " + datasetId;
            }

            return text;
        }

        private string getYear(string dateString)
        {

            string year = "";
            var date_time_parts = dateString.Split(' ');
            // Format: "3/6/2022 7:47:10 PM"
            if (dateString.Contains("/"))
            {
                var date_parts = date_time_parts[0].Split('/');
                year = date_parts[2];
            }
            // Format: "2008-02-22"
            else
            {
                var date_parts = date_time_parts[0].Split('-');
                year = date_parts[0];
            }

            return year;
        }

        public enum Format
        {
            RIS,
            Text,
            Bibtex
        }

    }
}