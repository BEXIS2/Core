using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Web.Shell.Areas.DDM.Helpers;
using BExIS.Web.Shell.Areas.DDM.Models;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Vaiona.Utils.Cfg;
using BExIS.IO;
using Ionic.Zip;
using BExIS.Security.Services.Objects;
using System.Xml.Linq;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dim.Entities;
using BExIS.Dim.Helpers;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using BExIS.Xml.Services;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Subjects;
using Vaiona.Web.Mvc.Models;
using BExIS.Security.Entities.Authorization;
using Newtonsoft.Json;
using Vaiona.Web.Extensions;
using System.Net;
using System.Net;
using System.Web.Script.Serialization;
using System.Xml;
using BExIS.Web.Shell.Areas.RPM.Controllers;
using BExIS.Web.Shell.Areas.RPM.Models;


namespace BExIS.Web.Shell.Areas.DDM.Controllers
{
    public class DataController : Controller
    {

        public ActionResult ShowData(long id)
        {

            DatasetManager dm = new DatasetManager();

            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();
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
                ViewAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 1, id, RightType.View),
                GrantAccess =
                    permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 1, id, RightType.Grant)
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
            setAdditionalFunctions();

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
            CreateTaskmanager TaskManager = new CreateTaskmanager();

            Dictionary<string, ActionInfo> actions = new Dictionary<string, ActionInfo>();

            //set function actions of COPY, RESET,CANCEL,SUBMIT
            ActionInfo copyAction = new ActionInfo();
            copyAction.ActionName = "Index";
            copyAction.ControllerName = "CreateDataset";
            copyAction.AreaName = "DCM";

            ActionInfo resetAction = new ActionInfo();
            resetAction.ActionName = "Reset";
            resetAction.ControllerName = "Form";
            resetAction.AreaName = "DCM";

            ActionInfo cancelAction = new ActionInfo();
            cancelAction.ActionName = "Cancel";
            cancelAction.ControllerName = "Form";
            cancelAction.AreaName = "DCM";

            ActionInfo submitAction = new ActionInfo();
            submitAction.ActionName = "Submit";
            submitAction.ControllerName = "CreateDataset";
            submitAction.AreaName = "DCM";


            TaskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, cancelAction);
            TaskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, copyAction);
            TaskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, resetAction);
            TaskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, submitAction);

            Session["CreateDatasetTaskmanager"] = TaskManager;
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

                    bool downloadAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 1,
                        datasetID, RightType.Download);

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
                Session["Columns"] = columns.Replace("ID","").Split(',');

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

                        // if filter selected
                        if (filterInUse())
                        {
                            #region generate a subset of a dataset
                            //ToDo filter datatuples

                            OutputDataManager ioOutputDataManager = new OutputDataManager();
                            path = ioOutputDataManager.GenerateExcelFile(id, title);

                            return File(path, "application/xlsm", title + ext);

                            #endregion
                        }

                        //filter not in use
                        else
                        {
                            OutputDataManager outputDataManager = new OutputDataManager();
                            path = outputDataManager.GenerateExcelFile(id, title);  

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

                        // if filter selected
                        if (filterInUse())
                        {
                            #region generate a subset of a dataset


                            String[] visibleColumns = null;

                            if (Session["Columns"] != null)
                                visibleColumns = (String[])Session["Columns"];

                            path = ioOutputDataManager.GenerateAsciiFile(id, title,"text/csv",visibleColumns);

                            return File(path, "text/csv", title + ext);
                            #endregion
                        }
                        else
                        {
                            path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/csv");

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

                        // if filter selected
                        if (filterInUse())
                        {
                            #region generate a subset of a dataset


                            String[] visibleColumns = null;

                            if (Session["Columns"] != null)
                                visibleColumns = (String[])Session["Columns"];

                            path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/plain", visibleColumns);

                            return File(path, "text/csv", title + ext);
                            #endregion
                        }
                        else
                        {
                            path = ioOutputDataManager.GenerateAsciiFile(id, title, "text/plain");

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
                    if ((Session["Filter"] != null || Session["Columns"] != null)  && !(bool)Session["DownloadFullDataset"])
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

        public ActionResult DownloadFile(string path,string mimeType)
        {
            string title = path.Split('\\').Last();
            return File(Path.Combine(AppConfiguration.DataPath, path),mimeType, title);
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
                     
                string zipPath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(),title + ".zip");

            
                if (FileHelper.FileExist(zipPath))
                {
                    if (FileHelper.WaitForFile(zipPath))
                    {
                        FileHelper.Delete(zipPath);
                    }
                }

                ZipFile zip = new ZipFile();

                foreach( ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    string path = Path.Combine(AppConfiguration.DataPath,cd.URI);
                    string name = cd.URI.Split('\\').Last();

                    zip.AddFile(path, "");      
                }

                zip.Save(zipPath);

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
                    ModelState.AddModelError(String.Empty,"Dataset is just in processing.");
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
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            List<DatasetPermissionGridRowModel> subjects = new List<DatasetPermissionGridRowModel>();

            IQueryable<Subject> data = subjectManager.GetAllSubjects();
            data.ToList().ForEach(s => subjects.Add(DatasetPermissionGridRowModel.Convert(dataId, entityManager.GetEntityById(1), s, permissionManager.GetAllRights(s.Id, 1, dataId).ToList())));

            return View(new GridModel<DatasetPermissionGridRowModel> { Data = subjects });
        }

        public DataPermission CreateDataPermission(long subjectId, long entityId, long dataId, int rightType)
        {
            PermissionManager permissionManager = new PermissionManager();

            return permissionManager.CreateDataPermission(subjectId, entityId, dataId, (RightType)rightType);
        }

        public bool DeleteDataPermission(long subjectId, long entityId, long dataId, int rightType)
        {
            PermissionManager permissionManager = new PermissionManager();

            permissionManager.DeleteDataPermission(subjectId, entityId, dataId, (RightType)rightType);

            return true;
        }

        #endregion

        #region submission

        public ActionResult publishData(long datasetId, long datasetVersionId=-1)
        {
            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            ShowPublishDataModel model = new ShowPublishDataModel();
            model.DataRepositories = publishingManager.DataRepositories;
            model.DatasetId = datasetId;

            if (datasetVersionId == -1)
            {
                DatasetManager datasetManager = new DatasetManager();
                datasetVersionId = datasetManager.GetDatasetLatestVersion(datasetId).Id;
            }

            //todo check if datasetversion id is correct

            foreach (DataRepository repo in publishingManager.DataRepositories)
            {
                string path = publishingManager.GetDirectoryPath(datasetId, repo);
                if (Directory.Exists(path))
                {
                    string[] filepaths = Directory.GetFiles(path, "*.zip");

                    foreach (var filepath in filepaths)
                    {

                            FileInfo fi = new FileInfo(filepath);

                        var creationTime = fi.CreationTimeUtc;
                        var tmpFileNameSplit = fi.Name.Split('_');
                        long version = 0;
                        if (tmpFileNameSplit != null && tmpFileNameSplit.Length > 0)
                        {
                            string versionAsString = tmpFileNameSplit[1];
                            version = Convert.ToInt64(versionAsString);
                        }

                        model.RepoFilesDictionary.Add(
                            new publishedFileModel()
                            {
                                DatasetId = datasetId,
                                DatasetVersionId = version,
                                DataRepository = repo,
                                CreationDate = creationTime
                            });
                    }
                }
            }

            return PartialView("_showPublishDataView", model);
        }

        public ActionResult LoadDataRepoRequirementView(string datarepo, long datasetid)
        {
            DataRepoRequirentModel model = new DataRepoRequirentModel();
            model.DatasetId = datasetid;

            //get datarepos
            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            // datasetversion
            DatasetManager dm = new DatasetManager();
            long version = dm.GetDatasetLatestVersion(datasetid).Id;
            model.DatasetVersionId = version;
            if (publishingManager.DataRepositories.Any(d => d.Name.Equals(datarepo)))
            {
                DataRepository dp =
                    publishingManager.DataRepositories.Where(d => d.Name.Equals(datarepo)).FirstOrDefault();

                if (dp != null) model.DataRepository = dp;

                if (publishingManager.Exist(datasetid, version, dp))
                {
                    model.Exist = true;
                }
                else
                {
                    #region metadata

                    // if no conversion is needed
                    if (String.IsNullOrEmpty(dp.ReqiuredMetadataStandard))
                    {
                        model.IsMetadataConvertable = true;
                    }
                    else
                    {
                        //if convertion check ist needed
                        //get all export attr from metadata structure
                        List<string> exportNames = XmlDatasetHelper.GetAllTransmissionInformation(datasetid, TransmissionType.mappingFileExport,AttributeNames.name).ToList();
                        if (exportNames.Contains(dp.ReqiuredMetadataStandard)) model.IsMetadataConvertable = true;
                    }

                    #endregion

                    #region primary Data

                    if (dp.PrimaryDataFormat.ToLower().Contains("text/plain") ||
                        dp.PrimaryDataFormat.ToLower().Contains("text/csv") ||
                        dp.PrimaryDataFormat.ToLower().Contains("application/excel") ||
                        String.IsNullOrEmpty(dp.PrimaryDataFormat))
                    {
                        model.IsDataConvertable = true;
                    }

                        #endregion
                }
            }

            return PartialView("_dataRepositoryRequirementsView", model);
        }

        public JsonResult CheckExportPossibility(string datarepo, long datasetid)
        {

            bool isDataConvertable = false;
            bool isMetadataConvertable = false;

            //get datarepos
            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            // datasetversion
            DatasetManager dm = new DatasetManager();
            long version = dm.GetDatasetLatestVersion(datasetid).Id;

            if (publishingManager.DataRepositories.Any(d => d.Name.Equals(datarepo)))
            {
                DataRepository dp =
                    publishingManager.DataRepositories.Where(d => d.Name.Equals(datarepo)).FirstOrDefault();


                if (publishingManager.Exist(datasetid, version, dp))
                {
                    //model.Exist = true;
                }
                else
                {
                    #region metadata

                    // if no conversion is needed
                    if (String.IsNullOrEmpty(dp.ReqiuredMetadataStandard))
                    {
                        //model.IsMetadataConvertable = true;
                        isMetadataConvertable = true;
                    }
                    else
                    {
                        //if convertion check ist needed
                        //get all export attr from metadata structure
                        List<string> exportNames = XmlDatasetHelper.GetAllTransmissionInformation(datasetid, TransmissionType.mappingFileExport, AttributeNames.name).ToList();
                        if (exportNames.Contains(dp.ReqiuredMetadataStandard))
                            isMetadataConvertable = true;

                    }

                    #endregion

                    #region primary Data

                    //todo need a check if the primary data is structured or not, if its unstructured also export should be possible
                    

                    if (dp.PrimaryDataFormat.ToLower().Contains("text/plain") ||
                        dp.PrimaryDataFormat.ToLower().Contains("text/csv") ||
                        dp.PrimaryDataFormat.ToLower().Contains("application/excel") ||
                        String.IsNullOrEmpty(dp.PrimaryDataFormat))
                    {
                        isDataConvertable = true;
                    }

                    #endregion
                }
            }

            return (isMetadataConvertable && isDataConvertable)?Json(true):Json(false);
        }

        public ActionResult DownloadZip(string datarepo ,long datasetid, long datasetversionid)
        {
            string path = "";

            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            DataRepository dataRepo =
                publishingManager.DataRepositories.Where(repository => repository.Name.Equals(datarepo))
                    .FirstOrDefault();

            if (dataRepo != null)
            {
                string zipName = publishingManager.GetZipFileName(datasetid, datasetversionid);
                string zipPath = publishingManager.GetDirectoryPath(datasetid, dataRepo);
                string zipFilePath = Path.Combine(zipPath, zipName);

                return File(zipFilePath, "application/zip", zipName);
            }

            return null;
        }

        public async Task <ActionResult> PrepareData(long datasetId, string datarepo)
        {
            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            DatasetManager datasetManager = new DatasetManager();

            if (datasetManager.IsDatasetCheckedIn(datasetId))
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                // convert metadata
                DataRepository dataRepository =
                    publishingManager.DataRepositories.Where(d => d.Name.Equals(datarepo)).FirstOrDefault();

                if (dataRepository != null)
                {
                    OutputMetadataManager.GetConvertedMetadata(datasetId, TransmissionType.mappingFileExport,
                        dataRepository.ReqiuredMetadataStandard);

                    // get primary data
                    // check the data sturcture type ...
                    if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                    {
                        OutputDataManager odm = new OutputDataManager();
                        // apply selection and projection

                        string title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title);

                        odm.GenerateAsciiFile(datasetId, title, dataRepository.PrimaryDataFormat);
                    }

                    string zipName = publishingManager.GetZipFileName(datasetId, datasetVersion.Id);
                    string zipPath = publishingManager.GetDirectoryPath(datasetId, dataRepository);
                    string zipFilePath = Path.Combine(zipPath, zipName);

                    FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(zipFilePath));



                    if (FileHelper.FileExist(zipFilePath))
                    {
                        if (FileHelper.WaitForFile(zipFilePath))
                        {
                            FileHelper.Delete(zipFilePath);
                        }
                    }

                   

                    // add datastructure
                    //ToDo put that functiom to the outputDatatructureManager
                    #region datatructure

                    DataStructureManager dataStructureManager = new DataStructureManager();

                    long dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                    DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                    if (dataStructure != null)
                    {
                        // get datastructure as json
                        //ToDo it is not allowed to call a action from a other controller, so we need to generate a function in the outputDatastructureManager of generating a structure 
                        // -> its not solve right now, because of sturtcure is using also entities under rpm/models
                        // we need to find a way to switch this functionality to the io libary an calling that from the api and here
                        BExIS.Web.Shell.Areas.RPM.Controllers.StructuresController dataStructureApi = new StructuresController();

                        try
                        {
                            string dynamicPathOfDS = "";
                            dynamicPathOfDS = storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion,
                                "datastructure", ".txt");
                            string datastructureFilePath = AsciiWriter.CreateFile(dynamicPathOfDS);
                            Structure structure = dataStructureApi.Get(datasetId);

                            string json = JsonConvert.SerializeObject(structure);

                            AsciiWriter.AllTextToFile(datastructureFilePath, json);


                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    #endregion

                    ZipFile zip = new ZipFile();

                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                        string name = cd.URI.Split('\\').Last();

                        if (FileHelper.FileExist(path))
                        {
                            zip.AddFile(path, "");
                        }
                    }


                    // add xsd of the metadata schema
                    string xsdDirectoryPath = OutputMetadataManager.GetSchemaDirectoryPath(datasetId);
                    if(Directory.Exists(xsdDirectoryPath))
                        zip.AddDirectory(xsdDirectoryPath,"Schema");

                    XmlDocument manifest = OutputDatasetManager.GenerateManifest(datasetId, datasetVersion.Id);

                    if (manifest != null)
                    {
                        string dynamicManifestFilePath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId,
                            datasetVersion.Id, "manifest", ".xml");
                        string fullFilePath = Path.Combine(AppConfiguration.DataPath, dynamicManifestFilePath);

                        manifest.Save(fullFilePath);
                        zip.AddFile(fullFilePath,"");

                    }


                    zip.Save(zipFilePath);
                }
            }

            //var product = await GetWSObject<object>();


            return RedirectToAction("publishData", new {datasetId});
        }

        public async Task<string> GetWSObject<T>()
        {

            string url =
                @"http://gfbio-pub2.inf-bb.uni-jena.de:8080/api/jsonws/GFBioProject-portlet.researchobject/get-research-object-by-id/request-json/%5B%7B%22researchobjectid%22%3A3%20%7D%5D";

            string returnValue = "";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //test@testerer.de:WSTest
                    var byteArray = Encoding.ASCII.GetBytes("broker.agent@gfbio.org:AgentPhase2");

                    // "basic "+ Convert.ToBase64String(byteArray)
                    AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = ahv;


                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    returnValue = ((HttpResponseMessage)response).Content.ReadAsStringAsync().Result;
                }
                return returnValue;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

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
