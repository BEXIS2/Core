using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Curation;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.Curation;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Util;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Routing;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class CurationEntryController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        private static CurationModel AnonymizedCurationResponseContent(DatasetVersion datasetVersion, IEnumerable<CurationEntry> curationEntries, User user)
        {
            bool userIsCurator = CurationEntry.GetCurationUserType(user).Equals(CurationUserType.Curator);

            // requesting user gets added to the set of seen users
            var userSet = new HashSet<long>() { user.Id };
            var userList = new List<User>();

            var responseEntries = new List<CurationEntryModel>();

            // extract all users out of the curationEntries
            // and create the list of curationEntryModels
            foreach (var curationEntry in curationEntries)
            {
                // add creator of each entry
                if (!userSet.Contains(curationEntry.Creator.Id))
                {
                    userSet.Add(curationEntry.Creator.Id);
                    userList.Add(curationEntry.Creator);
                }
                // The users (who are not curators) will not receive status entry items
                if (!userIsCurator && curationEntry.Type.Equals(CurationEntryType.StatusEntryItem))
                {
                    continue;
                }
                // add the curationEntryModel
                responseEntries.Add(new CurationEntryModel(curationEntry));
                // add the users from the notes to the seen users
                foreach (var note in curationEntry.Notes)
                {
                    if (!userSet.Contains(note.User.Id))
                    { 
                        userSet.Add(note.User.Id);
                        userList.Add(note.User);
                    }
                }
            }

            // remove requesting user because hes not in the user list
            // (unnecessary for now but should be done for other future checks)
            userSet.Remove(user.Id);

            // order the users by name for consistency
            // always put the requesting user on the first place
            var sortedUsers = userList.OrderBy(u => u.DisplayName).ToList();
            sortedUsers = sortedUsers.Prepend(user).ToList();

            // convert the user list to new userIds from 1 to n
            var userMap = new Dictionary<long, CurationUserModel>();
            for (var i = 0; i < sortedUsers.Count; i++) userMap.Add(sortedUsers[i].Id, new CurationUserModel(sortedUsers[i], i+1));

            // replace original userIds with the anonymized ones
            foreach (var curationEntry in responseEntries)
            {
                curationEntry.CreatorId = userMap[curationEntry.CreatorId].Id;
                foreach (var note in curationEntry.Notes)
                {
                    note.UserId = userMap[note.UserId].Id;
                }
            }

            return new CurationModel(datasetVersion.Id, datasetVersion.Title, datasetVersion.Timestamp, responseEntries, userMap.Values);
        }

        private static CurationEntryModel AnonymizeCurationEntryModel(CurationEntryModel knownCurationEntry, CurationEntry curationEntry, User user)
        {
            // create the CurationEntryModel from the CurationModel and replace the userId
            var newCurationEntry = new CurationEntryModel(curationEntry)
            {
                CreatorId = knownCurationEntry.CreatorId
            };

            // map each note to its user
            var noteUserMap = new Dictionary<long, long>();
            foreach (var knownNote in knownCurationEntry.Notes)
            {
                if (!noteUserMap.ContainsKey(knownNote.Id) && !knownNote.UserId.Equals(1))
                    noteUserMap.Add(knownNote.Id, knownNote.UserId);
            }

            // replace original userIds with the anonymized ones from the knownCurationEntry
            foreach (var newNote in newCurationEntry.Notes)
            {
                if (newNote.UserId == user.Id)
                    newNote.UserId = 1;
                else if (noteUserMap.TryGetValue(newNote.Id, out long value))
                    newNote.UserId = value;
                else
                    newNote.UserId = 0;
            }
            return newCurationEntry;
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [GetRoute("api/datasets/{datasetid}/curation")]
        public async Task<HttpResponseMessage> Get(long datasetid)
        {
            if (datasetid == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "id should be greater than 0");

            // get user or respond with error
            if (!ActionContext.ControllerContext.RouteData.Values.TryGetValue("user", out object userObject) || !(userObject is User user))
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");

            CurationModel responseContent;

            using (var curationManager = new CurationManager())
            using (var datasetManager = new DatasetManager())
            {
                var curationEntries = curationManager.CurationEntryRepository.Get().Where(c => c.Dataset.Id == datasetid).ToList();
                var datasetVersion = datasetManager.GetDatasetLatestVersion(datasetid);
                responseContent = AnonymizedCurationResponseContent(datasetVersion, curationEntries, user);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK, responseContent);
            return response;
                 
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [GetRoute("api/curationentries")]
        public async Task<HttpResponseMessage> Get()
        {
            // get user or respond with error
            if (!ActionContext.ControllerContext.RouteData.Values.TryGetValue("user", out object userObject) || !(userObject is User user))
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User not found");

            IEnumerable<CurationEntryModel> responseContent;

            // get CurationEntries
            List<CurationEntry> curationEntries = new List<CurationEntry>();
            using (var curationManager = new CurationManager())
            {
                curationEntries = curationManager.CurationEntryRepository.Get().ToList();
                responseContent = curationEntries.Select(e => new CurationEntryModel(e)).ToList();
            }

            var response = Request.CreateResponse(HttpStatusCode.OK, responseContent);
            return response;
        }

        // PUT: api/data/5
        [BExISApiAuthorize]
        [PutRoute("api/curationentries")]
        [JsonNetFilter]
        [HttpPut]
        public async Task<HttpResponseMessage> Put(CurationEntryModel curationEntryModel)
        {
            if (curationEntryModel == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "curationEntry is null");
            if (curationEntryModel.Id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed,"id should be greater than 0");

            // get user or respond with error
            if (!ActionContext.ControllerContext.RouteData.Values.TryGetValue("user", out object userObject) || !(userObject is User user))
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User not found");

            List<CurationNote> notes;
            if (curationEntryModel.Notes == null || !curationEntryModel.Notes.Any())
                notes = new List<CurationNote>();
            else
                notes = curationEntryModel.Notes.Select(n => new CurationNote() { Id = n.Id, Comment = n.Comment }).ToList();

            using (var curationManager = new CurationManager())
            {
                var c = curationManager.CurationEntries.FirstOrDefault(ce => ce.Id == curationEntryModel.Id);
                if (c == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "curationEntry not found");

                var newCurationEntry = curationManager.Update(
                        curationEntryModel.Id,
                        curationEntryModel.Topic,
                        curationEntryModel.Name,
                        curationEntryModel.Description,
                        curationEntryModel.Solution,
                        curationEntryModel.Position,
                        curationEntryModel.Source,
                        notes,
                        curationEntryModel.UserlsDone,
                        curationEntryModel.IsApproved,
                        user
                );

                var response = AnonymizeCurationEntryModel(curationEntryModel, newCurationEntry, user);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
           
        }

        [BExISApiAuthorize]
        [PostRoute("api/curationentries")]
        [JsonNetFilter]
        [HttpPost]
        public async Task<HttpResponseMessage> Post(CurationEntryModel curationEntryModel)
        {
            if (curationEntryModel == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "curationEntry must not be null");
            if (curationEntryModel.Id > 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "curationEntry id must be 0");
            if (curationEntryModel.DatasetId == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "datasetId should be greater than 0");

            // get user or respond with error
            if (!ActionContext.ControllerContext.RouteData.Values.TryGetValue("user", out object userObject) || !(userObject is User user))
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User not found");

            if (!CurationEntry.GetCurationUserType(user).Equals(CurationUserType.Curator))
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User not permitted to create curation entries");

            List<CurationNote> notes;
            if (curationEntryModel.Notes == null || !curationEntryModel.Notes.Any())
                notes = new List<CurationNote>();
            else
                notes = curationEntryModel.Notes.Select(n => new CurationNote(user, n.Comment)).ToList(); // all notes will be created for the current user

            using (var curationManager = new CurationManager())
            {
                var newCurationEntry = curationManager.Create(
                    curationEntryModel.Topic,
                    curationEntryModel.Type,
                    curationEntryModel.DatasetId,
                    curationEntryModel.Name,
                    curationEntryModel.Description,
                    curationEntryModel.Solution,
                    curationEntryModel.Position,
                    curationEntryModel.Source,
                    notes,
                    curationEntryModel.UserlsDone,
                    curationEntryModel.IsApproved,
                    user
                );

                var response = AnonymizeCurationEntryModel(curationEntryModel, newCurationEntry, user);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

    }
}