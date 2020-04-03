﻿using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Helpers;
using BExIS.Xml.Helpers;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            var es = new EmailService();
            var datasetId = 1;
            var title = "my cool dataset";
            es.Send(MessageHelper.GetCreateDatasetHeader(),
                MessageHelper.GetCreateDatasetMessage(datasetId, title, "David Schöne"),
                ConfigurationManager.AppSettings["SystemEmail"]
                );

            string name = "test";
            var x = RegExHelper.IsFilenameValid(name);

            name = "test | filename";

            x = RegExHelper.IsFilenameValid(name);

            name = RegExHelper.GetCleanedFilename(name);

            name = "des<>";
            x = RegExHelper.IsFilenameValid(name);
            name = RegExHelper.GetCleanedFilename(name);

            name = "123\"";
            x = RegExHelper.IsFilenameValid(name);
            name = RegExHelper.GetCleanedFilename(name);

            return View();
        }

        public ActionResult DTTest()
        {
            for (int i = 0; i < 100; i++)
            {
                DateTime dt = DateTime.FromOADate(i + 1);

                Debug.WriteLine(i + " -> " + DateTime.FromOADate(i).ToString() + " " + i + "+1-> " + DateTime.FromOADate(i + 1).ToString());
            }

            Debug.WriteLine("Leap Year");

            DateTime j1Start = new DateTime(1900, 1, 1, 0, 0, 0);
            DateTime j1End = new DateTime(1900, 12, 31, 23, 59, 59);

            DateTime j2Start = new DateTime(1905, 1, 1, 0, 0, 0);
            DateTime j2End = new DateTime(1905, 12, 31, 23, 59, 59);

            TimeSpan j1TimeSpan = j1End.Subtract(j1Start);
            TimeSpan j2TimeSpan = j2End.Subtract(j2Start);

            int diff = TimeSpan.Compare(j1TimeSpan, j2TimeSpan);

            return View("Index");
        }

        public ActionResult ReferenceTest()
        {
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();

            var x = entityReferenceManager.Create(1, 1, 2, 2, 2, 3, "test", "testType");
            entityReferenceManager.Delete(x);

            return View("index");
        }

        public ActionResult CreateTestDatasets(int n)
        {
            DatasetManager datasetManager = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            ResearchPlanManager researchPlanManager = new ResearchPlanManager();


            try
            {
                var structure = dataStructureManager.UnStructuredDataStructureRepo.Get(1);
                var metadatastructure = metadataStructureManager.Repo.Get(1);
                var researchplan = researchPlanManager.Repo.Get(1);
                var xmlDatasetHelper = new XmlDatasetHelper();

                var xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                var metadataXml = xmlMetadatWriter.CreateMetadataXml(metadatastructure.Id);

                for (int i = 0; i < n; i++)
                {
                   var dataset =  datasetManager.CreateEmptyDataset(structure, researchplan, metadatastructure);


                    if (datasetManager.IsDatasetCheckedOutFor(dataset.Id, "test") || datasetManager.CheckOutDataset(dataset.Id, "test"))
                    {
                        DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(dataset.Id);

                        datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                        datasetManager.CheckInDataset(dataset.Id, "", "test", ViewCreationBehavior.None);

                      
                        workingCopy.Metadata = Xml.Helpers.XmlWriter.ToXmlDocument(metadataXml);

                        string xpath = xmlDatasetHelper.GetInformationPath(metadatastructure.Id, NameAttributeValues.title);

                        workingCopy.Metadata.SelectSingleNode(xpath).InnerText = i.ToString();
                        workingCopy.Title = i.ToString();

                        datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                        datasetManager.CheckInDataset(dataset.Id, "", "test", ViewCreationBehavior.None);
                    }
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                datasetManager.Dispose();
                dataStructureManager.Dispose();
                metadataStructureManager.Dispose();
                researchPlanManager.Dispose();
            }


            return View("Index");
        }

        
    }

}