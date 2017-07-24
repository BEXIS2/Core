using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
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

            ////get all
            //var x = MappingUtils.GetAllMatchesInSystem(1, LinkElementType.MetadataNestedAttributeUsage);
            //// get all where value = david
            //x = MappingUtils.GetAllMatchesInSystem(1, LinkElementType.MetadataNestedAttributeUsage,"David");

            return View("Index");
        }
    }
}