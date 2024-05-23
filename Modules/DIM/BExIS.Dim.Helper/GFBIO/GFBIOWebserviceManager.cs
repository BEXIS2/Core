using BExIS.Dim.Entities.Publications;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;


namespace BExIS.Dim.Helpers.GFBIO
{
    public class GFBIOWebserviceManager : BasicWebService
    {
        public Broker Broker { get; set; }

        private string pathToApi = @"api/jsonws/GFBioProject-portlet.";
        private string addtionalPath = @"request-json";


        public GFBIOWebserviceManager(Broker broker)
        {
            Broker = broker;
        }

        #region Get

        //getUser
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> GetUserByEmail(string email)
        {
            string functionName = "get-user-by-email-address";
            string entityName = "userextension";
            //string addtionalPathX = @"json";

            string url = @"" + Broker.Server + "/" + pathToApi + entityName + "/" + functionName + "/" + addtionalPath + "/";
            string json = "{\"emailaddress\":\"" + email + "\"}";
            string encodedParameters = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, encodedParameters);
        }

        //get project
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetProjectById(long id)
        {
            string functionName = "get-project-by-id";
            string entityName = "project";
            string parameterName = "projectid";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName + "/" + addtionalPath + "/";
            string json = "{\"" + parameterName + "\":" + id + "}";
            string encodedParameters = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, encodedParameters);
        }

        //Get projects by username
        public async Task<string> GetProjectsByUser(long id)
        {
            string functionName = "get-projects-by-user";
            string entityName = "project";
            string parameterName = "userid";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName + "/" + addtionalPath + "/"; ;
            string json = "{\"" + parameterName + "\":" + id + "}";
            string encodedParameters = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, encodedParameters);
        }

        //get researchObject
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetResearchObjectById(long id)
        {
            string functionName = "get-research-object-by-id";
            string entityName = "researchobject";
            string parameterName = "researchobjectid";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName + "/" + addtionalPath + "/"; ;
            string json = "[{\"" + parameterName + "\":" + id + "}]";
            string encodedParameters = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, encodedParameters);
        }

        public async Task<string> GetStatusByResearchObjectById(long id)
        {
            string functionName = "get-status-by-research-object-id-and-version";
            string entityName = "submission";
            string parameterName = "researchobjectid";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName + "/" + addtionalPath + "/"; ;
            string json = "{\"" + parameterName + "\":" + id + "}";
            string encodedParameters = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, encodedParameters);
        }

        #endregion

        #region Set

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task<string> CreateProject(long userid, string name, string description)
        {
            string functionName = "create-project";
            string entityName = "project";
            addtionalPath = @"request-json";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName + "/" + addtionalPath + "/";

            string json = "{\"userid\":" + userid + ",\"name\":\"" + name + "\",\"description\":\"" + description + "\"}";

            string encodedParameters = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, encodedParameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="submitterid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="researchobjecttype"></param>
        /// <param name="authornames"></param>
        /// <returns></returns>
        public async Task<string> CreateResearchObject(long userid, long projectid, string name, string description, string researchobjecttype, XmlDocument extendedData, List<string> authornames, string metadataLabel)
        {
            string functionName = "create-research-object";
            string entityName = "researchobject";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName/* + "/" + addtionalPath + "/"*/;

            GFBIOResearchObjectJSON researchObject = new GFBIOResearchObjectJSON();

            researchObject.userid = userid;
            researchObject.projectid = projectid;
            researchObject.name = name;
            researchObject.description = description;
            researchObject.researchobjecttype = researchobjecttype;
            researchObject.extendeddata = extendedData;
            researchObject.metadatalabel = metadataLabel;
            researchObject.authornames = authornames.ToList();

            string json = "[" + JsonConvert.SerializeObject(researchObject) + "]";

            //string body = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, "", json);
        }

        public async Task<string> UpdateResearchObject(long researchobjectid, string name, string description, XmlDocument extendedData, List<string> authornames)
        {
            string functionName = "update-research-object";
            string entityName = "researchobject";

            string url = Broker.Server + "/" + pathToApi + entityName + "/" + functionName/* + "/" + addtionalPath + "/"*/;

            GFBIOResearchObjectUpdateJSON researchObject = new GFBIOResearchObjectUpdateJSON();

            researchObject.researchobjectid = researchobjectid;
            researchObject.name = name;
            researchObject.description = description;
            researchObject.extendeddata = extendedData;
            researchObject.authornames = authornames.ToList();

            string json = "[" + JsonConvert.SerializeObject(researchObject) + "]";

            //string body = WebServiceHelper.Encode(json);

            return await BasicWebService.Call(url, Broker.UserName, Broker.Password, "", json);
        }

        #endregion

    }

    ///// <summary>
    ///// Gfbio Post  Research Object
    ///// what you should send to the create a reseachbject
    ///// </summary>
    //public class GFBIOResearchObjectPostJSON
    //{
    //    public long userid { get; set; }
    //    public string name { get; set; }
    //    public long projectid { get; set; }
    //    public string extendeddata { get; set; }
    //    public long researchobjecttype { get; set; }


    //}


    public class GFBIOResearchObjectMiniJSON
    {
        public long userid { get; set; }

        // length 200
        public string name { get; set; }
        // length 15000
        public string description { get; set; }
        public string researchobjecttype { get; set; }
    }

    /// <summary>
    /// GFBIO Recieve Research Object
    /// WHat you get after call a get ResearchObject webservice
    /// or the returen result from a create research object
    /// </summary>
    public class GFBIOResearchObjectJSON
    {
        public long userid { get; set; }
        public long projectid { get; set; }

        // length 200
        public string name { get; set; }
        // length 15000
        public string description { get; set; }
        public string researchobjecttype { get; set; }
        // length 1500
        public List<string> authornames { get; set; }
        // length 1500
        public XmlDocument extendeddata { get; set; }
        public string metadatalabel { get; set; }

        public GFBIOResearchObjectJSON()
        {
            userid = 0;
            projectid = 0;
            name = "";
            description = "";
            researchobjecttype = "";
            authornames = new List<string>();
            extendeddata = new XmlDocument();
            metadatalabel = "";
        }
    }

    public class GFBIOResearchObjectUpdateJSON
    {
        public long researchobjectid { get; set; }

        // length 200
        public string name { get; set; }
        // length 15000
        public string description { get; set; }
        // length 1500
        public List<string> authornames { get; set; }
        // length 1500
        public XmlDocument extendeddata { get; set; }

        public GFBIOResearchObjectUpdateJSON()
        {
            researchobjectid = 0;
            name = "";
            description = "";
            authornames = new List<string>();
            extendeddata = new XmlDocument();
        }
    }

    public class GFBIOUser
    {
        public long userid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string middlename { get; set; }
        public string emailaddress { get; set; }
        public string fullname { get; set; }
        public string screenname { get; set; }
    }

    public class GFBIOResearchObjectResult
    {
        public long researchobjectid { get; set; }
        public long researchobjectversionid { get; set; }
    }

    public class GFBIOResearchObjectStatus
    {
        public long researchobjectid { get; set; }
        public string status { get; set; }
    }

    public class GFBIOProject
    {
        public long projectid { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class GFBIOException
    {
        public string exception { get; set; }
        public string message { get; set; }
    }
}
