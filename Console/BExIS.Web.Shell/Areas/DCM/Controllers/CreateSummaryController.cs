using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.Web.Shell.Areas.DCM.Models.Create;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateSummaryController : Controller
    {
        CreateDatasetTaskmanager TaskManager;

        //
        // GET: /DCM/CreateSummary/
        [HttpGet]
        public ActionResult Summary(int index)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            SummaryModel model = new SummaryModel();
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.UpdateStepStatus(index);
                TaskManager.SetCurrent(index);
                Session["CreateDatasetTaskmanager"] = TaskManager;


                if(TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    Dictionary<string, MetadataPackageModel> list = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    model = SummaryModel.Convert(list,TaskManager.Current());
                    return PartialView(model);
    
                }
                

            }

           model.StepInfo = TaskManager.Current();
           return PartialView(SummaryModel.Convert((Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST], TaskManager.Current()));
        }

        [HttpPost]
        public ActionResult Summary()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {

                Session["CreateDatasetTaskmanager"] = TaskManager;
            }

            return PartialView(SummaryModel.Convert((Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST], TaskManager.Current()));
        }

    }
}
