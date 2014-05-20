using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.DDM.Helpers;
using Telerik.Web.Mvc;
using Vaiona.Util.Cfg;

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

        #region metadata
            public ActionResult ShowMetaData(int datasetID)
        {
            try
            {
                DatasetManager dm = new DatasetManager();
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                Session["Metadata"] = SearchUIHelper.ConvertXmlToHtml(dsv.Metadata.InnerXml.ToString(), "\\UI\\HtmlShowMetadata.xsl");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            return View();
        }

        #endregion

        #region primary data

            //Javad: this method is called on the hover of the search results.
            public ActionResult ShowPrimaryData(int datasetID)
            {
                DatasetManager dm = new DatasetManager();
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                DataStructureManager dsm = new DataStructureManager();
                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                List<DataTuple> dsVersionTuples = dm.GetDatasetVersionEffectiveTuples(dsv);

                string downloadUri = "";

                if (dsv.ContentDescriptors.Count > 0)
                {
                    if (dsv.ContentDescriptors.Count(p => p.Name.Equals("generated")) == 1)
                    {
                        downloadUri = dsv.ContentDescriptors.Where(p => p.Name.Equals("generated")).First().URI;
                    }
                }

                Tuple<DataTable, int, StructuredDataStructure, String> m = new Tuple<DataTable, int, StructuredDataStructure, String>(
                   SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dsVersionTuples),
                   datasetID,
                   sds,
                   downloadUri
                   );

                return View(m);
            }

            [GridAction]
            public ActionResult _CustomPrimaryDataBinding(GridCommand command, int datasetID)
            {
                DatasetManager dm = new DatasetManager();
                DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
                List<DataTuple> dsVersionTuples = dm.GetDatasetVersionEffectiveTuples(dsv);
                DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dsVersionTuples);

                foreach (DataRow v in table.Rows)
                {
                    Debug.WriteLine(v[0]);
                }

                return View(new GridModel(table));
            }

            public ActionResult DownloadPrimaryData(string path)
            {
                string[] temp = path.Split('\\');
                // define a correct name
                return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", temp.Last());
            }

            public ActionResult DownloadAsCsvData(long id)
            {



                return File(Path.Combine(AppConfiguration.DataPath), "text/plain", "test.txt");
            }
        #endregion


        #region datastructure

            [GridAction]
            public ActionResult _CustomDataStructureBinding(GridCommand command, int datasetID)
            {
                long id = (long)datasetID;
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

            public ActionResult ShowPreviewDataStructure(int datasetID)
            {
                DatasetManager dm = new DatasetManager();
                DatasetVersion ds = dm.GetDatasetLatestVersion((long)datasetID);
                DataStructureManager dsm = new DataStructureManager();
                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(ds.Dataset.DataStructure.Id);

                Tuple<StructuredDataStructure, int> m = new Tuple<StructuredDataStructure, int>(
                    sds,
                    datasetID
                   );

                return PartialView("_previewDatastructure", m);
            }

        #endregion
        


    }
}
