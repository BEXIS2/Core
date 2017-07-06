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
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : Controller
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
                dsv = dm.GetDatasetLatestVersion(id);

                MetadataStructureManager msm = new MetadataStructureManager();
                dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                title = XmlDatasetHelper.GetInformation(dsv, NameAttributeValues.title);
                metadataStructureId = dsv.Dataset.MetadataStructure.Id;
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
                ViewAccess = entityPermissionManager.HasRight(HttpContext.User.Identity.Name, typeof(User), "Dataset", typeof(Dataset), id, RightType.Read),
                GrantAccess = entityPermissionManager.HasRight(HttpContext.User.Identity.Name, typeof(User), "Dataset", typeof(Dataset), id, RightType.Grant)
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
            // ToDo Modularity DDM -> DCM Add open function to set the actions from a CreateTaskmanager and add this to the manifestfile
            setAdditionalFunctions();

            //ToDO Modularity from ddm -> dcm

            return RedirectToAction("LoadMetadataFromExternal", "Form", new
            {
                area = "DCM",
                entityId,
                title,
                metadatastructureId,
                datastructureId,
                researchplanId,
                sessionKeyForMetadata
            });
        }

        private void setAdditionalFunctions()
        {
            // ToDO Modularity DDM -> DCM
            // ToDo Modularity DDM -> DCM Add open function to set the actions from a CreateTaskmanager and add this to the manifestfile
            // commented by Javad during porting.
            // Dcm.CreateDatasetWizard.CreateTaskmanager TaskManager = new CreateTaskmanager();

            //Dictionary<string, ActionInfo> actions = new Dictionary<string, ActionInfo>();

            ////set function actions of COPY, RESET,CANCEL,SUBMIT
            //ActionInfo copyAction = new ActionInfo();
            //copyAction.ActionName = "Copy";
            //copyAction.ControllerName = "CreateDataset";
            //copyAction.AreaName = "DCM";

            //ActionInfo resetAction = new ActionInfo();
            //resetAction.ActionName = "Reset";
            //resetAction.ControllerName = "Form";
            //resetAction.AreaName = "DCM";

            //ActionInfo cancelAction = new ActionInfo();
            //cancelAction.ActionName = "Cancel";
            //cancelAction.ControllerName = "Form";
            //cancelAction.AreaName = "DCM";

            //ActionInfo submitAction = new ActionInfo();
            //submitAction.ActionName = "Submit";
            //submitAction.ControllerName = "CreateDataset";
            //submitAction.AreaName = "DCM";


            //TaskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, cancelAction);
            //TaskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, copyAction);
            //TaskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, resetAction);
            //TaskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, submitAction);

            //Session["CreateDatasetTaskmanager"] = TaskManager;
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
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                DataStructureManager dsm = new DataStructureManager();


                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                DataStructure ds = dsm.AllTypesDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

                //permission download
                PermissionManager permissionManager = new PermissionManager();
                SubjectManager subjectManager = new SubjectManager();

                // TODO: refactor
                bool downloadAccess = false; // permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 1, datasetID, RightType.Download);

                //TITLE
                string title = XmlDatasetHelper.GetInformation(dsv, NameAttributeValues.title);

                if (ds.Self.GetType() == typeof(StructuredDataStructure))
                {

                    List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv, 0, 100);
                    //List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv);

                    DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);

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

                List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv, command.Page - 1,
                    command.PageSize);
                //List<AbstractTuple> dataTuples2 = dm.DataTupleRepo.Query(dt => dt.DatasetVersion.Equals(dsv))
                //    .Skip((command.Page - 1)*command.PageSize)
                //    .Take(command.PageSize).ToList();

                Session["gridTotal"] = dm.GetDatasetVersionEffectiveTupleCount(dsv);

                DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);
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

        public ActionResult DownloadAsCsvData(long id)
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

        public ActionResult DownloadAsTxtData(long id)
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

        public ActionResult DownloadFile(string path, string mimeType)
        {
            string title = path.Split('\\').Last();
            string message = string.Format("file was downloaded");
            LoggerFactory.LogCustom(message);

            return File(Path.Combine(AppConfiguration.DataPath, path), mimeType, title);
        }

        public ActionResult DownloadAllFiles(long id)
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


        #endregion
    }
}
