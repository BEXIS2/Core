using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
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
using System.IO.Compression;
using Ionic.Zip;
using BExIS.Security.Services.Objects;
using BExIS.Dlm.Entities.MetadataStructure;
using System.Xml;
using System.Xml.Linq;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using BExIS.Xml.Services;
using BExIS.Dlm.Services.MetadataStructure;
using Vaiona.Logging.Aspects;

namespace BExIS.Web.Shell.Areas.DDM.Controllers
{
    public class DataController : Controller
    {
        //
        // GET: /DDM/Data/

        public ActionResult Index()
        {
           
            
            return View();
        }

        public ActionResult ShowData(long id)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion dsv = dm.GetDatasetLatestVersion(id);

            MetadataStructureManager msm = new MetadataStructureManager();
            dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

            string title = XmlDatasetHelper.GetInformation(dsv, AttributeNames.title);

            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();


            ShowDataModel model = new ShowDataModel()
            {
                Id = id,
                Title = title,
                ViewAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 1, id, RightType.View),
            };


            return View(model);
        }

        #region metadata

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetID"></param>
        /// <returns>model</returns>
        public ActionResult ShowMetaData(int datasetID)
        {
            ShowMetadataModel model = new ShowMetadataModel();

            try
            {
                DatasetManager dm = new DatasetManager();
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);

                MetadataStructureManager msm = new MetadataStructureManager();
                dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

            //get title
            model.Title = XmlDatasetHelper.GetInformation(dsv,AttributeNames.title);
            model.Description = XmlDatasetHelper.GetInformation(dsv, AttributeNames.description);

            #region create table
            XDocument xDoc = XmlUtility.ToXDocument(dsv.Metadata);

            //get a list of MetadataPackageUsages
            IEnumerable<XElement> MetadataPackageUsageList = helper.GetElementsByAttribute(xDoc, "type", BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage.ToString());
                
            //For Each MetadataPackageUsage
            foreach (XElement packageUsage in MetadataPackageUsageList)
            {
                PackageUsageModel puElement = new PackageUsageModel();
                puElement.Name = packageUsage.Attribute("name").Value;

                // get childrens of
                List<XElement> childrens = XmlUtility.GetChildren(packageUsage).ToList();

                foreach (XElement child in childrens)
                {
                    puElement.Attributes.Add(
                            GetModelFromElement(child)
                        );
                }

                model.PU.Add(puElement);
            }
            #endregion
                

            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            return PartialView(model);
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

            [MeasurePerformance]
            public ActionResult ShowPrimaryData(long datasetID)
            {
                Session["Filter"] = null;
                Session["Columns"] = null;
                Session["DownloadFullDataset"] = false;
                ViewData["DownloadOptions"] = null;
         

                DatasetManager dm = new DatasetManager();
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                DataStructureManager dsm = new DataStructureManager();

                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                DataStructure ds = dsm.AllTypesDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

                //permission download
                PermissionManager permissionManager = new PermissionManager();
                SubjectManager subjectManager = new SubjectManager();
                
                bool downloadAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 1, datasetID, RightType.Download);

                //TITLE
                string title = XmlDatasetHelper.GetInformation(dsv, AttributeNames.title);

                if (ds.Self.GetType() == typeof(StructuredDataStructure))
                {

                    List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv,0,100);
                    //List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv);

                    DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);

                    Session["gridTotal"] = dm.GetDatasetVersionEffectiveTupleCount(dsv);

                    return PartialView(ShowPrimaryDataModel.Convert(datasetID, title, sds, table, downloadAccess));

                    //return PartialView(new ShowPrimaryDataModel());
                }

                if (ds.Self.GetType() == typeof(UnStructuredDataStructure))
                {
                    return PartialView(ShowPrimaryDataModel.Convert(datasetID,title, ds, SearchUIHelper.GetContantDescriptorFromKey(dsv, "unstructuredData"),downloadAccess));
                }

                return null;
            }

            #region server side

            [GridAction(EnableCustomBinding = true)]
            [MeasurePerformance]
            public ActionResult _CustomPrimaryDataBinding(GridCommand command, int datasetID)
            {

                Session["Filter"] = command;

                DatasetManager dm = new DatasetManager();
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);


                List<AbstractTuple> dataTuples = dm.GetDatasetVersionEffectiveTuples(dsv,command.Page-1,command.PageSize);

                Session["gridTotal"] = dm.GetDatasetVersionEffectiveTupleCount(dsv);

                DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dataTuples);
                GridModel model = new GridModel(table);
                model.Total = Convert.ToInt32(Session["gridTotal"]);// (int)Session["gridTotal"];

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
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));
                    
                    string path = "";

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        // I have changed all the references to the class "DataTuple" into the class "AbstractTuple" to support previous versions' tuples too.
                        //the AbstractTuple.TupleType indicates whether the tuple is original or comming from the history
                        // if(datatuples.First().TupleType == DataTupleType.Original) ...
                        List<AbstractTuple> datatuples = GetFilteredDataTuples(datasetVersion); 

                            if (Session["Columns"] != null)
                                writer.VisibleColumns = (String[])Session["Columns"];

                            long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                            path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                            writer.AddDataTuplesToTemplate(datatuples, path, datastuctureId);

                            return File(path, "application/xlsm", title + ext);
                        #endregion
                    }

                    //filter not in use
                    else
                    {
                        //excel allready exist
                        if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals("generated")) > 0)
                        {
                            #region FileStream exist

                                ContentDescriptor contentdescriptor = datasetVersion.ContentDescriptors.Where(p => p.Name.Equals("generated")).FirstOrDefault();
                                path = Path.Combine(AppConfiguration.DataPath, contentdescriptor.URI);

                                long version = datasetVersion.Id;
                                long versionNrGeneratedFile = Convert.ToInt64(contentdescriptor.URI.Split('\\').Last().Split('_')[1]);

                                // check if FileStream exist
                                if (FileHelper.FileExist(path) && version == versionNrGeneratedFile)
                                {
                                    return File(path, contentdescriptor.MimeType, title + ext);
                                }

                                // if not generate
                                else
                                {
                                    List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                                    long datastuctureId = datasetVersion.Dataset.DataStructure.Id;
                                    path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                                    storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);
                                    writer.AddDataTuplesToTemplate(datatupleIds, path, datastuctureId);

                                    return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", title + ext);
                                }

                            #endregion
                        }
                        // not exist needs to generated
                        else
                        {
                            #region FileStream not exist

                                List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                                long datastuctureId = datasetVersion.Dataset.DataStructure.Id;
                                path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                                storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);
                                writer.AddDataTuplesToTemplate(datatupleIds, path, datastuctureId);

                                return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", title + ext);

                            #endregion
                        }

                    }
                }
                
                public ActionResult DownloadAsCsvData(long id)
                {
                    string ext = ".csv";

                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";

                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset
                        List<AbstractTuple> datatuples = GetFilteredDataTuples(datasetVersion);

                        long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                        path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                        if (Session["Columns"] != null)
                            writer.VisibleColumns = (String[])Session["Columns"];

                        writer.AddDataTuples(datatuples, path, datastuctureId);


                        return File(path, "text/csv", title + ext);
                        #endregion
                    }
                    else
                    {
                        List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                        long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                        //csv allready exist
                        if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals("generatedCSV")) > 0)
                        {
                            #region FileStream exist

                            ContentDescriptor contentdescriptor = datasetVersion.ContentDescriptors.Where(p => p.Name.Equals("generatedCSV")).FirstOrDefault();
                            path = Path.Combine(AppConfiguration.DataPath, contentdescriptor.URI);

                            if (FileHelper.FileExist(path))
                            {
                                return File(path, contentdescriptor.MimeType, title + ext);
                            }
                            else
                            {
                                
                                path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext ,writer);

                                storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);

                                writer.AddDataTuples(datatupleIds, path, datastuctureId);

                                return File(path, "text/csv", title + ".csv");
                            }

                            #endregion

                        }
                        // not exist needs to generated
                        else
                        {
                            #region FileStream not exist

                            path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                            storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);

                            writer.AddDataTuples(datatupleIds, path, datastuctureId);

                            return File(path, "text/csv", title + ".csv");

                            #endregion
                        }
                    }

                }

                public ActionResult DownloadAsTxtData(long id)
                {
                    string ext = ".txt";

                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.tab);
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";

                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                            List<AbstractTuple> datatuples = GetFilteredDataTuples(datasetVersion);

                            long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                            path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                            if (Session["Columns"] != null)
                                writer.VisibleColumns = (String[])Session["Columns"];

                            writer.AddDataTuples(datatuples, path, datastuctureId);


                            return File(path, "text/plain", title + ".txt");

                        #endregion
                    }
                    else
                    {

                        List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                        long datastuctureId = datasetVersion.Dataset.DataStructure.Id;
                       

                        //csv allready exist
                        if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals("generatedTXT")) > 0 || datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(path)) == 1)
                        {
                            #region FileStream exist

                                ContentDescriptor contentdescriptor = datasetVersion.ContentDescriptors.Where(p => p.Name.Equals("generatedTXT")).FirstOrDefault();
                                path = Path.Combine(AppConfiguration.DataPath, contentdescriptor.URI);

                                if (FileHelper.FileExist(path))
                                {
                                    // return FileStream based on loaded link from content discriptor
                                    return File(path, contentdescriptor.MimeType, title + ext);
                                }
                                else
                                {
                                    path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                                    //generate a entry in the ContentDiscriptor
                                    storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);

                                    // Add DataStructure and Datatuples to FileStream
                                    writer.AddDataTuples(datatupleIds, path, datastuctureId);

                                    // return created FileStream
                                    return File(path, "text/plain", title + ext);
                                }

                            #endregion
                        }
                        // not exist needs to generated
                        else
                        {
                            #region FileStream not exist

                                path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                                //generate a entry in the ContentDiscriptor
                                storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);

                                // Add DataStructure and Datatuples to FileStream
                                writer.AddDataTuples(datatupleIds, path, datastuctureId);

                                // return created FileStream
                                return File(path, "text/plain", title + ext);

                            #endregion
                        }
                    }

                }

                #region helper

                    private string generateDownloadFile(long id, long datasetVersionOrderNo,long dataStructureId, string title, string ext, DataWriter writer)
                    {
                        if (ext.Equals(".csv") || ext.Equals(".txt"))
                        {
                            AsciiWriter asciiwriter = (AsciiWriter)writer;
                            return asciiwriter.CreateFile(id, datasetVersionOrderNo, dataStructureId, title, ext);
                        }
                        else
                        if(ext.Equals(".xlsm"))
                        {
                            ExcelWriter excelwriter = (ExcelWriter)writer;
                            return excelwriter.CreateFile(id, datasetVersionOrderNo, dataStructureId, title, ext);
                        }

                        return "";
                    }

                    private void storeGeneratedFilePathToContentDiscriptor(long datasetId,DatasetVersion datasetVersion, string title, string ext, DataWriter writer)
                    {
                       
                        string name = "";
                        string mimeType = "";

                        if (ext.Contains("csv"))
                        {
                            name = "generatedCSV";
                            mimeType = "text/csv"; 
                        }

                        if (ext.Contains("txt"))
                        {
                            name = "generatedTXT";
                            mimeType = "text/plain";
                        }

                        if (ext.Contains("xlsm"))
                        {
                            name = "generated";
                            mimeType = "application/xlsm";
                        }

                        // create the generated FileStream and determine its location
                        string dynamicPath = writer.GetDynamicStorePath(datasetId, datasetVersion.Id, title, ext);
                        //Register the generated data FileStream as a resource of the current dataset version
                        ContentDescriptor generatedDescriptor = new ContentDescriptor()
                        {
                            OrderNo = 1,
                            Name = name,
                            MimeType = mimeType,
                            URI = dynamicPath,
                            DatasetVersion = datasetVersion,
                        };

                        if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(generatedDescriptor.Name)) > 0)
                        {   // remove the one contentdesciptor 
                            foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                            {
                                if (cd.Name == generatedDescriptor.Name)
                                {
                                    cd.URI = generatedDescriptor.URI;
                                }
                            }
                        }
                        else
                        {
                            // add current contentdesciptor to list
                            datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                        }

                        DatasetManager dm = new DatasetManager();
                        dm.EditDatasetVersion(datasetVersion, null, null, null);
                    }
                    
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
                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    datasetVersion.Dataset.MetadataStructure = msm.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id);

                    //TITLE
                    string title = XmlDatasetHelper.GetInformation(datasetVersion, AttributeNames.title);
                     
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
            
            #endregion
        #endregion

        #region datastructure

            [GridAction]
            public ActionResult _CustomDataStructureBinding(GridCommand command, long datasetID)
            {
                long id = datasetID;
                DatasetManager dm = new DatasetManager();
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

                return View(new GridModel(new DataTable()));
            }

            public ActionResult ShowPreviewDataStructure(long datasetID)
            {
                DatasetManager dm = new DatasetManager();
                DatasetVersion ds = dm.GetDatasetLatestVersion((long)datasetID);
                DataStructureManager dsm = new DataStructureManager();
                DataStructure dataStructure = dsm.AllTypesDataStructureRepo.Get(ds.Dataset.DataStructure.Id);
        

                long id = (long)datasetID;

                Tuple<DataStructure, long> m = new Tuple<DataStructure, long>(
                    dataStructure,
                    id
                   );

                return PartialView("_previewDatastructure", m);
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

    }
}
