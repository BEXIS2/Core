using BExIS.App.Bootstrap.Attributes;
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
        [HttpGet, GetRoute("api/versions/database")]
        public async Task<HttpResponseMessage> GetVersionFromDatabase()
        {
            try
            {
                using (var versionManager = new VersionManager())
                {
                    var versionDatabase = versionManager.GetLatestVersion().Value;
                    return Request.CreateResponse(HttpStatusCode.OK, versionDatabase);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/versions/site")]
        public async Task<HttpResponseMessage> GetVersionFromSite()
        {
            try
            {
                // Site
                var versionSite = ConfigurationManager.AppSettings["ApplicationVersion"];

                return Request.CreateResponse(HttpStatusCode.OK, versionSite);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/versions/workspace")]
        public async Task<HttpResponseMessage> GetVersionFromWorkspace()
        {
            try
            {
                // Workspace
                string filePath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.xml");
                XDocument settings = XDocument.Load(filePath);
                XElement entry = XmlUtility.GetXElementByAttribute("entry", "key", "version", settings);
                var versionWorkspace = entry.Attribute("value")?.Value;

                return Request.CreateResponse(HttpStatusCode.OK, versionWorkspace);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/versions/")]
        public async Task<HttpResponseMessage> GetVersions()
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

                    var model = new ReadVersionsModel()
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