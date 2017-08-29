using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers.GFBIO;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {



            ////get all
            //var x = MappingUtils.GetAllMatchesInSystem(1, LinkElementType.MetadataNestedAttributeUsage);
            //// get all where value = david
            //x = MappingUtils.GetAllMatchesInSystem(1, LinkElementType.MetadataNestedAttributeUsage,"David");


            // get value from metadata over the system
            // partytpe person - attr firstname

            long partyCustomtAttr = 1;
            LinkElementType type = LinkElementType.PartyCustomType;

            long datasetId = 1;

            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);


            List<string> tmp = MappingUtils.GetValuesFromMetadata(partyCustomtAttr, type,
                datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

            tmp = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(Key.Title), LinkElementType.Key,
               datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));


            return View("Index");
        }

        public async Task<ActionResult> GetStatus(long id)
        {

            PublicationManager publicationManager = new PublicationManager();

            Broker broker =
                                       publicationManager.GetBroker()
                                           .Where(b => b.Name.ToLower().Equals("gfbio dev1"))
                                           .FirstOrDefault();


            if (broker != null)
            {
                //create a gfbio api webservice manager
                GFBIOWebserviceManager gfbioWebserviceManager = new GFBIOWebserviceManager(broker);

                string roStatusJsonResult = await gfbioWebserviceManager.GetStatusByResearchObjectById(id);

                //get status and store ro
                List<GFBIOResearchObjectStatus> gfbioRoStatusList =
                    new JavaScriptSerializer().Deserialize<List<GFBIOResearchObjectStatus>>(
                        roStatusJsonResult);
                GFBIOResearchObjectStatus gfbioRoStatus = gfbioRoStatusList.LastOrDefault();
                return Content(gfbioRoStatus.status);

            }

            return Content("no status");
        }

        private void CreatePartys()
        {
            #region CREATE PARTYS
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();

            PartyType partyType = partyTypeManager.Repo.Get().Where(p => p.Title.Equals("Person")).FirstOrDefault();

            if (partyType != null)
            {
                PartyStatusType partyStatusType = partyTypeManager.AddStatusType(partyType, "just created",
                    "this is for test data", 0);

                var p = partyManager.Create(partyType, "David Blaa", "desc", null, null, partyStatusType);
                // add value
                var pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("FirstName")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "David");

                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("LastName")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "Schöne");

                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("Email")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "ds@test.de");

                /***********************************/
                p = partyManager.Create(partyType, "Sven Thiel", "desc", null, null, partyStatusType);
                // add value
                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("FirstName")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "Sven");

                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("LastName")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "Thiel");

                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("Email")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "st@test.de");

                /***********************************/
                p = partyManager.Create(partyType, "Martin Hohmuth", "desc", null, null, partyStatusType);
                // add value
                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("FirstName")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "Martin");

                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("LastName")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "Hohmuth");

                pAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(a => a.Name.Equals("Email")).FirstOrDefault();
                partyManager.AddPartyCustomAttriuteValue(p, pAttr, "mh@test.de");

            }




            #endregion
        }
    }
}