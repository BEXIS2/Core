using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BExIS.Web.Shell.Models;
using System.Data;
using System.Diagnostics;
using System.Xml;
using BExIS.Dim.Entities;
using BExIS.Dim.Helpers;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Web.Shell.Helpers;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Xml.Services;
using BExIS.IO.Transform.Output;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using BExIS.IO;
using Vaiona.Utils.Cfg;
using Ionic.Zip;
using Vaiona.Logging.Aspects;

namespace BExIS.Web.Shell.Controllers
{
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

        public async Task<ActionResult> Call()
        {
            SubmissionManager submissionManager = new SubmissionManager();
            submissionManager.Load();

            DataRepository dataRepository =
                submissionManager.DataRepositories.Where(d => d.Name.ToLower().Equals("gfbio")).FirstOrDefault();

            GFBIOWebserviceManager gfbioWebserviceManager = new GFBIOWebserviceManager(dataRepository);
            


            Debug.WriteLine("call a webservice from gfbio");
            Debug.WriteLine("GET");
            Debug.WriteLine("-----------------------------------------------------------");

            //research object id 201
            //wieso als json ein array?
            var p = await CallGFBIOWebservice(201, "get-research-object-by-id","researchobject", "[{\"researchobjectid\":201}]");
            Debug.WriteLine(p);

            p = await gfbioWebserviceManager.GetResearchObjectById(401);
            Debug.WriteLine(p);

            Debug.WriteLine("-----------------------------------------------------------");
            //project id 201
            p = await CallGFBIOWebservice(201, "get-project-by-id","project", "{\"projectid\":401}");
            Debug.WriteLine(p);

            //if (string.IsNullOrEmpty(p))
            //{
            //    Debug.WriteLine("Create");
            //    Debug.WriteLine("-----------------------------------------------------------");
            //    p =
            //        await
            //            CallGFBIOWebservice(201, "create-project", "project",
            //                "{\"userid\":1,\"name\":\"bexis 2 test\",\"description\":\"test\"}");

            //    Debug.WriteLine(p);
            //}

            //wieso als return json ein array?
            // create research object 
            // name, label, extendeddata (json), researchobjecttype (e.eg metadata schema)
            //p = await CallGFBIOWebservice(201, "create-research-object", "researchobject", "[{\"name\":\"bexis 2 ro create\",\"label\":\"bexis 2 ro create\",\"extendeddata\":{},\"researchobjecttype\":\"metadata schema\"}]");
            Debug.WriteLine(p);


            UiTestModel model = new UiTestModel();
            model = DynamicListToDataTable();

            return View("Index",model);
        }

        public async Task<string> CallGFBIOWebservice(long id, string apiName,string entityName, string json)
        {

            string server =
                @"http://gfbio-pub2.inf-bb.uni-jena.de:8080/api/jsonws/GFBioProject-portlet.";

            string jsonRequest = @"request-json";


            //string parameters = @"[{" + parametername + ":" + id + "}]";
            //"[{\""+parametername+"\":"+id+"}]";
            string parameters = json;

            string url = server+ entityName+"/"+ apiName + "/" + jsonRequest + "/";

            

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
                    var byteArray = Encoding.ASCII.GetBytes("broker.agent@gfbio.org:AgentPhase2");

                    // "basic "+ Convert.ToBase64String(byteArray)
                    AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = ahv;

                    //http://gfbio-pub2.inf-bb.uni-jena.de:8080/api/jsonws/GFBioProject-portlet.researchobject/get-research-object-by-id/request-json/%5B%7B%22researchobjectid%22%3A3%20%7D%5D
                    string requesturl = url + Server.UrlEncode(parameters);
                    Debug.WriteLine(requesturl);
                    Debug.WriteLine(Server.UrlEncode(parameters));
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

            SubmissionManager pm = new SubmissionManager();
            DatasetManager dm = new DatasetManager();

            pm.Load();

            DataRepository gfbio = pm.DataRepositories.Where(d => d.Name.ToLower().Equals("gfbio")).FirstOrDefault();

            // get metadata
            long testdatasetId = 1;
            string formatname = "";
            XmlDocument newXmlDoc;
            DatasetVersion dsv = dm.GetDatasetLatestVersion(testdatasetId);
            string title = XmlDatasetHelper.GetInformation(dsv, NameAttributeValues.title);


            if (gfbio != null)
            {
                formatname =
                    XmlDatasetHelper.GetAllTransmissionInformation(1, TransmissionType.mappingFileExport, AttributeNames.name)
                        .First();
                OutputMetadataManager.GetConvertedMetadata(testdatasetId, TransmissionType.mappingFileExport,
                    formatname);


                // get primary data
                // check the data sturcture type ...
                if (dsv.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    OutputDataManager odm = new OutputDataManager();
                    // apply selection and projection
                    
                    odm.GenerateAsciiFile(testdatasetId, title, gfbio.PrimaryDataFormat);
                }

                string zipName = pm.GetZipFileName(testdatasetId, dsv.Id);
                string zipPath = pm.GetDirectoryPath(testdatasetId, gfbio);
                string zipFilePath = Path.Combine(zipPath, zipName);

                FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(zipFilePath));

                

                if (FileHelper.FileExist(zipFilePath))
                {
                    if (FileHelper.WaitForFile(zipFilePath))
                    {
                        FileHelper.Delete(zipFilePath);
                    }
                }

                ZipFile zip = new ZipFile();

                foreach (ContentDescriptor cd in dsv.ContentDescriptors)
                {
                    string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                    string name = cd.URI.Split('\\').Last();

                    if (FileHelper.FileExist(path))
                    {
                        zip.AddFile(path, "");
                    }
                }
                zip.Save(zipFilePath);
            }
            else
            {
                newXmlDoc = dsv.Metadata;
            }


            

            return View("Index", model);
        }

    }
}
