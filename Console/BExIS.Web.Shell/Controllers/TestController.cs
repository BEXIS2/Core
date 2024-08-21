using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    public class TestController : BaseController
    {
        public ActionResult About()
        {
            return View();
        }

        public ActionResult TestRedirectToSvelte(long id)
        {
            return Redirect("/dcm/edit?id=" + id);
        }

        public ActionResult TestRedirectToSvelte2(long id)
        {
            return RedirectToAction("", "edit", new { area = "dcm", id });
        }

        public ActionResult AddRequest()
        {
            var requestManager = new RequestManager();

            try
            {
                requestManager.Create(32768, 1, 5);

                return View("Index");
            }
            finally
            {
                requestManager.Dispose();
            }
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

        //[RecordCall]
        //[LogExceptions]

        public ActionResult Index(Int64 id = 0)
        {
            ViewBag.Title = PresentationModel.GetGenericViewTitle("Test Page");
            /*in the Vaiona.Web.Mvc.Models namespace*/
            //String.Format("{0} {1} - {2}", AppConfiguration.ApplicationName, AppConfiguration.ApplicationVersion, "Test Page");

            //List<string> a = new List<string>() { "A", "B", "C" };
            //List<string> b = new List<string>() { "A", "B", "D" };
            //var ab = a.Union(b);

            //dm.DatasetRepo.LoadIfNot(ds.Tuples);
            ////dm.DatasetRepo.Get(
            //LoggerFactory.LogCustom("Hi, I am a custom message!");
            //LoggerFactory.LogData(id.ToString(), typeof(Dataset).Name, Vaiona.Entities.Logging.CrudState.Deleted);
            //LoggerFactory.LogDataRelation(id.ToString(), typeof(Dataset).Name, "20", typeof(DatasetVersion).Name, Vaiona.Entities.Logging.CrudState.Deleted);

            //DatasetExportTest2();
            //SimpleMatDematWithExport();
            //Add2UnitsAnd1Conversion();
            //Add2UnitsAnd1ConversionUsingAPI();
            //saveDatset();
            //string a = "this a book isolated";
            //bool b = a.ContainsExact("is");
            //testExtendedProperty();
            //getDataset();
            // Int64 dsId = 0;

            //    dsId = createDatasetVersion();
            //editDatasetVersion(dsId);
            //deleteTupleFromDatasetVersion(dsId);
            //deleteDataset(dsId);
            //purgeDataset(dsId);
            //purgeAll();
            //getAllDatasetVersions();

            //addConstraintsTo();
            //testMetadataStructure();
            //testSecuirty();
            //getEffectiveTuples(id);
            //getDataStructures();
            //return RedirectToAction("About");
            //createMetadataAttribute();
            ObtainingMethodManager om = new ObtainingMethodManager();

            ////Test Party Type Manager
            //Dictionary<string, string> dic = new Dictionary<string, string>();
            //dic.Add("Title_2", "a;liRel");
            //dic.Add("Title_1", "masoudRel");
            //dic.Add("Description_1", "");
            //dic.Add("Description_2", "ss");
            //dic.Add("StartDate_1", "11/6/2016");
            //dic.Add("StartDate_2", "");
            //dic.Add("EndDate_2", "");
            //dic.Add("EndDate_1", "11/6/2017");
            //dic.Add("Scope_1", "all");
            //var prs = ConvertDictionaryToPartyRelationships(dic);

            //Add Party Type

            // PartyUniqenessTest();

            //var partyType = addPartyType();
            ////removePartyType(partyType);
            //var partyStatusType = addPartyStatusType(partyType);
            //var cusAttr = addTestPartyCustomAttribute(partyType);
            ////removeTestPartyCustomAttribute(cusAttr);
            //// removePartyStatusType(partyStatusType);

            //////Create party
            Dlm.Services.Party.PartyManager partyManager = new Dlm.Services.Party.PartyManager();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                var partyRelationship = repoPR.Get().First();
                //var entity = repoPR.Reload(partyRelationship);
                repoPR.Delete(partyRelationship);
                uow.Commit();
            }
            ///// Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            //var parties = new List<Dlm.Entities.Party.Party>();
            //parties.Add(addTestParty(partyType, partyStatusType));
            //parties.Add(addTestParty(partyType, partyStatusType));
            //////update last test party
            //updateTestParty(parties.Last().Id);

            //////////deleteTestParty last test party
            ////////deleteTestParty(parties.First());
            //////////Add custom attribute value
            //var customAttrVal = addTestPartyCustomAttributeValue(parties.First(), cusAttr);
            //addTestPartyCustomAttributeValue(parties.Last(), cusAttr);
            ////removeTestPartyCustomAttributeValue(customAttrVal);
            ////// Create Party relationshiptype
            //////in the same time of creating partyrelationshiptype partyType pairs created and add to that
            //// var partyReType = addTestPartyRelationshipType(partyType, partyType);
            ////removeTestPartyRelationshipType(partyReType);
            ////Add relation between two parties
            ////The other relation are for testing minimum and maximum cardinaity
            //// var partyRel = addTestPartyRelationship(parties.First(), parties.Last(), partyReType);
            //// var partyRel2=addTestPartyRelationship(parties.First(), parties.Last(), partyReType);
            //// var partyRel3=addTestPartyRelationship(parties.First(), parties.Last(), partyReType);
            //// addTestPartyRelationship(parties.Last(), parties.First(), partyReType);
            ////    removePartyRelationship(partyRel);
            //// removePartyRelationship(partyRel2);
            ////removePartyRelationship(partyRel3);
            //// var partyPair = addTestPartyTypePair(partyType, addPartyType());
            //// removeTestPartyTypePair(partyPair);
            ////  var ps = addTestPartyStatus(parties.First());
            ////removeTestPartyStatus(ps);
            ////  deleteTestParty(parties.First());

            return View();
        }

        [DoesNotNeedDataAccess]
        public ActionResult Index2()
        {
            testNHibernateSession();
            //getDatasetVersionIdsThatHaveSOmeTuples(1);
            //addConstraintsTo(); // should face an exception since thre is no ambient session created, see DoesNotNeedDataAccess attribute

            return View("Index");
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

        public ActionResult TestUserToGroup()
        {
            var group = new Group() { Name = $"Group{new Random().Next(1, 10000)}", Description = "Super" };
            var user = new User()
            {
                Name = $"User{DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                UserName = "test"
            };

            var userManager = new UserManager();
            userManager.CreateAsync(user);

            var groupManager = new GroupManager();
            groupManager.CreateAsync(group);

            userManager.AddToRoleAsync(user, group.Name);

            return View("Index");
        }

        //public ActionResult XmlEdit()
        //{
        //    string url = Url.Action("XmlEditResult", "Test", new { Area = string.Empty });
        //    XsltViewModel model = new XsltViewModel()
        //    {
        //        XmlPath = Server.MapPath("~/App_Data/data2.xml"),
        //        XsltPath = Server.MapPath("~/App_Data/edit.xsl"),
        //        Params = new Dictionary<string, object>()
        //        {
        //            {"postBackUrl", url},
        //        }
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult XmlEditResult()
        //{
        //    // here the user has access to the Request.Form object, which contains all the attributes defined in the xsl FileStream and the associated values
        //    // I'd prefer to have an xml doc like the original one with new values. It is possible to find relevant fields in the xml based on the Form.Key and update its value
        //    // something like XmlDocument doc = createUpdatedDoc(originalfile, Request.Form);
        //    string url = Url.Action("Index", "Home", new { Area = string.Empty });
        //    XsltViewModel model = new XsltViewModel()
        //    {
        //        XmlPath = Server.MapPath("~/App_Data/data2.xml"),
        //        XsltPath = Server.MapPath("~/App_Data/edit.xsl"),
        //        Params = new Dictionary<string, object>()
        //        {
        //            {"postBackUrl", url},
        //        }
        //    };
        //    //send the Request.Form object to the view too, so that it can overrides them to show the updated version
        //    return View("XmlEdit", model);
        //}

        //public ActionResult XmlShow()
        //{
        //    XsltViewModel model = new XsltViewModel()
        //    {
        //        XmlPath = Server.MapPath("~/App_Data/data.xml"),
        //        XsltPath = Server.MapPath("~/App_Data/view.xsl")
        //    };

        //    return View(model);
        //}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // should be called automatically
        }

        private void addConstraintsTo()
        {
            //DataContainerManager dcManager = new DataContainerManager();
            //var attr = dcManager.DataAttributeRepo.Get(1);

            //var c1 = new RangeConstraint(ConstraintProviderSource.Internal, "", "en-US",
            //    "should be between 1 and 12 meter", true, null, null, null, 1.00, true, 12.00, true);
            //dcManager.AddConstraint(c1, attr);
            //var v1 = c1.IsSatisfied(14);

            //var c2 = new PatternConstraint(ConstraintProviderSource.Internal, "", "en-US",
            //    "a simple email validation constraint", false, null, null, null, @"^\S+@\S+$", false);
            //dcManager.AddConstraint(c2, attr);
            //var v2 = c2.IsSatisfied("javad.chamanara@uni-jena.com");

            //List<DomainItem> items = new List<DomainItem>()
            //{
            //    new DomainItem() {Key = "A", Value = "This is A"},
            //    new DomainItem() {Key = "B", Value = "This is B"},
            //    new DomainItem() {Key = "C", Value = "This is C"},
            //    new DomainItem() {Key = "D", Value = "This is D"},
            //};
            //var c3 = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US",
            //    "a simple domain validation constraint", false, null, null, null, items);
            //dcManager.AddConstraint(c3, attr);
            //var v3 = c3.IsSatisfied("A");
            //v3 = c3.IsSatisfied("E");
            //c3.Negated = true;
            //v3 = c3.IsSatisfied("A");

            //var c4 = new ComparisonConstraint(ConstraintProviderSource.Internal, "", "en-US",
            //    "a comparison validation constraint", false, null, null, null
            //    , ComparisonOperator.GreaterThanOrEqual, ComparisonTargetType.Value, "", ComparisonOffsetType.Ratio,
            //    1.25);
            //dcManager.AddConstraint(c4, attr);
            //var v4 = c4.IsSatisfied(14, 10);

            throw new NotImplementedException();
        }

        private List<PartyRelationship> ConvertDictionaryToPartyRelationships(
            Dictionary<string, string> partyRelationshipsDic)
        {
            var partyRelationships = new List<PartyRelationship>();
            foreach (var partyRelationshipDic in partyRelationshipsDic)
            {
                var key = partyRelationshipDic.Key.Split('_');
                if (key.Length != 2)
                    continue;
                int id = int.Parse(key[1]);
                string fieldName = key[0];
                var partyRelationship = partyRelationships.FirstOrDefault(item => item.TargetParty.Id == id);
                if (partyRelationship == null)
                {
                    partyRelationship = new PartyRelationship();
                    partyRelationship.TargetParty.Id = id;
                    partyRelationships.Add(partyRelationship);
                }
                if (!string.IsNullOrEmpty(partyRelationshipDic.Value))
                    switch (fieldName.ToLower())
                    {
                        case "title":
                            partyRelationship.Title = partyRelationshipDic.Value;
                            break;

                        case "description":
                            partyRelationship.Description = partyRelationshipDic.Value;
                            break;

                        case "startdate":
                            partyRelationship.StartDate = Convert.ToDateTime(partyRelationshipDic.Value);
                            break;

                        case "enddate":
                            partyRelationship.EndDate = Convert.ToDateTime(partyRelationshipDic.Value);
                            break;

                        case "scope":
                            partyRelationship.Scope = partyRelationshipDic.Value;
                            break;
                    }
            }
            return partyRelationships;
        }

        private void createMetadataAttribute()
        {
            //UnitManager um = new UnitManager();
            //Unit km = um.Create("Kilometer", "Km", "This is the Kilometer", "Length", MeasurementSystem.Metric);
            DataTypeManager dtManager = new DataTypeManager();
            DataType dt1 = dtManager.Repo.Get(p => p.Name.Equals("String")).FirstOrDefault();
            if (dt1 == null)
            {
                dt1 = dtManager.Create("String", "A test String", System.TypeCode.String);
            }

            MetadataAttributeManager maManager = new MetadataAttributeManager();

            // for unique name checks USE maManager.MetadataAttributeRepo that is searching both simple and compound attribute names
            var msa1 = maManager.MetadataAttributeRepo.Get(p => p.ShortName.Equals("Simple 1")).FirstOrDefault();
            if (msa1 == null)
            {
                msa1 = new MetadataSimpleAttribute()
                {
                    ShortName = "Simple 1",
                    DataType = dt1,
                };
                maManager.Create((MetadataSimpleAttribute)msa1);
            }
            var msa2 = maManager.MetadataAttributeRepo.Get(p => p.ShortName.Equals("Simple 2")).FirstOrDefault();
            if (msa2 == null)
            {
                msa2 = new MetadataSimpleAttribute()
                {
                    ShortName = "Simple 2",
                    DataType = dt1,
                };
                maManager.Create((MetadataSimpleAttribute)msa2);
            }

            MetadataCompoundAttribute mca1 =
                (MetadataCompoundAttribute)
                maManager.MetadataAttributeRepo.Get(p => p.ShortName.Equals("Compound 1")).FirstOrDefault();
            if (mca1 == null)
            {
                mca1 = new MetadataCompoundAttribute()
                {
                    ShortName = "Compound 1",
                    DataType = dt1,
                };
                MetadataNestedAttributeUsage u1 = new MetadataNestedAttributeUsage()
                {
                    Label = "First member",
                    Description = "I am a link between Compound 1 and Simple 1",
                    MinCardinality = 0,
                    MaxCardinality = 2,
                    Master = mca1,
                    Member = msa1,
                };
                mca1.MetadataNestedAttributeUsages.Add(u1);

                MetadataNestedAttributeUsage u2 = new MetadataNestedAttributeUsage()
                {
                    Label = "Second member",
                    Description = "I am a link between Compound 1 and Simple 2",
                    MinCardinality = 0,
                    MaxCardinality = 2,
                    Master = mca1,
                    Member = msa2,
                };
                mca1.MetadataNestedAttributeUsages.Add(u2);

                maManager.Create(mca1);
            }

            maManager.Delete(msa1);
        }

        private long createUnits(Unit incomingUnit, long dimensionId, MeasurementSystem measurementSystem,
            List<string> dataTypes, Dictionary<long, Dimension> dimLookup, Dictionary<string, Unit> unitLookup,
            Dictionary<string, DataType> datatypeLookup, Dictionary<string, MeasurementSystem> measSystemLookup)
        {
            UnitManager unitManager = null;
            //DataTypeManager dataTypeManger = new DataTypeManager();
            try
            {
                unitManager = new UnitManager();
                // create new unit
                Unit unit = new Unit
                {
                    Name = incomingUnit.Name + (new Random()).Next(),
                    Abbreviation = incomingUnit.Abbreviation,
                    Description = incomingUnit.Description
                };

                if (unit.Description.Length > 255)
                    unit.Description = unit.Description.Substring(0, 255);

                // attach dimension
                if (dimLookup.ContainsKey(dimensionId))
                {
                    unit.Dimension = dimLookup[dimensionId];
                }

                // find measurement system
                var ms = measurementSystem.ToString();
                if (measSystemLookup.ContainsKey(ms))
                {
                    unit.MeasurementSystem = measSystemLookup[ms];
                }

                // get existing unit or create new one
                if (unitLookup.ContainsKey(unit.Name.ToLower()))
                {
                    unit = unitLookup[unit.Name.ToLower()];
                }
                else
                {
                    unit = unitManager.Create(unit.Name, unit.Abbreviation, unit.Description, unit.Dimension,
                        unit.MeasurementSystem);
                }

                // attach datatypes to units
                foreach (string type in dataTypes)
                {
                    var t = type.ToLower();
                    if (datatypeLookup.ContainsKey(t))
                    {
                        var dt = datatypeLookup[t];
                        if (!(unit.AssociatedDataTypes.Contains(dt)))
                        {
                            unit.AssociatedDataTypes.Add(dt);
                        }
                    }
                }
                unitManager.Update(unit);

                // add unit-ID to the mappedUnits Table
                return unit.Id;
            }
            finally
            {
                unitManager.Dispose();
            }
        }

        private void deleteTupleFromDatasetVersion(long datasetId)
        {
            //DatasetManager dm = new DatasetManager();
            //Dataset ds = dm.DatasetRepo.Get(datasetId);
            ////if (!dm.IsDatasetCheckedIn(ds.Id))
            ////    return;
            //if (dm.IsDatasetCheckedOutFor(ds.Id, "Javad") || dm.CheckOutDataset(ds.Id, "Javad"))
            //{
            //    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

            //    DataTuple deleting = dm.GetDatasetVersionEffectiveTuples(workingCopy).First();

            //    dm.EditDatasetVersion(workingCopy, null, null, new List<DataTuple>() { deleting });
            //    dm.CheckInDataset(ds.Id, "editedVersion version", "Javad");
            //}
        }

        private void editDatasetVersion(Int64 datasetId)
        {
            //DatasetManager dm = new DatasetManager();
            //Dataset ds = dm.DatasetRepo.Get(datasetId);
            ////if (!dm.IsDatasetCheckedIn(ds.Id))
            ////    return;
            //if (dm.IsDatasetCheckedOutFor(ds.Id, "Javad") || dm.CheckOutDataset(ds.Id, "Javad"))
            //{
            //    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

            //    AbstractTuple changed = dm.GetDatasetVersionEffectiveTuples(workingCopy).First();
            //    changed.VariableValues.First().Value = (new Random()).Next().ToString();

            //    //DataTuple dt = dm.DataTupleRepo.Get(40);
            //    //DataTuple newDt = new DataTuple();
            //    //newDt.XmlAmendments = dt.XmlAmendments;
            //    //newDt.XmlVariableValues = dt.XmlVariableValues; // in normal cases, the VariableValues are set and then Dematerialize is called
            //    //newDt.Materialize();
            //    //newDt.OrderNo = 1;
            //    //newDt.TupleAction = TupleAction.Created;//not required
            //    //newDt.Timestamp = DateTime.UtcNow; //required? no, its set in the Edit
            //    //newDt.DatasetVersion = workingCopy;//required? no, its set in the Edit

            //    dm.EditDatasetVersion(workingCopy, null, new List<DataTuple>() { changed }, null);
            //    dm.CheckInDataset(ds.Id, "editedVersion version", "Javad");
            //}
        }

        private void getAllDatasetVersions()
        {
            DatasetManager dm = new DatasetManager();
            List<Int64> ids = dm.DatasetRepo.Query().Select(p => p.Id).ToList();
            var b = dm.GetDatasetLatestVersions();
            var a = dm.GetDatasetLatestVersions(ids);
            var c = dm.GetDatasetLatestMetadataVersions();
        }

        private void getDataset()
        {
            //DatasetManager dm = new DatasetManager();
            //Dataset ds = dm.GetDataset(3);
            //if (ds != null)
            //{
            //    //var a = ds.Tuples.First().VariableValues.First().Variable.Name;
            //    StructuredDataStructure sds = (StructuredDataStructure)(ds.DataStructure.Self);
            //    DatasetVersion dsv = dm.GetDatasetLatestVersion(ds.Id);
            //    var tuples = dm.GetDatasetVersionEffectiveTuples(dsv);
            //}
        }

        private void getDatasetVersionIdsThatHaveSOmeTuples(long datasetId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<DatasetVersion> repo = uow.GetReadOnlyRepository<DatasetVersion>();
                var versionIds1 =
                    repo.Query()
                        .Where(p => p.Dataset.Id == 1 && p.PriliminaryTuples.Count() >= 1)
                        .Select(p => p.Id)
                        .ToList();
            }
        }

        private void getDataStructures()
        {
            var dsm = new DataStructureManager();
            var all = dsm.AllTypesDataStructureRepo.Get();
            var uns = all.Where(p => p.Name.Contains("uns")).FirstOrDefault();
        }

        private void getEffectiveTuples(Int64 versionId)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion workingCopy = dm.GetDatasetVersion(versionId);
            var changed = dm.GetDatasetVersionEffectiveTuples(workingCopy);
        }

        private void purgeAll()
        {
            DatasetManager dm = new DatasetManager();
            foreach (var item in dm.DatasetRepo.Query().Select(p => p.Id).ToList())
            {
                dm.PurgeDataset(item);
            }
        }

        private void purgeDataset(long dsId)
        {
            DatasetManager dm = new DatasetManager();
            dm.PurgeDataset(dsId);
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

        //        dsm.CreateStructuredDataStructure(
        //    }
        private void testExtendedProperty()
        {
            DatasetManager dm = new DatasetManager();
            DataContainerManager dcManager = new DataContainerManager();

            Dataset ds = dm.GetDataset(24L);
            StructuredDataStructure sds = (ds.DataStructure.Self as StructuredDataStructure);
            ExtendedProperty exp = null;
            try
            {
                exp = dcManager.ExtendedPropertyRepo.Get(1);
            }
            catch
            {
            }
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

        private void testMetadataStructure()
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            MetadataStructure root = mdsManager.Repo.Get(p => p.Name == "Root").FirstOrDefault();
            if (root == null) root = mdsManager.Create("Root", "This is the root metadata structure", "", "", null);

            MetadataStructure s1 = mdsManager.Repo.Get(p => p.Name == "S1").FirstOrDefault();
            if (s1 == null) s1 = mdsManager.Create("S1", "This is S1 metadata structure", "", "", root);

            MetadataStructure s11 = mdsManager.Repo.Get(p => p.Name == "S1.1").FirstOrDefault();
            if (s11 == null) s11 = mdsManager.Create("S1.1", "This is S1.1 metadata structure", "", "", s1);

            MetadataStructure s2 = mdsManager.Repo.Get(p => p.Name == "S2").FirstOrDefault();
            if (s2 == null) s2 = mdsManager.Create("S2", "This is S2 metadata structure", "", "", root);

            MetadataPackage p1 = mdpManager.MetadataPackageRepo.Get(p => p.Name == "P1").FirstOrDefault();
            if (p1 == null) p1 = mdpManager.Create("P1", "Sample Package 1", true);

            MetadataPackage p2 = mdpManager.MetadataPackageRepo.Get(p => p.Name == "P2").FirstOrDefault();
            if (p2 == null) p2 = mdpManager.Create("P2", "Sample Package 2", true);

            MetadataPackage p3 = mdpManager.MetadataPackageRepo.Get(p => p.Name == "P3").FirstOrDefault();
            if (p3 == null) p3 = mdpManager.Create("P3", "Sample Package 3", true);

            MetadataPackage p4 = mdpManager.MetadataPackageRepo.Get(p => p.Name == "P4").FirstOrDefault();
            if (p4 == null) p4 = mdpManager.Create("P4", "Sample Package 4", true);

            if (s1.MetadataPackageUsages.Where(p => p.MetadataPackage == p1).Count() <= 0)
                mdsManager.AddMetadataPackageUsage(s1, p1, "P1 in S1", "", 0, 1);

            if (s1.MetadataPackageUsages.Where(p => p.MetadataPackage == p2).Count() <= 0)
                mdsManager.AddMetadataPackageUsage(s1, p2, "P2 in S1", "", 1, 1);

            if (s11.MetadataPackageUsages.Where(p => p.MetadataPackage == p3).Count() <= 0)
                mdsManager.AddMetadataPackageUsage(s11, p3, "P3 in S1.1", "", 0, 10);

            if (s11.MetadataPackageUsages.Where(p => p.MetadataPackage == p4).Count() <= 0)
                mdsManager.AddMetadataPackageUsage(s11, p4, "P4 in S1.1", "", 2, 5);

            var usages = mdsManager.GetEffectivePackages(3);
        }

        private void testNHibernateSession()
        {
            try
            {
                var dimLookup = new Dictionary<long, Dimension>();
                var unitLookup = new Dictionary<string, Unit>();
                var datatypeLookup = new Dictionary<string, DataType>();
                var measSystemLookup = new Dictionary<string, MeasurementSystem>();
                using (var uow = this.GetUnitOfWork())
                {
                    uow.GetReadOnlyRepository<Dimension>().Get().ToList().ForEach(p => dimLookup.Add(p.Id, p));
                    uow.GetReadOnlyRepository<Unit>().Get().ToList().ForEach(p => unitLookup.Add(p.Name.ToLower(), p));
                    uow.GetReadOnlyRepository<DataType>()
                        .Get()
                        .ToList()
                        .ForEach(p => datatypeLookup.Add(p.Name.ToLower(), p));
                    Enum.GetValues(typeof(MeasurementSystem))
                        .OfType<MeasurementSystem>()
                        .ToList()
                        .ForEach(p => measSystemLookup.Add(p.ToString(), p));

                    // the for loop is inside th UOW because traversing over each unit's AssociatedDataTypes needs the DB session to remain open.
                    // this may cause 'another operation is already in progress' problem
                    foreach (var unit in unitLookup)
                    {
                        Unit u = unit.Value;
                        var dataTypes = u.AssociatedDataTypes.Select(p => p.Name).ToList();
                        createUnits(u, u.Dimension.Id, u.MeasurementSystem, dataTypes, dimLookup, unitLookup,
                            datatypeLookup, measSystemLookup);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void testNHibernateSession2()

        {
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            var entityPermissions = entityPermissionManager.EntityPermissionRepository.Get();

            var x = entityPermissionManager.EntityPermissions.Where(m => m.Entity.Id == 1);
            for (int i = 0; i < 500; i++)
            {
                var ep = entityPermissionManager.CreateAsync<User>("javad", "Dataset", typeof(Dataset), 1,
                    Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                //entityPermissionManager.Create<User>("javad", "Dataset", typeof(Dataset), 2, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                //entityPermissionManager.Create<User>("javad", "Dataset", typeof(Dataset), 3, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                //var entityPermissions = entityPermissionManager.QueryEntityPermissions().Where(m => m.Entity.Id == ep.Id);
            }
            entityPermissionManager.CreateAsync<User>("javad", "Dataset", typeof(Dataset), 1,
                Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
            var y = x.ToList(); // it should work, while its repo is created from an isolated UoW.
        }

        #region PartyManager

        #region party_test

        /*
	- party type
		- create
			- with empty string
			- without status type
		- Delete
			- With some parties
			- have a statusType which its partystatusTpye has party: errors
			- have a statusType which its partystatusTpye doesn't have party: statusType and partystatusTpye should be deleted
			- have a CustomAttributes which has CustomAttributeValues: errors
			- have a CustomAttributes which doesn't have CustomAttributeValues	- test party
	- party:
		add :
			- add null party
			- add party without date => max date and min date
			- add party without
        delete:
			- delete party immidiately
			- delete party after have
				- custom attribute value
				- party relation ship
				- party status
			- update
	- PartyCustomAttribute
		- add
			- with null party type : error
			- without name
			- doublicate name with one party type : error
			- display order test
		- Delete
			-
	- partyCustomAttributeValue
		- check uniqeness
        */

        protected void PartyUniqenessTest()
        {
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "test", Description = "" });
            var pt = ptm.Create("test", "", "", partyStatusTypes);
            Console.WriteLine("Party type PartyUniqenessTest_Type created.");

            var pca = ptm.CreatePartyCustomAttribute(pt, "", "email", "", "", "", false, true);

            Console.WriteLine("one custom attribute email created.");
            var party = pm.Create(pt, "", "", null, null, partyStatusTypes.First());
            var party2 = pm.Create(pt, "", "", null, null, partyStatusTypes.First());
            Console.WriteLine("two party with the same party type created.");
            pm.AddPartyCustomAttributeValue(party, pca, "a@2.com");
            try
            {
                pm.AddPartyCustomAttributeValue(party2, pca, "a@2.com");
                System.Diagnostics.Debug.WriteLine("Failed single uniqeness test. add the same pcv .");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Success single uniqeness  test. add the same pcv .");
            }
            var pca2 = ptm.CreatePartyCustomAttribute(pt, "", "name", "", "", "", false, true);
            try
            {
                pm.AddPartyCustomAttributeValue(party2, pca, "a@2.com");
                System.Diagnostics.Debug.WriteLine("Success multiple uniqeness test. add the same pcv .");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("failed multiple uniqeness  test. add the same pcv .");
            }
            var pcav1 = pm.AddPartyCustomAttributeValue(party2, pca2, "mas");
            var party3 = pm.Create(pt, "", "", null, null, partyStatusTypes.First());
            try
            {
                pm.AddPartyCustomAttributeValue(party3, pca, "a@2.com");
                pm.AddPartyCustomAttributeValue(party3, pca2, "mas");
                System.Diagnostics.Debug.WriteLine("Success multiple uniqeness test for new party. add the same pcv .");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("failed multiple uniqeness  test for new party. add the same pcv .");
            }

            var party4 = pm.Create(pt, "", "", null, null, partyStatusTypes.First());
            var pcavs = new Dictionary<PartyCustomAttribute, string>();
            pcavs.Add(pca, "a@2.com");
            pcavs.Add(pca2, "mas");
            try
            {
                pm.AddPartyCustomAttributeValues(party4, pcavs);
                System.Diagnostics.Debug.WriteLine(
                    "failed multiple uniqeness test doesnt have any error . add the same pcv .");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Success multiple uniqeness  test has errors. add the same pcv .");
            }
            pcavs = new Dictionary<PartyCustomAttribute, string>();
            pcavs.Add(pca, "a@2.com");
            pcavs.Add(pca2, "mas1");
            var pcavs_list = new List<PartyCustomAttributeValue>();
            try
            {
                pcavs_list = pm.AddPartyCustomAttributeValues(party4, pcavs).ToList();
                System.Diagnostics.Debug.WriteLine(
                    "success multiple uniqeness add test doesnt have any error . add the same pcv .");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed multiple uniqeness  add test has errors. add the same pcv .");
            }
            try
            {
                var thisCustomAtrval = pcavs_list.Last();
                thisCustomAtrval.Value = "mas";
                pm.UpdatePartyCustomAttributeValues(pcavs_list);
                System.Diagnostics.Debug.WriteLine(
                    "failed multiple uniqeness update test doesnt have any error . add the same pcv .");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "success multiple uniqeness  update test has errors. add the same pcv . error");
            }
            //Update single with the same without errror
            //update multiple with different value without error

            try
            {
                pcavs[pca2] = "mas";
                pm.AddPartyCustomAttributeValues(party4, pcavs);
                System.Diagnostics.Debug.WriteLine(
                    "failed multiple uniqeness update test doesnt have any error . add the same pcv .");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(
                    "success multiple uniqeness  update test has errors. add the same pcv . error");
            }
            try
            {
                party3.StartDate = DateTime.Now;
                pm.Update(party3);
                var partyUpdated = pm.PartyRepository.Get(party3.Id);
                if (partyUpdated.StartDate == party3.StartDate)
                    System.Diagnostics.Debug.WriteLine("success party update .");
                else
                    System.Diagnostics.Debug.WriteLine("failed party update .last start date after update");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("failed party update. Error!!");
            }
            try
            {
                pca.DataType = "Integer";
                ptm.UpdatePartyCustomAttribute(pca);
                var pcaUpdated = ptm.PartyCustomAttributeRepository.Get(pca.Id);
                if (pcaUpdated.DataType == "Integer")
                    System.Diagnostics.Debug.WriteLine("success party custom attribute update .");
                else
                    System.Diagnostics.Debug.WriteLine("failed party custom attribute update .data type is not Integer");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("failed party custom attribute update. Error!!");
            }
        }

        /*
        */

        #endregion party_test

        #region party

        private Dlm.Entities.Party.Party addTestParty(PartyType partyType, PartyStatusType st)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();

            var party = pm.Create(partyType, "", "party created for test", null, null, st);
            return party;
        }

        private void deleteTestParty(Dlm.Entities.Party.Party party)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            pm.Delete(party);
        }

        private void deleteTestParty(List<Dlm.Entities.Party.Party> parties)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            pm.Delete(parties);
        }

        private void updateTestParty(long id)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            var party = pm.PartyRepository.Get(id);
            party.Description = "updated..";
            pm.Update(party);
        }

        #endregion party

        #region partyStatus

        private Dlm.Entities.Party.PartyStatus addTestPartyStatus(Dlm.Entities.Party.Party party)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            var partyType = ptm.Create("partyTypeTest2", "just for test2", "", null);
            var st = ptm.AddStatusType(partyType, "second try", "this is for test data", 0);
            return pm.AddPartyStatus(party, st, "test");
        }

        #endregion partyStatus

        #region PartyRelationship

        //private Dlm.Entities.Party.PartyRelationship addTestPartyRelationship(Dlm.Entities.Party.Party SourceParty, Dlm.Entities.Party.Party TargetParty, PartyRelationshipType prt)
        //{
        //    Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
        //    return pm.AddPartyRelationship(SourceParty, TargetParty, prt, "test Rel", "test relationship", DateTime.Now);
        //}

        private bool removePartyRelationship(Dlm.Entities.Party.PartyRelationship partyRelationship)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            return pm.RemovePartyRelationship(partyRelationship);
        }

        #endregion PartyRelationship

        #region PartyCustomAttributeValue

        private Dlm.Entities.Party.PartyCustomAttributeValue addTestPartyCustomAttributeValue(
            Dlm.Entities.Party.Party party, Dlm.Entities.Party.PartyCustomAttribute partyCustomAttr)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            pm.AddPartyCustomAttributeValue(party, partyCustomAttr, "TestName");
            Dictionary<PartyCustomAttribute, string> customAtts = new Dictionary<PartyCustomAttribute, string>();
            customAtts.Add(partyCustomAttr, "Dic");
            pm.AddPartyCustomAttributeValues(party, customAtts);
            return pm.AddPartyCustomAttributeValue(party, partyCustomAttr, "TestName updated");
        }

        private bool removeTestPartyCustomAttributeValue(Dlm.Entities.Party.PartyCustomAttributeValue partyCustomAttrVal)
        {
            Dlm.Services.Party.PartyManager pm = new Dlm.Services.Party.PartyManager();
            return pm.RemovePartyCustomAttributeValue(partyCustomAttrVal);
        }

        #endregion PartyCustomAttributeValue

        #endregion PartyManager

        #region Party Type Manager

        #region partyType

        private Dlm.Entities.Party.PartyType addPartyType()
        {
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            return ptm.PartyTypeRepository.Get(2);
            // return ptm.Create("partyTypeTest", "just for test", null);
        }

        private void removePartyType(PartyType partyType)
        {
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            ptm.Delete(partyType);
        }

        #endregion partyType

        #region addStatusType

        private Dlm.Entities.Party.PartyStatusType addPartyStatusType(PartyType partyType)
        {
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            return ptm.AddStatusType(partyType, "just created", "this is for test data", 0);
        }

        private void removePartyStatusType(PartyStatusType partyStatusType)
        {
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            ptm.RemoveStatusType(partyStatusType);
        }

        #endregion addStatusType

        #region PartyCustomAttribute

        private Dlm.Entities.Party.PartyCustomAttribute addTestPartyCustomAttribute(
            Dlm.Entities.Party.PartyType partyType)
        {
            Dlm.Services.Party.PartyTypeManager ptm = new Dlm.Services.Party.PartyTypeManager();
            return ptm.CreatePartyCustomAttribute(partyType, "string", "Namen", "Name for test", "", "", true, true);
        }

        private bool removeTestPartyCustomAttribute(Dlm.Entities.Party.PartyCustomAttribute partyCustomAttr)
        {
            Dlm.Services.Party.PartyTypeManager pm = new Dlm.Services.Party.PartyTypeManager();
            return pm.DeletePartyCustomAttribute(partyCustomAttr);
        }

        #endregion PartyCustomAttribute

        #endregion Party Type Manager

        #region Party RelationshipType Manager

        #region PartyRelationshipType

        private PartyRelationshipType addTestPartyRelationshipType(PartyType alowedSource, PartyType alowedTarget)
        {
            Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            return pmr.Create("test", "", "", false, 3, 2, false, alowedSource, alowedTarget, "", "", "", "", 0);
        }

        private bool removeTestPartyRelationshipType(PartyRelationshipType partyRelationshipType)
        {
            Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            return pmr.Delete(partyRelationshipType);
        }

        private bool removeTestPartyRelationshipType(List<PartyRelationshipType> partyRelationshipType)
        {
            Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            return pmr.Delete(partyRelationshipType);
        }

        #endregion PartyRelationshipType

        #region PartyTypePair

        private Dlm.Entities.Party.PartyTypePair addTestPartyTypePair(PartyType alowedSource, PartyType alowedTarget)
        {
            Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            return pmr.AddPartyTypePair("TitleTest", alowedSource, alowedTarget, "rel Type test", false, null, "", "", 0);
        }

        private bool removeTestPartyTypePair(PartyTypePair partyTypePair)
        {
            Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            return pmr.RemovePartyTypePair(partyTypePair);
        }

        private bool removeTestPartyTypePair(List<PartyTypePair> partyTypePairs)
        {
            Dlm.Services.Party.PartyRelationshipTypeManager pmr = new Dlm.Services.Party.PartyRelationshipTypeManager();
            return pmr.RemovePartyTypePair(partyTypePairs);
        }

        #endregion PartyTypePair

        #endregion Party RelationshipType Manager

        private void testSecuirty()
        {
            //PermissionManager pManager = new PermissionManager();
            //var user = pManager.UsersRepo.Get(3); // Roman
            //var the = pManager.UsersRepo.Refresh(user.Id);
            //var the2 = pManager.UsersRepo.Reload(user);
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

        private void unit_AfterCommit(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        //        DataStructureManager dsm = new DataStructureManager();
        private void unit_BeforeCommit(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public ActionResult CheckActivator()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            var l = hookManager.GetHooksFor("dataset", "details", HookMode.edit);
            var metadataHook = l.FirstOrDefault();
            metadataHook.Check(1, "david");

            return View();
        }

        public JsonResult ErrorTest()
        {
            throw new NotImplementedException("this is a error test");
        }
    }
}