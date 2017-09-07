using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : BaseController
    {

        public ActionResult ShowData(long id)
        {

            DatasetManager dm = new DatasetManager();

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
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

                //metadataStructureId = dm.DatasetVersionRepo.Get(id).Dataset.MetadataStructure.Id; 

                //MetadataStructureManager msm = new MetadataStructureManager();
                //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                title = XmlDatasetHelper.GetInformation(dsv, NameAttributeValues.title); // this function only needs metadata and extra fields, there is no need to pass the version to it.
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
                ViewAccess = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), id, RightType.Read),
                GrantAccess = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), id, RightType.Grant)
            };

            //set metadata in session
            Session["ShowDataMetadata"] = metadata;

            return View(model);
        }

        public JsonResult IsDatasetCheckedIn(long id)
        {
            DatasetManager dm = new DatasetManager();

            if (id != -1 && dm.IsDatasetCheckedIn(id))
                return Json(true);
            else
                return Json(false);
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
            result = this.Run("DCM", "Form", "SetAdditionalFunctions", new RouteValueDictionary() { { "actionName", "Reset" }, { "controllerName", "CreateDataset" }, { "area", "Form" }, { "type", "reset" } });
            result = this.Run("DCM", "Form", "SetAdditionalFunctions", new RouteValueDictionary() { { "actionName", "Cancel" }, { "controllerName", "CreateDataset" }, { "area", "DCM" }, { "type", "cancel" } });
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

        #endregion

        #region primary data

        //[MeasurePerformance]
        public ActionResult ShowPrimaryData(long datasetID)
        {
            Session["Filter"] = null;
            Session["Columns"] = null;
            Session["DownloadFullDataset"] = false;
            ViewData["DownloadOptions"] = null;

            DatasetManager dm = new DatasetManager();

            if (dm.IsDatasetCheckedIn(datasetID))
            {
                //long versionId = dm.GetDatasetLatestVersionId(datasetID); // check for zero value
                //DatasetVersion dsv = dm.DatasetVersionRepo.Get(versionId);
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                DataStructureManager dsm = new DataStructureManager();
                this.Disposables.Add(dsm);

                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                DataStructure ds = dsm.AllTypesDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

                //permission download
                EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
                this.Disposables.Add(entityPermissionManager);

                // TODO: refactor Download Right not existing, so i set it to read
                bool downloadAccess = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name,
                    "Dataset", typeof(Dataset), datasetID, RightType.Read);

                //TITLE
                string title = XmlDatasetHelper.GetInformation(dsv, NameAttributeValues.title);

                if (ds.Self.GetType() == typeof(StructuredDataStructure))
                {

                    //ToDO Javad: 18.07.2017 -> replaced to the new API for fast retrieval of the latest version
                    //
                    //List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv, 0, 100);
                    //DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);
                    DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, 0, 100);

                    Session["gridTotal"] = dm.GetDatasetVersionEffectiveTupleCount(dsv);

                    return PartialView(ShowPrimaryDataModel.Convert(datasetID, title, sds, table, downloadAccess));

                    //return PartialView(new ShowPrimaryDataModel());
                }

                if (ds.Self.GetType() == typeof(UnStructuredDataStructure))
                {
                    return
                        PartialView(ShowPrimaryDataModel.Convert(datasetID, title, ds,
                            SearchUIHelper.GetContantDescriptorFromKey(dsv, "unstructuredData"), downloadAccess));
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
            }


            return PartialView(null);

        }

        #region server side

        [GridAction(EnableCustomBinding = true)]
        //[MeasurePerformance]
        public ActionResult _CustomPrimaryDataBinding(GridCommand command, int datasetID)
        {
            GridModel model = new GridModel();
            Session["Filter"] = command;
            DatasetManager dm = new DatasetManager();
            if (dm.IsDatasetCheckedIn(datasetID))
            {
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);


                // commented by Javad. Now the new API is called
                //List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv, command.Page - 1,
                //    command.PageSize);
                //DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);
                DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, command.Page - 1, command.PageSize);

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
        #endregion

        public ActionResult SetGridCommand(string filters, string orders, string columns)
        {
            Session["Columns"] = columns.Replace("ID", "").Split(',');

            Session["Filter"] = GridHelper.ConvertToGridCommand(filters, orders);

            return null;
        }

        #region download

        public ActionResult SetFullDatasetDownload(bool subset)
        {
            Session["DownloadFullDataset"] = subset;

            return Content("changed");
        }

        public ActionResult DownloadAsExcelData(long id)
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

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        //ToDo filter datatuples

                        OutputDataManager ioOutputDataManager = new OutputDataManager();
                        path = ioOutputDataManager.GenerateExcelFile(id, title);
                        LoggerFactory.LogCustom(message);

                        return File(path, "application/xlsm", title + ext);

                        #endregion
                    }

                    //filter not in use
                    else
                    {
                        OutputDataManager outputDataManager = new OutputDataManager();
                        path = outputDataManager.GenerateExcelFile(id, title);
                        LoggerFactory.LogCustom(message);

                        return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", title + ext);
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        public ActionResult DownloadAsCsvData(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {

                string ext = ".csv";

                try
                {
                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    OutputDataManager ioOutputDataManager = new OutputDataManager();
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";
                    string message = string.Format("dataset {0} version {1} was downloaded as csv.", id,
                        datasetVersion.Id);
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset


                        String[] visibleColumns = null;

                        if (Session["Columns"] != null)
                            visibleColumns = (String[])Session["Columns"];

                        path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/csv", visibleColumns);

                        LoggerFactory.LogCustom(message);

                        return File(path, "text/csv", title + ext);

                        #endregion
                    }
                    else
                    {
                        path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/csv");

                        LoggerFactory.LogCustom(message);

                        return File(path, "text/csv", title + ".csv");
                    }

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            else
            {
                return Content("User has no rights.");
            }

        }

        public ActionResult DownloadAsTxtData(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {

                string ext = ".txt";

                try
                {


                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    OutputDataManager ioOutputDataManager = new OutputDataManager();
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as txt.", id,
                                                    datasetVersion.Id);

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset


                        String[] visibleColumns = null;

                        if (Session["Columns"] != null)
                            visibleColumns = (String[])Session["Columns"];

                        path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/plain", visibleColumns);

                        LoggerFactory.LogCustom(message);

                        return File(path, "text/csv", title + ext);

                        #endregion
                    }
                    else
                    {
                        path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/plain");


                        LoggerFactory.LogCustom(message);

                        return File(path, "text/plain", title + ".txt");
                    }

                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
            else
            {
                return Content("User has no rights.");
            }

        }

        #region helper

        private List<AbstractTuple> GetFilteredDataTuples(DatasetVersion datasetVersion)
        {
            DatasetManager datasetManager = new DatasetManager();
            List<AbstractTuple> datatuples = datasetManager.GetDatasetVersionEffectiveTuples(datasetVersion);

            if (Session["Filter"] != null)
            {
                GridCommand command = (GridCommand)Session["Filter"];

                List<AbstractTuple> dataTupleList = datatuples;


                if (command.FilterDescriptors.Count > 0)
                {

                    foreach (IFilterDescriptor filter in command.FilterDescriptors)
                    {
                        var test = filter;

                        // one filter is set
                        if (filter.GetType() == typeof(FilterDescriptor))
                        {
                            FilterDescriptor filterDescriptor = (FilterDescriptor)filter;

                            // get id as long from filtername
                            Regex r = new Regex("(\\d+)");
                            long id = Convert.ToInt64(r.Match(filterDescriptor.Member).Value);

                            var list = from datatuple in dataTupleList
                                       let val = datatuple.VariableValues.Where(p => p.Variable.Id.Equals(id)).FirstOrDefault()
                                       where GridHelper.ValueComparion(val, filterDescriptor.Operator, filterDescriptor.Value)
                                       select datatuple;

                            dataTupleList = list.ToList();
                        }
                        else
                        // more than one filter is set 
                        if (filter.GetType() == typeof(CompositeFilterDescriptor))
                        {
                            CompositeFilterDescriptor filterDescriptor = (CompositeFilterDescriptor)filter;

                            List<AbstractTuple> temp = new List<AbstractTuple>();

                            foreach (IFilterDescriptor f in filterDescriptor.FilterDescriptors)
                            {
                                if ((FilterDescriptor)f != null)
                                {
                                    FilterDescriptor fd = (FilterDescriptor)f;
                                    // get id as long from filtername
                                    Regex r = new Regex("(\\d+)");
                                    long id = Convert.ToInt64(r.Match(fd.Member).Value);

                                    var list = from datatuple in dataTupleList
                                               let val = datatuple.VariableValues.Where(p => p.Variable.Id.Equals(id)).FirstOrDefault()
                                               where GridHelper.ValueComparion(val, fd.Operator, fd.Value)
                                               select datatuple;

                                    //temp  = list.Intersect<AbstractTuple>(temp as IEnumerable<AbstractTuple>).ToList();
                                    dataTupleList = list.ToList();
                                }
                            }

                            //dataTupleList = temp;

                        }
                    }
                }

                if (command.SortDescriptors.Count > 0)
                {
                    foreach (SortDescriptor sort in command.SortDescriptors)
                    {

                        string direction = sort.SortDirection.ToString();

                        // get id as long from filtername
                        Regex r = new Regex("(\\d+)");
                        long id = Convert.ToInt64(r.Match(sort.Member).Value);

                        if (direction.Equals("Ascending"))
                        {
                            var list = from datatuple in dataTupleList
                                       let val = datatuple.VariableValues.Where(p => p.Variable.Id.Equals(id)).FirstOrDefault()
                                       orderby GridHelper.CastVariableValue(val.Value, val.DataAttribute.DataType.SystemType) ascending
                                       select datatuple;

                            dataTupleList = list.ToList();
                        }
                        else
                        if (direction.Equals("Descending"))
                        {
                            var list = from datatuple in dataTupleList
                                       let val = datatuple.VariableValues.Where(p => p.Variable.Id.Equals(id)).FirstOrDefault()
                                       orderby GridHelper.CastVariableValue(val.Value, val.DataAttribute.DataType.SystemType) descending
                                       select datatuple;

                            dataTupleList = list.ToList();
                        }
                    }

                }

                return dataTupleList;
            }

            return null;

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

        public void SetCommand(string filters, string orders)
        {
            Session["Filter"] = GridHelper.ConvertToGridCommand(filters, orders);
        }

        #endregion

        #endregion

        #region download FileStream

        public ActionResult DownloadFile(long id, string path, string mimeType)
        {
            if (hasUserRights(id, RightType.Read))
            {

                string title = path.Split('\\').Last();
                string message = string.Format("file was downloaded");
                LoggerFactory.LogCustom(message);

                return File(Path.Combine(AppConfiguration.DataPath, path), mimeType, title);
            }

            return Content("User has no rights.");
        }

        public ActionResult DownloadAllFiles(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {

                try
                {


                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    datasetVersion.Dataset.MetadataStructure = msm.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id);

                    //TITLE
                    string title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title);
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


                    return File(zipPath, "application/zip", title + ".zip");
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            return Content("User has no rights.");

        }

        #endregion
        #endregion

        #region datastructure

        [GridAction]
        public ActionResult _CustomDataStructureBinding(GridCommand command, long datasetID)
        {
            long id = datasetID;
            DatasetManager dm = new DatasetManager();
            if (dm.IsDatasetCheckedIn(id))
            {
                DatasetVersion ds = dm.GetDatasetLatestVersion(id);
                if (ds != null)
                {
                    DataStructureManager dsm = new DataStructureManager();
                    this.Disposables.Add(dsm);

                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(ds.Dataset.DataStructure.Id);
                    dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);
                    //StructuredDataStructure sds = (StructuredDataStructure)(ds.Dataset.DataStructure.Self);
                    DataTable table = SearchUIHelper.ConvertStructuredDataStructureToDataTable(sds);

                    return View(new GridModel(table));
                }

            }
            else
            {
                ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
            }

            return View(new GridModel(new DataTable()));
        }

        public ActionResult ShowPreviewDataStructure(long datasetID)
        {
            DatasetManager dm = new DatasetManager();
            try
            {
                DatasetVersion ds = dm.GetDatasetLatestVersion(datasetID);
                DataStructureManager dsm = new DataStructureManager();
                this.Disposables.Add(dsm);

                DataStructure dataStructure = dsm.AllTypesDataStructureRepo.Get(ds.Dataset.DataStructure.Id);


                long id = (long)datasetID;

                Tuple<DataStructure, long> m = new Tuple<DataStructure, long>(
                    dataStructure,
                    id
                    );

                return PartialView("_previewDatastructure", m);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion

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

        #endregion

        #region Permissions

        public ActionResult Subjects(long dataId)
        {
            ViewData["DataId"] = dataId;

            return PartialView("_SubjectsPartial");
        }

        [GridAction]
        public ActionResult Subjects_Select(long dataId)
        {
            EntityManager entityManager = new EntityManager();
            //PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            List<DatasetPermissionGridRowModel> subjects = new List<DatasetPermissionGridRowModel>();

            // TODO: refactor ( SVEN )
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            //IQueryable<Subject> data = subjectManager.GetAllSubjects();
            //data.ToList().ForEach(s => subjects.Add(DatasetPermissionGridRowModel.Convert(dataId, entityManager.GetEntityById(1), s, permissionManager.GetAllRights(s.Id, 1, dataId).ToList())));


            return View(new GridModel<DatasetPermissionGridRowModel> { Data = subjects });
        }

        // TODO: refactor ( SVEN )
        //public DataPermission CreateDataPermission(long subjectId, long entityId, long dataId, int rightType)
        //{
        //    PermissionManager permissionManager = new PermissionManager();

        //    return permissionManager.CreateDataPermission(subjectId, entityId, dataId, (RightType)rightType);
        //}

        //public bool DeleteDataPermission(long subjectId, long entityId, long dataId, int rightType)
        //{
        //    PermissionManager permissionManager = new PermissionManager();

        //    permissionManager.DeleteDataPermission(subjectId, entityId, dataId, (RightType)rightType);

        //    return true;
        //}

        #endregion


        #region helper

        private bool hasUserRights(long entityId, RightType rightType)
        {
            #region security permissions and authorisations check

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            return entityPermissionManager.HasRight<User>(GetUsernameOrDefault(), "Dataset", typeof(Dataset), entityId, rightType);

            #endregion security permissions and authorisations check
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

        #endregion
    }
}
