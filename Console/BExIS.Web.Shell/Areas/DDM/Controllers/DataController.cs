using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Orm.NH.Qurying;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : BaseController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public ActionResult DatasetPermissions(long datasetId)
        {
            var entityManager = new EntityManager();

            try
            {
                if (this.IsAccessible("SAM", "UserPermissions", "Subjects"))
                {
                    var result = this.Render("SAM", "UserPermissions", "Subjects",
                        new RouteValueDictionary() { { "EntityId", entityManager.FindByName("Dataset").Id }, { "InstanceId", datasetId } });

                    return Content(result.ToHtmlString(), "text/html");
                }
                else
                {
                    return PartialView("Error");
                }
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public JsonResult IsDatasetCheckedIn(long id)
        {
            DatasetManager dm = new DatasetManager();

            if (id != -1 && dm.IsDatasetCheckedIn(id))
                return Json(true);
            else
                return Json(false);
        }

        public ActionResult ShowData(long id)
        {
            DatasetManager dm = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();


            try
            {
                DatasetVersion dsv;
                ShowDataModel model = new ShowDataModel();

                string title = "";
                long metadataStructureId = -1;
                long dataStructureId = -1;
                long researchPlanId = 1;
                XmlDocument metadata = new XmlDocument();

                if (dm.IsDatasetCheckedIn(id))
                {
                    long versionId = dm.GetDatasetLatestVersionId(id); // check for zero value
                    dsv = dm.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo

                    metadataStructureId = dsv.Dataset.MetadataStructure.Id;

                    //MetadataStructureManager msm = new MetadataStructureManager();
                    //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                    title = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.title); // this function only needs metadata and extra fields, there is no need to pass the version to it.
                    dataStructureId = dsv.Dataset.DataStructure.Id;
                    researchPlanId = dsv.Dataset.ResearchPlan.Id;
                    metadata = dsv.Metadata;

                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Show Data : " + title, this.Session.GetTenant());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
                }

                model = new ShowDataModel()
                {
                    Id = id,
                    Title = title,
                    MetadataStructureId = metadataStructureId,
                    DataStructureId = dataStructureId,
                    ResearchPlanId = researchPlanId,
                    ViewAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), id, RightType.Read),
                    GrantAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), id, RightType.Grant)
                };

                //set metadata in session
                Session["ShowDataMetadata"] = metadata;

                return View(model);
            }
            finally
            {
                dm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        #region metadata

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetID"></param>
        /// <returns>model</returns>
        public ActionResult ShowMetaData(long entityId, string title, long metadatastructureId, long datastructureId, long researchplanId, string sessionKeyForMetadata)
        {
            var result = this.Run("DCM", "Form", "SetAdditionalFunctions", new RouteValueDictionary() { { "actionName", "Copy" }, { "controllerName", "CreateDataset" }, { "area", "DCM" }, { "type", "copy" } });
            result = this.Run("DCM", "Form", "SetAdditionalFunctions", new RouteValueDictionary() { { "actionName", "Reset" }, { "controllerName", "Form" }, { "area", "Form" }, { "type", "reset" } });
            result = this.Run("DCM", "Form", "SetAdditionalFunctions", new RouteValueDictionary() { { "actionName", "Cancel" }, { "controllerName", "Form" }, { "area", "DCM" }, { "type", "cancel" } });
            result = this.Run("DCM", "Form", "SetAdditionalFunctions", new RouteValueDictionary() { { "actionName", "Submit" }, { "controllerName", "CreateDataset" }, { "area", "DCM" }, { "type", "submit" } });

            var view = this.Render("DCM", "Form", "LoadMetadataFromExternal", new RouteValueDictionary()
            {
                { "entityId", entityId },
                { "title", title },
                { "metadatastructureId", metadatastructureId },
                { "datastructureId", datastructureId },
                { "researchplanId", researchplanId },
                { "sessionKeyForMetadata", sessionKeyForMetadata },
                { "resetTaskManager", false }
            });

            return Content(view.ToHtmlString(), "text/html");
        }

        private BaseModelElement GetModelFromElement(XElement element)
        {
            string name = element.Attribute("name").Value;
            string displayName = "";
            BExIS.Xml.Helpers.XmlNodeType type;

            // get Type
            type = BExIS.Xml.Helpers.XmlMetadataWriter.GetXmlNodeType(element.Attribute("type").Value);

            // get DisplayName
            if (type.Equals(BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute))
                displayName = element.Parent.Attribute("name").Value;
            else
                displayName = element.Attribute("name").Value;

            if (XmlUtility.HasChildren(element))
            {
                CompundAttributeModel model = new CompundAttributeModel();
                model.Name = name;
                model.Type = type;
                model.DisplayName = displayName;

                List<XElement> childrens = XmlUtility.GetChildren(element).ToList();
                foreach (XElement child in childrens)
                {
                    model.Childrens.Add(GetModelFromElement(child));
                }

                return model;
            }
            else
            {
                return new SimpleAttributeModel()
                {
                    Name = name,
                    DisplayName = displayName,
                    Value = element.Value,
                    Type = type
                };
            }
        }

        #endregion metadata

        #region primary data

        public ActionResult SetGridCommand(string filters, string orders, string columns)
        {
            Session["Columns"] = columns.Replace("ID", "").Split(',');

            //Session["Filter"] = GridHelper.ConvertToGridCommand(filters, orders);

            return null;
        }

        //[MeasurePerformance]
        public ActionResult ShowPrimaryData(long datasetID)
        {
            Session["Filter"] = null;
            Session["Columns"] = null;
            Session["DownloadFullDataset"] = false;
            ViewData["DownloadOptions"] = null;

            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            //permission download
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                if (dm.IsDatasetCheckedIn(datasetID))
                {
                    //long versionId = dm.GetDatasetLatestVersionId(datasetID); // check for zero value
                    //DatasetVersion dsv = dm.DatasetVersionRepo.Get(versionId);
                    DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                    DataStructure ds = dsm.AllTypesDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

                    // TODO: refactor Download Right not existing, so i set it to read
                    bool downloadAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name,
                        "Dataset", typeof(Dataset), datasetID, RightType.Read);

                    //TITLE
                    string title = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.title);

                    if (ds.Self.GetType() == typeof(StructuredDataStructure))
                    {
                        //ToDO Javad: 18.07.2017 -> replaced to the new API for fast retrieval of the latest version
                        //
                        //List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv, 0, 100);
                        //DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);
                        DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, 0, 100, true);

                        Session["gridTotal"] = dm.GetDatasetVersionEffectiveTupleCount(dsv);

                        return PartialView(ShowPrimaryDataModel.Convert(datasetID, title, sds, table, downloadAccess, IOUtility.GetSupportedAsciiFiles()));

                        //return PartialView(new ShowPrimaryDataModel());
                    }

                    if (ds.Self.GetType() == typeof(UnStructuredDataStructure))
                    {
                        return
                            PartialView(ShowPrimaryDataModel.Convert(datasetID, title, ds,
                                SearchUIHelper.GetContantDescriptorFromKey(dsv, "unstructuredData"), downloadAccess, IOUtility.GetSupportedAsciiFiles()));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
                }

                return PartialView(null);
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        #region server side

        [GridAction(EnableCustomBinding = true)]
        //[MeasurePerformance]
        public ActionResult _CustomPrimaryDataBinding(GridCommand command, string columns, int datasetID)
        {
            GridModel model = new GridModel();
            Session["Filter"] = command;
            DatasetManager dm = new DatasetManager();


            try
            {
                if (dm.IsDatasetCheckedIn(datasetID))
                {
                    DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);

                    FilterExpression filter = GridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = GridHelper.Convert(command.SortDescriptors.ToList());
                 
                    //ProjectionExpression projection;

                    DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, filter, orderBy, null, command.Page - 1, command.PageSize);

                    Session["gridTotal"] = dm.GetDatasetVersionEffectiveTupleCount(dsv);

                    model = new GridModel(table);
                    model.Total = Convert.ToInt32(Session["gridTotal"]); // (int)Session["gridTotal"];
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                }

                return View(model);
            }
            finally
            {
                dm.Dispose();
            }
        }

        #endregion server side

        #region download

        public JsonResult PrepareAscii(long id, string ext)
        {
            if (hasUserRights(id, RightType.Read))
            {
                DatasetManager datasetManager = new DatasetManager();
                string mimetype = MimeMapping.GetMimeMapping(ext);
                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    OutputDataManager ioOutputDataManager = new OutputDataManager();
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as {2}.", id,
                                                    datasetVersion.Id,ext);

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = GetFilteredData(id);
                        path = ioOutputDataManager.GenerateAsciiFile("temp", datatable, title, mimetype, datasetVersion.Dataset.DataStructure.Id);

                        LoggerFactory.LogCustom(message);


                        #endregion generate a subset of a dataset
                    }
                    else
                    {
                        path = ioOutputDataManager.GenerateAsciiFile(id, title, mimetype);

                        LoggerFactory.LogCustom(message);
                    }

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
                finally
                {
                    datasetManager.Dispose();
                }
            }
            else
            {
                return Json("User has no rights.", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadAscii(long id, string ext)
        {
            if (hasUserRights(id, RightType.Read))
            {
                DatasetManager datasetManager = new DatasetManager();

                string mimetype = MimeMapping.GetMimeMapping(ext);

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    OutputDataManager ioOutputDataManager = new OutputDataManager();
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";
                    string message = string.Format("dataset {0} version {1} was downloaded as {2}.", id,
                        datasetVersion.Id, ext);
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = GetFilteredData(id);
                        path = ioOutputDataManager.GenerateAsciiFile("temp", datatable, title, mimetype, datasetVersion.Dataset.DataStructure.Id);

                        LoggerFactory.LogCustom(message);

                        return File(path, mimetype, title + ext);

                        #endregion generate a subset of a dataset
                    }
                    else
                    {
                        path = ioOutputDataManager.GenerateAsciiFile(id, title, mimetype);

                        LoggerFactory.LogCustom(message);

                        var es = new EmailService();
                        es.Send(MessageHelper.GetDownloadDatasetHeader(),
                            MessageHelper.GetDownloadDatasetMessage(id, title, GetUsernameOrDefault()),
                            ConfigurationManager.AppSettings["SystemEmail"]
                            );


                        return File(path, mimetype, title + ext);
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(),
                        ex.Message,
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                    //OutputDataManager.ClearTempDirectory();

                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        public JsonResult PrepareExcelData(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsx";

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        datasetVersion.Id);

                    OutputDataManager outputDataManager = new OutputDataManager();


                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = GetFilteredData(id);
                        path = outputDataManager.GenerateExcelFile("temp", datatable, title, datasetVersion.Dataset.DataStructure.Id);

                        LoggerFactory.LogCustom(message);

                        //return File(path, "text/csv", title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, title, false);
                        LoggerFactory.LogCustom(message);
                    }


                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);

                }
                finally
                {
                    datasetManager.Dispose();
                }
            }
            else
            {
                return Json("User has no rights.", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadAsExcelData(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsx";

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        datasetVersion.Id);

                    OutputDataManager outputDataManager = new OutputDataManager();
                    string mimitype = MimeMapping.GetMimeMapping(ext);

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        //ToDo filter datatuples

                        DataTable datatable = GetFilteredData(id);
                        path = outputDataManager.GenerateExcelFile("temp", datatable, title, datasetVersion.Dataset.DataStructure.Id, ext);

                        LoggerFactory.LogCustom(message);

                        return File(path, mimitype, title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, title, false);
                        LoggerFactory.LogCustom(message);

                        var es = new EmailService();
                        es.Send(MessageHelper.GetDownloadDatasetHeader(),
                            MessageHelper.GetDownloadDatasetMessage(id, title, GetUsernameOrDefault()),
                            ConfigurationManager.AppSettings["SystemEmail"]
                            );

                        return File(Path.Combine(AppConfiguration.DataPath, path), mimitype, title + ext);
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(),
                        ex.Message,
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                    //OutputDataManager.ClearTempDirectory();

                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        public JsonResult PrepareExcelTemplateData(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsm";

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        datasetVersion.Id);

                    OutputDataManager outputDataManager = new OutputDataManager();


                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = GetFilteredData(id);
                        path = outputDataManager.GenerateExcelFile(id, title, true, datatable);

                        LoggerFactory.LogCustom(message);

                        //return File(path, "text/csv", title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, title, true);
                        LoggerFactory.LogCustom(message);
                    }


                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);

                }
                finally
                {
                    datasetManager.Dispose();
                }
            }
            else
            {
                return Json("User has no rights.", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadAsExcelTemplateData(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsm";

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        datasetVersion.Id);

                    OutputDataManager outputDataManager = new OutputDataManager();
                    string mimitype = MimeMapping.GetMimeMapping(ext);

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        //ToDo filter datatuples

                        DataTable datatable = GetFilteredData(id);
                        path = outputDataManager.GenerateExcelFile(id,title, true, datatable);

                        LoggerFactory.LogCustom(message);



                        return File(path, mimitype, title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, title, true);
                        LoggerFactory.LogCustom(message);

                        var es = new EmailService();
                        es.Send(MessageHelper.GetDownloadDatasetHeader(),
                            MessageHelper.GetDownloadDatasetMessage(id, title, GetUsernameOrDefault()),
                            ConfigurationManager.AppSettings["SystemEmail"]
                            );

                        return File(Path.Combine(AppConfiguration.DataPath, path), mimitype, title + ext);
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(),
                        ex.Message,
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                    //OutputDataManager.ClearTempDirectory();

                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        public ActionResult SetFullDatasetDownload(bool subset)
        {
            Session["DownloadFullDataset"] = subset;

            return Content("changed");
        }

        #region helper

        public void SetCommand(string filters, string orders)
        {
            Session["Filter"] = GridHelper.ConvertToGridCommand(filters, orders);
        }

        private bool filterInUse()
        {
            if ((Session["Filter"] != null || Session["Columns"] != null) && !(bool)Session["DownloadFullDataset"])
            {
                GridCommand command = (GridCommand)Session["Filter"];
                string[] columns = (string[])Session["Columns"];

                if (columns != null)
                {
                    if (command.FilterDescriptors.Count > 0 || command.SortDescriptors.Count > 0 || columns.Count() > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private DataTable GetFilteredData( long datasetId)
        {

            DatasetManager datasetManager = new DatasetManager();
            try
            {

                GridCommand command = null;
                string[] columns = null;

                if (Session["Filter"] != null) command = (GridCommand)Session["Filter"];

                if (Session["Columns"] != null) columns = (string[])Session["Columns"];

                FilterExpression filter = GridHelper.Convert(command.FilterDescriptors.ToList());
                OrderByExpression orderBy = GridHelper.Convert(command.SortDescriptors.ToList());

                ProjectionExpression projection = GridHelper.Convert(columns);

                DataTable table = datasetManager.GetLatestDatasetVersionTuples(datasetId, filter, orderBy, projection);

                if (projection == null) table.Strip();


                return table;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        private string getTitle(string title)
        {
            if (Session["Filter"] != null)
            {
                GridCommand command = (GridCommand)Session["Filter"];
                if (command.FilterDescriptors.Count > 0 || command.SortDescriptors.Count > 0)
                {
                    return title + "-Filtered";
                }
            }

            return title;
        }

        #endregion helper

        #endregion download

        #region download FileStream

        public ActionResult DownloadAllFiles(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {
                DatasetManager datasetManager = new DatasetManager();
                string title = "";
                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                    //TITLE
                    title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                    title = String.IsNullOrEmpty(title) ? "unknown" : title;

                    string zipPath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), title + ".zip");

                    if (FileHelper.FileExist(zipPath))
                    {
                        if (FileHelper.WaitForFile(zipPath))
                        {
                            FileHelper.Delete(zipPath);
                        }
                    }

                    ZipFile zip = new ZipFile();

                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                        string name = cd.URI.Split('\\').Last();

                        zip.AddFile(path, "");
                    }

                    zip.Save(zipPath);

                    string message = string.Format("all files from dataset {0} version {1} was downloaded.", datasetVersion.Dataset.Id,
                            datasetVersion.Id);
                    LoggerFactory.LogCustom(message);

                    var es = new EmailService();
                    es.Send(MessageHelper.GetDownloadDatasetHeader(),
                        MessageHelper.GetDownloadDatasetMessage(id, title, GetUsernameOrDefault()),
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );

                    return File(zipPath, "application/zip", title + ".zip");
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(),
                        ex.Message,
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                }
            }

            return Content("User has no rights.");
        }

        public ActionResult DownloadFile(long id, string path, string mimeType)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string title = path.Split('\\').Last();
                string message = string.Format("file was downloaded");
                LoggerFactory.LogCustom(message);

                var es = new EmailService();
                es.Send(MessageHelper.GetDownloadDatasetHeader(),
                    MessageHelper.GetDownloadDatasetMessage(id, title, GetUsernameOrDefault()),
                    ConfigurationManager.AppSettings["SystemEmail"]
                    );

                return File(Path.Combine(AppConfiguration.DataPath, path), mimeType, title);
            }

            return Content("User has no rights.");
        }

        #endregion download FileStream

        #endregion primary data

        #region datastructure

        [GridAction]
        public ActionResult _CustomDataStructureBinding(GridCommand command, long datasetID)
        {
            DataStructureManager dsm = new DataStructureManager();
            DatasetManager dm = new DatasetManager();


            try
            {
                long id = datasetID;
                if (dm.IsDatasetCheckedIn(id))
                {
                    DatasetVersion ds = dm.GetDatasetLatestVersion(id);
                    if (ds != null)
                    {

                        StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(ds.Dataset.DataStructure.Id);
                        dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);
                        //StructuredDataStructure sds = (StructuredDataStructure)(ds.Dataset.DataStructure.Self);
                        SearchUIHelper suh = new SearchUIHelper();
                        DataTable table = suh.ConvertStructuredDataStructureToDataTable(sds);

                        return View(new GridModel(table));
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                }
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
            }

            return View(new GridModel(new DataTable()));
        }

        public ActionResult ShowPreviewDataStructure(long datasetID)
        {
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();

            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    long dsId = dm.GetDatasetLatestVersion(datasetID).Id;
                    DatasetVersion ds = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsId);
                    DataStructure dataStructure = uow.GetReadOnlyRepository<DataStructure>().Get(ds.Dataset.DataStructure.Id);

                    long id = (long)datasetID;

                    Tuple<DataStructure, long> m = new Tuple<DataStructure, long>(
                        dataStructure,
                        id
                        );

                    return PartialView("_previewDatastructure", m);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion datastructure

        #region helper

        private List<DropDownItem> GetDownloadOptions()
        {
            List<DropDownItem> options = new List<DropDownItem>();

            options.Add(new DropDownItem()
            {
                Text = "Excel",
                Value = "0"
            });

            options.Add(new DropDownItem()
            {
                Text = "Excel (filtered)",
                Value = "1"
            });

            options.Add(new DropDownItem()
            {
                Text = "Csv",
                Value = "2"
            });

            options.Add(new DropDownItem()
            {
                Text = "Csv (filtered)",
                Value = "3"
            });

            options.Add(new DropDownItem()
            {
                Text = "Text",
                Value = "4"
            });

            options.Add(new DropDownItem()
            {
                Text = "Text (filtered)",
                Value = "5"
            });

            return options;
        }


        public string GetUsernameOrDefault()
        {
            var username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private static string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext)
        {
            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }

            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            DatasetManager dm = new DatasetManager();

            try
            {
                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
                {   // remove the one contentdesciptor
                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        if (cd.Name == name)
                        {
                            cd.URI = dynamicPath;
                            dm.UpdateContentDescriptor(cd);
                        }
                    }
                }
                else
                {
                    // add current contentdesciptor to list
                    //datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                    dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
                }

                //dm.EditDatasetVersion(datasetVersion, null, null, null);
                return dynamicPath;
            }
            finally
            {
                dm.Dispose();
            }
        }

        private bool hasUserRights(long entityId, RightType rightType)
        {
            #region security permissions and authorisations check

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            return entityPermissionManager.HasEffectiveRight(GetUsernameOrDefault(), "Dataset", typeof(Dataset), entityId, rightType);

            #endregion security permissions and authorisations check
        }

        #endregion helper
    }
}