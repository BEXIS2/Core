using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Core.Serialization;
using System.Xml.Serialization;
using Vaiona.Persistence.Api;
using Vaiona.IoC;
using Vaiona.Util.Cfg;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Data;
using System.IO;
using Vaiona.Web.Models;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
#if DEBUG // it is designed just for testing and debugging purposes. For production versions, use release profile. Javad. 07.02.13
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            //List<string> a = new List<string>() { "A", "B", "C" };
            //List<string> b = new List<string>() { "A", "B", "D" };
            //var ab = a.Union(b);

            //dm.DatasetRepo.LoadIfNot(ds.Tuples);
            ////dm.DatasetRepo.Get(

            //DatasetExportTest2();
            //SimpleMatDematWithExport();
            //Add2UnitsAnd1Conversion();
            //Add2UnitsAnd1ConversionUsingAPI();
            //saveDatset();
            //string a = "this a book isolated";
            //bool b = a.ContainsExact("is");
            //testExtendedProperty();
            //getDataset();
            createDatasetVersion(4);
            //return RedirectToAction("About");
            return View();
        }

        private void createDataset()
        {
            DatasetManager dm = new DatasetManager();
            //dm.CreateEmptyDataset()
        }

        private void createDatasetVersion(Int64 datasetId)
        {
            DatasetManager dm = new DatasetManager();
            if (dm.IsDatasetCheckedOut(datasetId, "Javad") || dm.CheckOutDataset(datasetId, "Javad"))
            {
                DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                DataTuple changed = dm.GetDatasetVersionEffectiveTuples(workingCopy).First();
                changed.VariableValues.First().Value = (new Random()).Next().ToString();
                

                dm.EditDatasetVersion(workingCopy, null, new List<DataTuple>() { changed }, null);
                dm.CheckInDataset(datasetId, "for testing purposes", "Javad");
            }

        }

    //    private void createADataStructure()
    //    {
    //        DataStructureGenerator sdGen = new DataStructureGenerator();
    //        StructuredDataStructure sds = sdGen.GenerateStructuredDataStructure();
    //        DataContainerManager dcm = new DataContainerManager();
    //        List<Parameter> pps = (from vari in sds.VariableUsages
    //                                from pari in vari.DataAttribute.ParameterUsages
    //                                select pari.Parameter).ToList();
    //        if(sds.Indexer != null)
    //            pps.Add(sds.Indexer);
    //        pps = pps.Distinct().ToList();
    //        foreach (var item in pps)
    //{
    //     dcm.CreateParameter(item.Name, 
    //}
            

    //        DataStructureManager dsm = new DataStructureManager();
            
    //        dsm.CreateStructuredDataStructure(
    //    }
        private void testExtendedProperty()
        {
            DatasetManager dm = new DatasetManager();
            DataContainerManager dcManager = new DataContainerManager();

            Dataset ds = dm.GetDataset(24L);
            StructuredDataStructure sds = (ds.DataStructure.Self as StructuredDataStructure);
            ExtendedProperty exp = null;
            try { exp = dcManager.ExtendedPropertyRepo.Get(1); }
            catch {}
            //if(exp == null)
            //    exp = dcManager.CreateExtendedProperty("Source", "the data provider", sds.VariableUsages.First().DataAttribute, null); // issue with session management


            //ds.ExtendedPropertyValues = new List<ExtendedPropertyValue>()
            //{
            //    new ExtendedPropertyValue() {Dataset = ds, ExtendedPropertyId = exp.Id, Value="Jena Experiment"},
            //    new ExtendedPropertyValue() {Dataset = ds, ExtendedPropertyId = exp.Id, Value="MPI"},
            //};
            ds.Dematerialize();
            //dm.UpdateDataset(ds);
        }

        private void getDataset()
        {
            DatasetManager dm = new DatasetManager();
            Dataset ds = dm.GetDataset(3);
            if (ds != null)
            {
                //var a = ds.Tuples.First().VariableValues.First().Variable.Name;
                StructuredDataStructure sds = (StructuredDataStructure)(ds.DataStructure.Self);
                DatasetVersion dsv = dm.GetDatasetLatestVersion(ds.Id);
                var tuples = dm.GetDatasetVersionEffectiveTuples(dsv);
            }
        }

        private void saveDatset()
        {
            //try
            //{
            //    DatasetManager dm = new DatasetManager();
            //    DatasetGeneator dsGen = new DatasetGeneator();
            //    Dataset sent = dsGen.GenerateDatasetWithStructuredDatasetObjectTuples();
            //    DataStructureManager dsm = new DataStructureManager();
            //    sent.DataStructure = dsm.SdsRepo.Get(1);
            //    // push object to xml to make them ready for perso=istence
            //    sent.Dematerialize();

            //    dm.CreateDataset(sent);//sent.Title, sent.Description, sent.Metadata, sent.Tuples, sent.ExtendedPropertyValues, sent.ContentDescriptors, sent.DataStructure);
            //}
            //catch (Exception ex) { }
        }

        private void Add2UnitsAnd1ConversionUsingAPI()
        {
            UnitManager um = new UnitManager();
            Unit km = um.Create("Kilometer", "Km", "This is the Kilometer", "Length", MeasurementSystem.Metric);
            Unit m = um.Create("Meter", "M", "This is the Meter", "Length", MeasurementSystem.Metric);
            Unit cm = um.Create("Centimeter", "Cm", "This is the CentiMeter which is equal to 0.01 Meter", "Length", MeasurementSystem.Metric);
            ConversionMethod cm1 = um.CreateConversionMethod("s*100", "Converts meter to centi meter", m, cm);
            ConversionMethod cm2 = um.CreateConversionMethod("s*1000", "Converts kilometer to meter", km, m);
            ConversionMethod cm3 = um.CreateConversionMethod("s/1000", "Converts meter to kilometer", m, km);
            ConversionMethod cm4 = um.CreateConversionMethod("s/100", "Converts centimeter to meter", cm, m);

            km.Description += "Updated";
            cm1.Description += "Updated";
            km.ConversionsIamTheSource.Clear(); //??
            um.Update(km);
            um.UpdateConversionMethod(cm1);

            // Works fine: 24.07.12, Javad
            //DataTypeManager dtManager = new DataTypeManager();
            //DataType deci = dtManager.Create("Decimal", "A decimal data type", TypeCode.Int16);
            //um.AddAssociatedDataType(m, deci);
            //um.RemoveAssociatedDataType(m, deci);

            um.DeleteConversionMethod(cm1);
            um.DeleteConversionMethod(new List<ConversionMethod>() { cm2, cm3 });
                                    
            um.Delete(cm);
            um.Delete(new List<Unit>() { km, m });
        }

        [Authorize(Roles = "Admins")]
        public ActionResult About()
        {
            return View();
        }

        public ActionResult XmlShow()
        {
            XsltViewModel model = new XsltViewModel()
            {
                XmlPath = Server.MapPath("~/App_Data/data.xml"),
                XsltPath = Server.MapPath("~/App_Data/view.xsl")
            };
 
            return View(model);
        }

        public ActionResult XmlEdit()
        {
            string url = Url.Action("XmlEditResult", "Test", new { Area = string.Empty });
            XsltViewModel model = new XsltViewModel()
            {
                XmlPath = Server.MapPath("~/App_Data/data2.xml"),
                XsltPath = Server.MapPath("~/App_Data/edit.xsl"),
                Params = new Dictionary<string, object>()
                {
                    {"postBackUrl", url},
                }
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult XmlEditResult()
        {
            // here the user has access to the Request.Form object, which contains all the attributes defined in the xsl file and the associated values
            // I'd prefer to have an xml doc like the original one with new values. It is possible to find relevant fields in the xml based on the Form.Key and update its value
            // something like XmlDocument doc = createUpdatedDoc(originalfile, Request.Form);
            string url = Url.Action("Index", "Home", new { Area = string.Empty });
            XsltViewModel model = new XsltViewModel()
            {
                XmlPath = Server.MapPath("~/App_Data/data2.xml"),
                XsltPath = Server.MapPath("~/App_Data/edit.xsl"),
                Params = new Dictionary<string, object>()
                {
                    {"postBackUrl", url},
                }
            };
            //send the Request.Form object to the view too, so that it can overrides them to show the updated version
            return View("XmlEdit", model);
        }

        public void Add2UnitsAnd1Conversion()
        {
            //UnitDataGen uGen = new UnitDataGen();
            //ConversionMethod cmo = uGen.GenerateUnits1();

            //IPersistenceManager pManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;

            //using (IUnitOfWork unit = pManager.CreateUnitOfWork(false, true, true, unit_BeforeCommit, unit_AfterCommit))
            //{
            //    IRepository<ConversionMethod> repo1 = unit.GetRepository<ConversionMethod>();
            //    IRepository<Unit> repo2 = unit.GetRepository<Unit>();
            //    repo1.Put(cmo);
            //    //repo2.Put(cmo.Source);                
            //    //repo2.Put(cmo.Target);
            //    unit.Commit();

            //    //var received = repo1.Get(1);
            //    //var received2 = repo1.Get(p => p.Id == 1 && p.Source.Id == 1);
            //    //var u1 = repo2.Get(1);
            //    ////repo1.Delete(received);

            //    //u1.ConversionsIamTheSource.ForEach(p => repo1.Delete(p));
            //    //u1.ConversionsIamTheSource.Clear();
            //    //u1.ConversionsIamTheTarget.ForEach(p => repo1.Delete(p));
            //    //u1.ConversionsIamTheTarget.Clear();
            //    //repo2.Delete(u1);
            //    //unit.Commit();

            //    ////Assert.AreEqual(received.Id, cmo.Id);
            //    ////Assert.AreEqual(received.Source.Id, cmo.Source.Id);
            //    ////Assert.IsTrue(received.Source.ConversionsIamTheSource.Count() >= 1);
            //    ////Assert.AreEqual(received.Source.ConversionsIamTheSource.First().Target.Id, cmo.Target.Id);
            //}
            ////pManager.Shutdown(); // do not do it in production code
        }

        void unit_BeforeCommit(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void unit_AfterCommit(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void DatasetExportTest2()
        {
            //Dataset actual;
            //DatasetGeneator dsGen = new DatasetGeneator();
            //actual = dsGen.GenerateDatasetWithStructuredDatasetObjectTuples();
            //actual.Dematerialize();
            //actual.ExtendedPropertyValues.Clear();
            //actual.Materialize();
            //actual.Tuples.ForEach(p => p.Dematerialize());

            //IObjectTransfromer transformer = new XmlObjectTransformer(); // TODO: Initialize to an appropriate value

            //string path = Path.Combine(AppConfiguration.GetComponentWorkspacePath("Dlm"), "Transformation", "Xml");
            //XmlDocument doc = (XmlDocument)transformer.ExportTo<Dataset>(actual, "Dataset2");
            //doc.Save(Path.Combine(path, "datset2.xml"));

            //XmlDocument doc2 = new XmlDocument();
            //doc2.Load(Path.Combine(path, "datset2.xml"));
            //Dataset expected = (Dataset)transformer.ImportFrom<Dataset>(doc2);
            //expected.Tuples.ForEach(p => p.VariableValues.Clear());
            //expected.Tuples.ForEach(p => p.Amendments.Clear());
            //expected.Tuples.ForEach(p => p.Materialize());

        }               

        public void SimpleMatDematWithExport()
        {
            //Person actual = MatDematDataGenerator.GenerateAPerson();
            //actual.Dematerialize();
            //actual.Addresses = null;
            //actual.Materialize();


            //IObjectTransfromer transformer = new XmlObjectTransformer(); // TODO: Initialize to an appropriate value

            //string path = string.Format("{0}{1}", AppConfiguration.WorkspacePath, @"transformation\xml\");
            //XmlDocument doc = (XmlDocument)transformer.ExportTo<Dataset>(actual, "Person1"); // here S is inferred from the source parameter
            //doc.Save(path + @"p1.xml");

            //XmlDocument doc2 = new XmlDocument();
            //doc2.Load(path + @"p1.xml");
            //Person expected = (Person)transformer.ImportFrom<Person>(doc2);
            //expected.Materialize();
        }
    }
#endif
}
