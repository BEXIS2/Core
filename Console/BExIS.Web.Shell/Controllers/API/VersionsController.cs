using BExIS.Security.Services.Versions;
using BExIS.Utils.Route;
using BExIS.Web.Shell.Models;
using BExIS.Xml.Helpers;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Controllers.API
{
    public class VersionsController : ApiController
    {
        [HttpGet, GetRoute("api/versions/")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                // Site
                var versionSite = ConfigurationManager.AppSettings["ApplicationVersion"];

                // Database
                using (var versionManager = new VersionManager())
                {
                    var versionDatabase = versionManager.GetLatestVersion().Value;

                    // Workspace
                    string filePath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.xml");
                    XDocument settings = XDocument.Load(filePath);
                    XElement entry = XmlUtility.GetXElementByAttribute("entry", "key", "version", settings);
                    var versionWorkspace = entry.Attribute("value")?.Value;

                    var model = new VersionModel()
                    {
                        Site = versionSite,
                        Database = versionDatabase,
                        Workspace = versionWorkspace
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
            
    }
}
