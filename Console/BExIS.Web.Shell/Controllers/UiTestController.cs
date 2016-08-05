using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Web.Shell.Models;
using System.Data;
using System.ComponentModel;
using System.Configuration;
using System.Xml;
using BExIS.Dim.Entities;
using BExIS.Dim.Helpers;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Web.Shell.Helpers;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Web.Shell.Areas.DIM.Controllers;
using BExIS.Xml.Services;
using Vaiona.Logging.Aspects;
using BExIS.IO.Transform.Output;
using NHibernate.Criterion;
using System.IO;
using System.Xml.Schema;
using BExIS.IO;
using Vaiona.Utils.Cfg;
using Ionic.Zip;
using Microsoft.Xml.XMLGen;

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

            //string filepath = @"D:\BEXIS APP\GIT\Code\Workspace\Modules\DCM\Metadata\Eml\eml.xsd";
            //string destinationPath = @"D:\BEXIS APP\GIT\Code\Workspace\Modules\DCM\Metadata\Eml\test.xml";
            //XmlReaderSettings settings2 = new XmlReaderSettings();
            //settings2.DtdProcessing = DtdProcessing.Ignore;

            //XmlSchema Schema = new XmlSchema();
            //XmlReader xsd_file = XmlReader.Create(filepath, settings2);
            //Schema = XmlSchema.Read(xsd_file, verifyErrors);

            //XmlQualifiedName qname = new XmlQualifiedName("eml",
            //               "eml://ecoinformatics.org/eml-2.1.1");

            //XmlTextWriter textWriter = new XmlTextWriter(destinationPath, null);

            //XmlSampleGenerator xsg = new XmlSampleGenerator(Schema,qname);
            //xsg.WriteXml(textWriter);


            return View(model);
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

            PublishingManager pm = new PublishingManager();
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
