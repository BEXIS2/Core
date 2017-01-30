using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Remoting.Channels;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using BExIS.Dim.Entities;
using System.Web;


namespace BExIS.Dim.Helpers
{
    public class GFBIOWebserviceManager:BasicWebService
    {
        public DataRepository DataRepo { get; set; }

        private string pathToApi = @"api\jsonws\GFBioProject-portlet.";
        private string addtionalPath = @"request-json";


        public GFBIOWebserviceManager(DataRepository dataRepository)
        {
            DataRepo = dataRepository;
        }

        #region Get

        //get project
        public async Task<string> GetProjectById(long id)
        {
            string functionName = "get-project-by-id";
            string entityName = "project";
            string parameterName = "projectid";
            string encodedParameters = "";

            string json = "[{\"" + parameterName + "\":" + id + "}]";
            encodedParameters = WebServiceHelper.Encode(json);

            string url = Path.Combine(DataRepo.Server, pathToApi + entityName, functionName, addtionalPath);

            return await BasicWebService.Call(url, DataRepo.User, DataRepo.Password, encodedParameters);
        }

        //get researchObject
        public async Task<string> GetResearchObjectById(long id)
        {
            string functionName = "get-research-object-by-id";
            string entityName = "researchobject";
            string parameterName = "researchobjectid";
            string encodedParameters = "";

            string json = "[{\"" + parameterName + "\":" + id + "}]";
            encodedParameters = WebServiceHelper.Encode(json);

            string url = Path.Combine(DataRepo.Server, pathToApi + entityName, functionName, addtionalPath);

            return await BasicWebService.Call(url, DataRepo.User, DataRepo.Password, encodedParameters);
        }

        #endregion

    }
}
