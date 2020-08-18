using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.Web.Shell.Helpers;
using BExIS.Web.Shell.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Schema;


namespace BExIS.Web.Shell.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]

    public class UiTestController : Controller
    {
        //[RecordCall]
        //[LogExceptions]
        //[Diagnose]
        //[MeasurePerformance]
        public ActionResult Index()
        {
            UiTestModel model = new UiTestModel();

            model = DynamicListToDataTable();

            Debug.WriteLine("call a webservice from gfbio");

            return View(model);
        }

        public ActionResult XSDtest()
        {
            string pathXsd = "G:/schema.xsd";
            string pathXml = "G:/test.xml";
            XmlSchema Schema;

            try
            {
                XmlDocument doc = new XmlDocument();

                if (FileHelper.FileExist("pathXml"))
                    FileHelper.Delete(pathXml);

                XmlNode root = doc.CreateElement("xyz");
                XmlAttribute rootAttr = doc.CreateAttribute("xmlns");
                rootAttr.Value = "abc";
                if (root.Attributes != null) root.Attributes.Append(rootAttr);

                XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(declaration);

                doc.AppendChild(root);
                doc.Save(pathXml);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            UiTestModel model = new UiTestModel();


            return View("Index", model);
        }

        /// <summary>
        /// Commented by Javad due to modularity issues.
        /// NOTE: module tests must go to the modules. At best to a Test project (or at least to a test controller)
        /// Thes functions should call the APIs of the DIM module and get json objects back.
        /// If Publication or any other entity is not part of the DLM, it is visible only to its own module.
        /// Other mosules who consume the API results of a module, should only expect .NET types, DLM types, json, xml, CSV, or Html.
        /// </summary>
        /*
        public ActionResult MappingTest()
        {

            //create Linkelements

            MappingManager mappingManager = new MappingManager();

            LinkElement source = mappingManager.CreateLinkElement(
                1, LinkElementType.MetadataStructure, LinkElementComplexity.Complex, "myMDS", ""
                , ""
                );

            LinkElement target = mappingManager.CreateLinkElement(
                1, LinkElementType.System, LinkElementComplexity.Complex, "System"
                , "", ""
                );

            Mapping m = mappingManager.CreateMapping(source, target, 1, null);

            mappingManager.DeleteMapping(m);
            mappingManager.DeleteLinkElement(source);
            mappingManager.DeleteLinkElement(target);

            UiTestModel model = new UiTestModel();
            model = DynamicListToDataTable();


            return View("Index", model);
        }
        public async Task<ActionResult> Call()
        {

            SubmissionManager submissionManager = new SubmissionManager();
            submissionManager.Load();

            DataRepository dataRepository =
                submissionManager.DataRepositories.Where(d => d.Name.ToLower().Equals("gfbio")).FirstOrDefault();

            Broker broker = new Broker();
            broker.Name = "gfbio";
            broker.UserName = "dblaa@bgc-jena.mpg.de";
            broker.Password = "1mpi4bg";
            broker.PrimaryDataFormat = "text/csv";
            broker.Server = "https://gfbio-dev1.inf-bb.uni-jena.de:8080";

            GFBIOWebserviceManager gfbioWebserviceManager = new GFBIOWebserviceManager(broker);

            //get user by email
            var user = await gfbioWebserviceManager.GetUserByEmail("dblaa@bgc-jena.mpg.de");
            GFBIOUser gfbiouser = new JavaScriptSerializer().Deserialize<GFBIOUser>(user);
            Debug.WriteLine(user);

            if (gfbiouser.userid == 0)
            {
                GFBIOException exception = new JavaScriptSerializer().Deserialize<GFBIOException>(user);
                Debug.WriteLine(exception);
            }

            //send false email by email
            user = await gfbioWebserviceManager.GetUserByEmail("xxx@cccc.x");
            gfbiouser = new JavaScriptSerializer().Deserialize<GFBIOUser>(user);
            if (gfbiouser.userid == 0)
            {
                GFBIOException exception = new JavaScriptSerializer().Deserialize<GFBIOException>(user);
                Debug.WriteLine(exception);
            }



            Debug.WriteLine(user);

            //get-project-by-id
            var project = await gfbioWebserviceManager.GetProjectById(201);
            GFBIOProject gfbioproject = new JavaScriptSerializer().Deserialize<GFBIOProject>(project);
            Debug.WriteLine(gfbioproject);


            //get - research - object - by - id
            var researchObject = await gfbioWebserviceManager.GetResearchObjectById(2001);
            Debug.WriteLine(researchObject);

            //create-project
            var newProject = await gfbioWebserviceManager.CreateProject(16297, "MyBexisProject", "MyBexisProject Description");
            gfbioproject = new JavaScriptSerializer().Deserialize<GFBIOProject>(newProject);
            Debug.WriteLine(newProject);


            string jsonResearchObject = "[" +
                                        "{ \"description\":\"information':'minimal information\",  \"name\":\"example 1\", \"researchobjecttype\":\"test\", \"submitterid\":1592616297},"
                                        + "{ \"description\":\"information':'it will create a error\", \"name\":\"example 2\", \"wrongparameter\":\"wrong parameter\"},"
                                        + "{ \"authornames\":[\"John Uploader\", \"Jane Researcher\"], \"brokerobjectid\":42, \"description\":\"information':'maximal\", \"extendeddata\":\"{'informations':'all information'}\", \"label\":\"ex 03\", \"licenselabel\":\"CC0\", \"metadatalabel\":\"Darwin Core\",\"name\":\"example 3\", \"parentresearchobjectid\":1, \"projectid\":2, \"researchobjecttype\":\"test\", \"submitterid\":15926}]";

            //create - research - object
            //var newResearchObject = gfbioWebserviceManager.CreateResearchObject(gfbiouser.userid, "bexis ro 2", "description", "ro type", "{lalala}",
            //    new string[] { "david", "marcel" });

            //Debug.WriteLine(newResearchObject);

            UiTestModel model = new UiTestModel();
            model = DynamicListToDataTable();

            return View("Index", model);
        }

        public async Task<string> CallGFBIOWebservice(string apiName, string entityName, string json, string jsonRequest)
        {

            string server =
                @"http://gfbio-dev1.inf-bb.uni-jena.de:8080/api/jsonws/GFBioProject-portlet.";

            //string jsonRequest = @"request-json";
            string parameters = json;

            string url = server + entityName + "/" + apiName + "/" + jsonRequest + "/";



            Debug.WriteLine(url);
            string returnValue = "";

            try
            {
                using (var client = new HttpClient())
                {
                    //generate url
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //test@testerer.de:WSTest
                    var byteArray = Encoding.ASCII.GetBytes("dblaa@bgc-jena.mpg.de:1mpi4bgc");

                    // "basic "+ Convert.ToBase64String(byteArray)
                    AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = ahv;

                    string requesturl = url + WebServiceHelper.Encode(parameters);
                    Debug.WriteLine(requesturl);
                    Debug.WriteLine(WebServiceHelper.Encode(parameters));
                    HttpResponseMessage response = await client.GetAsync(requesturl);
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

        public ActionResult publicationTest()
        {
            //get datasetversion 
            DatasetManager datasetManager = new DatasetManager();

            DatasetVersion dsv = datasetManager.GetDatasetLatestVersion(1);

            //publicationtest

            PublicationManager publicationManager = new PublicationManager();


            //create broker
            Broker broker = publicationManager.CreateBroker("Broker 1", "ServerUrl", "dave", "1234", "", "text/csv");

            //create Repository
            Repository Repo = publicationManager.CreateRepository("Repo 1", "Url", broker);

            //create Publication
            Publication p = publicationManager.CreatePublication(dsv, broker, "ROJ 1", 1, "filepath");

            //update Publication
            p.Repository = Repo;
            publicationManager.UpdatePublication(p);


            publicationManager.DeletePublication(p);
            publicationManager.DeleteBroker(broker);
            publicationManager.DeleteRepository(Repo);


            UiTestModel model = new UiTestModel();

            model = DynamicListToDataTable();
            return View("Index", model);
        }
        */

        //event handler to manage the errors
        private void verifyErrors(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                //Debug.Writeline(args.Message);
            }
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

        public ActionResult Test()
        {
            UiTestModel model = new UiTestModel();

            model = DynamicListToDataTable();

            //SubmissionManager pm = new SubmissionManager();
            //DatasetManager dm = new DatasetManager();

            //pm.Load();

            //DataRepository gfbio = pm.DataRepositories.Where(d => d.Name.ToLower().Equals("gfbio")).FirstOrDefault();

            //// get metadata
            //long testdatasetId = 1;
            //string formatname = "";
            //XmlDocument newXmlDoc;
            //DatasetVersion dsv = dm.GetDatasetLatestVersion(testdatasetId);
            //string title = XmlDatasetHelper.GetInformation(dsv, NameAttributeValues.title);


            //if (gfbio != null)
            //{
            //    formatname =
            //        XmlDatasetHelper.GetAllTransmissionInformation(1, TransmissionType.mappingFileExport, AttributeNames.name)
            //            .First();
            //    OutputMetadataManager.GetConvertedMetadata(testdatasetId, TransmissionType.mappingFileExport,
            //        formatname);


            //    // get primary data
            //    // check the data sturcture type ...
            //    if (dsv.Dataset.DataStructure.Self is StructuredDataStructure)
            //    {
            //        OutputDataManager odm = new OutputDataManager();
            //        // apply selection and projection

            //        odm.GenerateAsciiFile(testdatasetId, title, gfbio.PrimaryDataFormat);
            //    }

            //    string zipName = pm.GetZipFileName(testdatasetId, dsv.Id);
            //    string zipPath = pm.GetDirectoryPath(testdatasetId, gfbio);
            //    string zipFilePath = Path.Combine(zipPath, zipName);

            //    FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(zipFilePath));



            //    if (FileHelper.FileExist(zipFilePath))
            //    {
            //        if (FileHelper.WaitForFile(zipFilePath))
            //        {
            //            FileHelper.Delete(zipFilePath);
            //        }
            //    }

            //    ZipFile zip = new ZipFile();

            //    foreach (ContentDescriptor cd in dsv.ContentDescriptors)
            //    {
            //        string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
            //        string name = cd.URI.Split('\\').Last();

            //        if (FileHelper.FileExist(path))
            //        {
            //            zip.AddFile(path, "");
            //        }
            //    }
            //    zip.Save(zipFilePath);
            //}
            //else
            //{
            //    newXmlDoc = dsv.Metadata;
            //}




            return View("Index", model);
        }


    }
}
