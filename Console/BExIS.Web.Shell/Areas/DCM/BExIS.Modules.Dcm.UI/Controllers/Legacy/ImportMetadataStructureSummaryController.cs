using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Modules.Dcm.UI.Models.ImportMetadata;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ImportMetadataStructureSummaryController : Controller
    {
        private ImportMetadataStructureTaskManager TaskManager;

        //
        // GET: /DCM/ImportMetadataStructureSummary/
        [HttpGet]
        public ActionResult Summary(int index)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            SummaryModel model = new SummaryModel(TaskManager.Current());
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ROOT_NODE))
                model.RootName = TaskManager.Bus[ImportMetadataStructureTaskManager.ROOT_NODE].ToString();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
                model.SchemaName = TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME].ToString();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Summary(object[] data)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            return PartialView();
        }
    }
}