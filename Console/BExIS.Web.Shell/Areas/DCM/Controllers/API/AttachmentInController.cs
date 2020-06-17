﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Helpers.API;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models.API;
using BExIS.Modules.Dcm.UI.Helper.API;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.IO;
using Vaiona.Utils.Cfg;
using BExIS.IO;
using System.Web;
using System.Xml;
using System.Text.RegularExpressions;
using BExIS.Utils.Route;
using Vaiona.Entities.Common;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// either of XML, JSON, or CSV formats.
    /// </summary>
    /// <remarks>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// either of XML, JSON, or CSV formats.
    /// The design follows the RESTFull pattern mentioned in http://www.asp.net/web-api/overview/older-versions/creating-a-web-api-that-supports-crud-operations
    /// CSV formatter is implemented in the DataTupleCsvFormatter class in the Models folder.
    /// The formatter is registered in the WebApiConfig as an automatic formatter, so if the clinet sets the request's Mime type to text/csv, this formatter will be automatically engaged.
    /// text/xml and text/json return XML and JSON content accordingly.
    /// </remarks>
    public class AttachmentInController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // PUT: api/data/5
        [BExISApiAuthorize]
        [PutRoute("api/Attachment/{id}")]
        [HttpPut]
        public async Task<HttpResponseMessage> Put(long id)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var filelist = new List<Stream>();
                var request = Request.CreateResponse();
                User user = null;
                string error = "";

                DatasetManager datasetManager = new DatasetManager();
                UserManager userManager = new UserManager();
                EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

                try
                {
                    #region security

                    string token = this.Request.Headers.Authorization?.Parameter;
                    if (String.IsNullOrEmpty(token)) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Bearer token not exist.");

                    user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();
                    if (user == null) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Token is not valid.");

                    //check permissions

                    //entity permissions
                    if (id > 0)
                    {
                        Dataset d = datasetManager.GetDataset(id);
                        //dataset exist?
                        if (d == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "the dataset with the id (" + id + ") does not exist.");
                        //user has the right to write?
                        if (!entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Write)) Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "The token is not authorized to write into the dataset.");
                    }

                    #endregion security

                    var provider = new MultipartMemoryStreamProvider();
                    await Request.Content.ReadAsMultipartAsync(provider);

                    if (provider.Contents.Count > 0)
                    {
                        uploadFiles(provider.Contents.ToList(), id, "attached via api", user.Name, datasetManager);
                    }

                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent("Successful upload", Encoding.UTF8, "text/plain");
                    response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(@"text/html");
                    return response;
                }
                catch (Exception e)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                finally
                {
                    datasetManager.Dispose();
                    entityPermissionManager.Dispose();
                    userManager.Dispose();
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "The request doesn't contain valid content!");
            }
        }

        public async void uploadFiles(List<HttpContent> attachments, long datasetId, String description, string userName, DatasetManager dm)
        {
            var dataset = dm.GetDataset(datasetId);
            // var datasetVersion = dm.GetDatasetLatestVersion(dataset);
            if (dm.IsDatasetCheckedOutFor(datasetId, userName) || dm.CheckOutDataset(datasetId, userName))
            {
                DatasetVersion datasetVersion = dm.GetDatasetWorkingCopy(datasetId);
                StringBuilder files = new StringBuilder();
                foreach (var httpContent in attachments)
                {
                    var dataStream = await httpContent.ReadAsStreamAsync();
                    var dataPath = AppConfiguration.DataPath;
                    var fileName = httpContent.Headers.ContentDisposition.FileName;
                    fileName = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_.() ]+", "");

                    if (!Directory.Exists(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments")))
                        Directory.CreateDirectory(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments"));
                    var destinationPath = Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);

                    using (var fileStream = File.Create(destinationPath))
                    {
                        FileHelper.CopyStream(dataStream, fileStream);
                        fileStream.Close();

                        AddFileInContentDiscriptor(datasetVersion, fileName, description);

                        if (files.Length > 0) files.Append(", ");
                        files.Append(fileName);
                    }
                }

                ////set modification
                datasetVersion.ModificationInfo = new EntityAuditInfo()
                {
                    Performer = userName,
                    Comment = "Attachment",
                    ActionType = AuditActionType.Edit
                };

                dm.EditDatasetVersion(datasetVersion, null, null, null);
                dm.CheckInDataset(dataset.Id, "File/s :" + files.ToString() + " via api", userName, ViewCreationBehavior.None);
            }
        }

        private string AddFileInContentDiscriptor(DatasetVersion datasetVersion, String fileName, String description)
        {
            string dataPath = AppConfiguration.DataPath;
            string storePath = Path.Combine(dataPath, "Datasets", datasetVersion.Dataset.Id.ToString(), "Attachments");
            int lastOrderContentDescriptor = 0;

            if (datasetVersion.ContentDescriptors.Any())
                lastOrderContentDescriptor = datasetVersion.ContentDescriptors.Max(cc => cc.OrderNo);
            ContentDescriptor originalDescriptor = new ContentDescriptor()
            {
                OrderNo = lastOrderContentDescriptor + 1,
                Name = fileName,
                MimeType = MimeMapping.GetMimeMapping(fileName),
                URI = Path.Combine("Datasets", datasetVersion.Dataset.Id.ToString(), "Attachments", fileName),
                DatasetVersion = datasetVersion,
            };
            // replace the URI and description in case they have a same name
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(originalDescriptor.Name)) > 0)
            {
                //
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == originalDescriptor.Name)
                    {
                        cd.URI = originalDescriptor.URI;
                        cd.Extra = SetDescription(cd.Extra, description);
                    }
                }
            }
            else
            {
                // add file description Node
                XmlDocument doc = SetDescription(originalDescriptor.Extra, description);
                originalDescriptor.Extra = doc;
                //Add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);
            }

            return storePath;
        }

        private XmlDocument SetDescription(XmlNode extraField, string description)
        {
            XmlNode newExtra;
            var source = (XmlDocument)extraField;
            if (source == null)
            {
                source = new XmlDocument();
                source.LoadXml("<extra><fileDescription>" + description + "</fileDescription></extra>");
            }
            else
            {
                if (XmlUtility.GetXmlNodeByName(extraField, "fileDescription") == null)
                {
                    XmlNode t = XmlUtility.CreateNode("fileDescription", source);
                    t.InnerText = description;
                    source.DocumentElement.AppendChild(t);
                }
                else
                {
                    var descNodes = source.SelectNodes("/extra/fileDescription");
                    descNodes[0].InnerText = description;
                }
            }
            return source;
        }
    }
}