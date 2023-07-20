using BExIS.Dcm.UploadWizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitSummaryController : AsyncController
    {
        private TaskManager TaskManager;
        private FileStream Stream;

        private static IDictionary<Guid, int> tasks = new Dictionary<Guid, int>();

        private UploadHelper uploadWizardHelper = new UploadHelper();

        private Dictionary<string, object> _bus = new Dictionary<string, object>();

        // GET: /DCM/Summary/

        private User _user;

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

                _bus = TaskManager.Bus;
            }

            SummaryModel model = new SummaryModel();
            model.AsyncUpload = isASyncUpload();
            model.StepInfo = TaskManager.Current();

            model = updateModel(model);

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE) && TaskManager.Bus[TaskManager.DATASET_TITLE] != null)
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Summary(object[] data)
        {
            int numberOfRows = 0;

            TaskManager = (TaskManager)Session["TaskManager"];
            _bus = TaskManager.Bus;

            SummaryModel model = new SummaryModel();

            model.StepInfo = TaskManager.Current();

            long id = Convert.ToInt32(_bus[TaskManager.DATASET_ID]);

            DataASyncUploadHelper asyncUploadHelper = new DataASyncUploadHelper();
            asyncUploadHelper.Bus = _bus;
            asyncUploadHelper.User = GetUser();
            asyncUploadHelper.RunningASync = isASyncUpload();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE) && TaskManager.Bus[TaskManager.DATASET_TITLE] != null)
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.NUMBERSOFROWS))
            {
                numberOfRows = Convert.ToInt32(TaskManager.Bus[TaskManager.NUMBERSOFROWS]);
            }

            if (asyncUploadHelper.RunningASync) //async
            {
                Task.Run(() => asyncUploadHelper.FinishUpload());

                // send email after starting the upload
                var es = new EmailService();
                var user = GetUser();

                es.Send(MessageHelper.GetASyncStartUploadHeader(id, model.DatasetTitle),
                    MessageHelper.GetASyncStartUploadMessage(id, model.DatasetTitle, numberOfRows),
                    new List<string>() { user.Email }, null,
                    new List<string>() { GeneralSettings.SystemEmail }
                    );

                model.AsyncUpload = true;
                model.AsyncUploadMessage = "All upload information has been entered and the upload will start now. After completion an email will be sent.";
            }
            else
            {
                List<Error> errors = asyncUploadHelper.FinishUpload().Result;
                if (errors.Count == 0)
                {
                    return null;
                }
                else
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("", error.ToHtmlString());
                    }
                }
            }

            // set model for the page

            #region set summary

            model = updateModel(model);

            #endregion set summary

            return PartialView(model);
        }

        private User GetUser()
        {
            if (_user == null)
            {
                string username = string.Empty;

                try
                {
                    username = HttpContext.User.Identity.Name;
                }
                catch { return null; }

                using (UserManager userManager = new UserManager())
                {
                    _user = userManager.FindByNameAsync(username).Result;
                }
            }

            return _user;
        }

        private SummaryModel updateModel(SummaryModel model)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            //dataset
            if (_bus.ContainsKey(TaskManager.DATASET_ID)) model.DatasetId = Convert.ToInt32(_bus[TaskManager.DATASET_ID]);
            if (_bus.ContainsKey(TaskManager.DATASET_TITLE)) model.DatasetTitle = _bus[TaskManager.DATASET_TITLE].ToString();
            if (_bus.ContainsKey(TaskManager.DATASET_STATUS)) model.DatasetStatus = _bus[TaskManager.DATASET_STATUS].ToString();

            //datastructure
            if (_bus.ContainsKey(TaskManager.DATASTRUCTURE_ID)) model.DataStructureId = Convert.ToInt32(_bus[TaskManager.DATASTRUCTURE_ID]);
            if (_bus.ContainsKey(TaskManager.DATASTRUCTURE_TITLE)) model.DataStructureTitle = _bus[TaskManager.DATASTRUCTURE_TITLE].ToString();
            if (_bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE)) model.DataStructureType = _bus[TaskManager.DATASTRUCTURE_TYPE].ToString();

            //upload

            if (_bus.ContainsKey(TaskManager.UPLOAD_METHOD))
            {
                model.UploadMethod = _bus[TaskManager.UPLOAD_METHOD].ToString();
            }
            else model.UploadMethod = "Append";

            if (_bus.ContainsKey(TaskManager.NUMBERSOFROWS)) model.NumberOfRows = Convert.ToInt32(_bus[TaskManager.NUMBERSOFROWS]);
            if (_bus.ContainsKey(TaskManager.NUMBERSOFVARIABLES)) model.NumberOfVariables = Convert.ToInt32(_bus[TaskManager.NUMBERSOFVARIABLES]);
            if (_bus.ContainsKey(TaskManager.PRIMARY_KEYS))
            {
                List<long> keys = (List<long>)_bus[TaskManager.PRIMARY_KEYS];
                if (keys.Count() == 0) model.PrimaryKeys = "N/A";
                else model.PrimaryKeys = string.Join(",", keys);
            }
            else model.PrimaryKeys = "N/A";

            //file
            if (_bus.ContainsKey(TaskManager.FILENAME)) model.Filename = _bus[TaskManager.FILENAME].ToString();
            if (_bus.ContainsKey(TaskManager.FILEPATH)) model.Filepath = _bus[TaskManager.FILEPATH].ToString();
            if (_bus.ContainsKey(TaskManager.EXTENTION)) model.Extention = _bus[TaskManager.EXTENTION].ToString();

            return model;
        }

        private int getCellLimit()
        {
            int cellLimit = 0;

            //get cellLimt from settings
            var settings = ModuleManager.GetModuleSettings("DCM");

            if (Int32.TryParse(settings.GetValueByKey("celllimit").ToString(), out cellLimit))
            {
                if (cellLimit == 0) cellLimit = 100000;
            }

            return cellLimit;
        }

        private bool isASyncUpload()
        {
            int cells = 0;
            int cellLimit = getCellLimit();

            if (_bus.ContainsKey(TaskManager.NUMBERSOFROWS) && _bus.ContainsKey(TaskManager.NUMBERSOFVARIABLES))
            {
                cells = Convert.ToInt32(_bus[TaskManager.NUMBERSOFROWS]) * Convert.ToInt32(_bus[TaskManager.NUMBERSOFVARIABLES]);
            }

            if (cells > cellLimit) return true;

            return false;
        }
    }
}