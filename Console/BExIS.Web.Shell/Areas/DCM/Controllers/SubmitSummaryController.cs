using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.DCM.Models;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class SubmitSummaryController : Controller
    {
        private TaskManager TaskManager;
        private FileStream Stream;

        //
        // GET: /DCM/Summary/

        [HttpGet]
        public ActionResult Summary(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);
                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            SummaryModel model = new SummaryModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
            {
                model.DatasetId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE))
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {
                model.DataStructureId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TITLE))
            {
                model.DataStructureTitle = TaskManager.Bus[TaskManager.DATASTRUCTURE_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_ID))
            {
                model.ResearchPlanId = Convert.ToInt32(TaskManager.Bus[TaskManager.RESEARCHPLAN_ID].ToString());
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_TITLE))
            {
                model.ResearchPlanTitle = TaskManager.Bus[TaskManager.RESEARCHPLAN_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.TITLE))
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.TITLE].ToString();
            }

            /*
            if (TaskManager.Bus.ContainsKey(TaskManager.AUTHOR))
            {
                model.Author = TaskManager.Bus[TaskManager.AUTHOR].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.OWNER))
            {
                model.Owner = TaskManager.Bus[TaskManager.OWNER].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.PROJECTNAME))
            {
                model.ProjectName = TaskManager.Bus[TaskManager.PROJECTNAME].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.INSTITUTE))
            {
                model.ProjectInstitute = TaskManager.Bus[TaskManager.INSTITUTE].ToString();
            }*/

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Summary(object[] data)
        {

            TaskManager = (TaskManager)Session["TaskManager"];
            SummaryModel model = new SummaryModel();

            model.StepInfo = TaskManager.Current();

            model.ErrorList = FinishUpload(TaskManager);

            if (model.ErrorList.Count > 0)
            {
                #region set summary
                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
                {
                    model.DatasetId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE))
                {
                    model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
                {
                    model.DataStructureId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TITLE))
                {
                    model.DataStructureTitle = TaskManager.Bus[TaskManager.DATASTRUCTURE_TITLE].ToString();
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_ID))
                {
                    model.ResearchPlanId = Convert.ToInt32(TaskManager.Bus[TaskManager.RESEARCHPLAN_ID].ToString());
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_TITLE))
                {
                    model.ResearchPlanTitle = TaskManager.Bus[TaskManager.RESEARCHPLAN_TITLE].ToString();
                }

                #endregion
                //ToDo: remove all changed from dataset and version
                return PartialView(model);

            }
            else
            {
                return null;
            }
        }

        public List<Error> FinishUpload(TaskManager taskManager)
        {
            List<Error> temp = new List<Error>();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID) && TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {

                long id = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                DataStructureManager dsm = new DataStructureManager();
                long iddsd = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
               

                //datatuple list
                List<DataTuple> rows;

                DatasetManager dm = new DatasetManager();
                Dataset ds = dm.GetDataset(id);
                DatasetVersion workingCopy = new DatasetVersion();

                #region structured data

                    if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE) && TaskManager.Bus[TaskManager.DATASTRUCTURE_TYPE].Equals(DataStructureType.Structured))
                    {
                        try
                        {
                            

                            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                            dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                            #region excel reader

                            if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                            {
                                int packageSize = 100000;
                                int counter = 0;

                                ExcelReader reader = new ExcelReader();

                                //schleife
                                do
                                {
                                    Stopwatch packageTime = Stopwatch.StartNew();

                                    counter++;
                                    // open file
                                    Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                                    rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), sds, (int)id, packageSize);

                                    if (reader.ErrorMessages.Count > 0)
                                    {
                                        //model.ErrorList = reader.errorMessages;
                                    }
                                    else
                                    {

                                        //XXX Add packagesize to excel read function
                                        if (dm.IsDatasetCheckedOutFor(ds.Id, GetUserNameOrDefault()) || dm.CheckOutDataset(ds.Id, GetUserNameOrDefault()))
                                        {

                                            workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                                            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                                            {

                                                if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new"))
                                                {
                                                    dm.EditDatasetVersion(workingCopy, rows, null, null);
                                                }

                                                if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("edit"))
                                                {
                                                    if (rows.Count > 0)
                                                    {
                                                        Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                                        splittedDatatuples = UploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy);
                                                        dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                    }
                                                }
                                            }
                                            else
                                            {

                                            }
                                            //dm.CheckInDataset(ds.Id, "upload data from upload wizard", "David");

                                        }
                                    }

                                    Stream.Close();

                                    packageTime.Stop();
                                    Debug.WriteLine("Package : " + counter +" packageTime Time " + packageTime.Elapsed.TotalSeconds.ToString());

                                } while (rows.Count > 0);
                            }

                            #endregion

                            #region ascii reader


                            if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv") ||
                                TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt"))
                            {
                                // open file
                                AsciiReader reader = new AsciiReader();
                                //Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());

                                //DatasetManager dm = new DatasetManager();
                                //Dataset ds = dm.GetDataset(id);

                                Stopwatch totalTime = Stopwatch.StartNew();

                                if (dm.IsDatasetCheckedOutFor(ds.Id, GetUserNameOrDefault()) || dm.CheckOutDataset(ds.Id, GetUserNameOrDefault()))
                                {
                                    workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
                                    int packageSize = 100000;
                                    //schleife

                                    do
                                    {
                                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                                        rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], sds, id, packageSize);
                                        Stream.Close();

                                        if (reader.ErrorMessages.Count > 0)
                                        {
                                            //model.ErrorList = reader.errorMessages;
                                        }
                                        else
                                        {
                                            //model.Validated = true;
                                            Stopwatch dbTimer = Stopwatch.StartNew();

                                            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                                            {
                                                if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new"))
                                                {

                                                    dm.EditDatasetVersion(workingCopy, rows, null, null);
                                                }

                                                if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("edit"))
                                                {
                                                    if (rows.Count > 0)
                                                    {
                                                        Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                                        splittedDatatuples = UploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy);
                                                        dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (rows.Count > 0)
                                                {
                                                    Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                                    splittedDatatuples = UploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy);
                                                    dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                }
                                            }

                                            dbTimer.Stop();
                                            Debug.WriteLine(" db time" + dbTimer.Elapsed.TotalSeconds.ToString());

                                        }

                                    } while (rows.Count > 0);



                                    totalTime.Stop();
                                    Debug.WriteLine(" Total Time " + totalTime.Elapsed.TotalSeconds.ToString());


                                }

                                //Stream.Close();

                            }

                            #endregion

                            // start download generator
                            // filepath
                            string path = "";
                            if (workingCopy != null)
                            {
                                path = GenerateDownloadFile(workingCopy);

                                dm.EditDatasetVersion(workingCopy, null, null, null);
                            }

                            // ToDo: Get Comment from ui and users
                            dm.CheckInDataset(ds.Id, "upload data from upload wizard", GetUserNameOrDefault());

                            // open the excel file and add data tuples
                            AddDatatuplesToFile(ds.Id, sds.Id, path);
                        }
                        catch (Exception e)
                        {

                            temp.Add(new Error(ErrorType.Other, "Can not upload. : " + e.Message));
                            dm.CheckInDataset(ds.Id, "checked in but no update on data tuples", GetUserNameOrDefault());
                        }
                        finally
                        { 
                            
                        }
                    }

                #endregion

                #region unstructured data

                    if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE) && TaskManager.Bus[TaskManager.DATASTRUCTURE_TYPE].Equals(DataStructureType.Unstructured))
                    {

                        workingCopy = dm.GetDatasetLatestVersion(ds.Id);
                        SaveFileInContentDiscriptor(workingCopy);

                        dm.EditDatasetVersion(workingCopy, null, null, null);

                        // ToDo: Get Comment from ui and users
                        dm.CheckInDataset(ds.Id, "upload unstructured data", GetUserNameOrDefault());
                    }

                #endregion


            }
            else
            {
                temp.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
            }

            return temp;
        }


        #region private methods

        public string GetUserNameOrDefault()
        {
            string userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        private string GenerateDownloadFile(DatasetVersion datasetVersion)
        {

            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            //dataset id and data structure id are available via datasetVersion properties,why you are passing them via the BUS? Javad
            long datasetId = Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID]);
            long dataStructureId = Convert.ToInt64(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);

            DatasetManager datasetManager = new DatasetManager();

            string title = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            string ext = ".xlsm";// TaskManager.Bus[TaskManager.EXTENTION].ToString();

            ExcelWriter excelWriter = new ExcelWriter();

            // Move Original File to its permanent location
            String tempPath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
            string originalFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
            string storePath = excelWriter.GetFullStorePathOriginalFile(datasetId, datasetVersion.Id, originalFileName);
            string dynamicStorePath = excelWriter.GetDynamicStorePathOriginalFile(datasetId, datasetVersion.VersionNo, originalFileName);

            //Why using the excel writer, isn't any function available in System.IO.File/ Directory, etc. Javad
            FileHelper.MoveFile(tempPath, storePath);

            //Register the original data as a resource of the current dataset version
            ContentDescriptor originalDescriptor = new ContentDescriptor()
            {
                OrderNo = 1,
                Name = "original",
                MimeType = "application/xlsm",
                URI = dynamicStorePath,
                DatasetVersion = datasetVersion,
            };

            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(originalDescriptor.Name)) > 0)
            {   // remove the one contentdesciptor 
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == originalDescriptor.Name)
                    {
                        cd.URI = originalDescriptor.URI;
                    }
                }
            }
            else
            {
                // add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);
            }



            // create the generated file and determine its location
            string path = excelWriter.CreateFile(datasetId, datasetVersion.VersionNo, dataStructureId, title, ext);
            string dynamicPath = excelWriter.GetDynamicStorePath(datasetId, datasetVersion.VersionNo, title, ext);
            //Register the generated data file as a resource of the current dataset version
            ContentDescriptor generatedDescriptor = new ContentDescriptor()
            {
                OrderNo = 1,
                Name = "generated",
                MimeType = "application/xlsm",
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

            // note: the descriptors are not persisted yet, they will be persisted if the caller of this method persists the datasetVersion object.
            return path;
        }

        private string SaveFileInContentDiscriptor(DatasetVersion datasetVersion)
        {
            try
            {
                //XXX put function like GetStorePathOriginalFile or GetDynamicStorePathOriginalFile
                // the function is available in the abstract class datawriter
                ExcelWriter excelWriter = new ExcelWriter();
                // Move Original File to its permanent location
                String tempPath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                string originalFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
                string storePath = excelWriter.GetFullStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);
                string dynamicStorePath = excelWriter.GetDynamicStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);

                //Why using the excel writer, isn't any function available in System.IO.File/ Directory, etc. Javad
                FileHelper.MoveFile(tempPath, storePath);

                //Register the original data as a resource of the current dataset version
                ContentDescriptor originalDescriptor = new ContentDescriptor()
                {
                    OrderNo = 1,
                    Name = "unstructuredData",
                    MimeType = "application/xlsm",
                    URI = dynamicStorePath,
                    DatasetVersion = datasetVersion,
                };

                    // add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);


                return storePath;

            }
            catch(Exception e)
            {
                return "";
            }
        }

        private void AddDatatuplesToFile(long datasetId, long dataStructureId, string path)
        {
            List<long> tempDataTuplesIds = GetDataTuples(datasetId);

            ExcelWriter excelWriter = new ExcelWriter();
            excelWriter.AddDataTuplesToTemplate(tempDataTuplesIds, path, dataStructureId);
        }

        private List<long> GetDataTuples(long datasetId)
        {
            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
  
            return datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
        }


        #endregion
    }
}
