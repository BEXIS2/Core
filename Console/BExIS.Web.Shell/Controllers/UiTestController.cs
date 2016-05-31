using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Web.Shell.Models;
using System.Data;
using System.ComponentModel;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Web.Shell.Helpers;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Logging.Aspects;

namespace BExIS.Web.Shell.Controllers
{
    public class UiTestController : Controller
    {
        //
        // GET: /UiTest/

        public ActionResult Index()
        {
            UiTestModel model = new UiTestModel();

            model = DynamicListToDataTable();

            return View(model);
        }

        public ActionResult sendForm(UiTestModel model)
        {

            return View("Index", model);
        }

        //[RecordCall]
        //[LogExceptions]
        //[Diagnose]
        //[MeasurePerformance]
        private UiTestModel DynamicListToDataTable()
        {

            DataStructureManager dm = new DataStructureManager();

            UiTestModel model = new UiTestModel();

            model.DataTable = BexisDataHelper.ToDataTable<DataStructure>(dm.AllTypesDataStructureRepo.Get(), new List<string> { "Id", "Name", "Description", "CreationInfo" });
            model.DataTable2 = model.DataTable;

            return model;
        }


    }
}
