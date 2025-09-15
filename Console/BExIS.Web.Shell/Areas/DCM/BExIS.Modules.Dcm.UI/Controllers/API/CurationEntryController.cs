using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Web.Http;
using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Curation;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.Curation;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config.Configurations;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Microsoft.AspNet.Identity;
using NHibernate.Linq;
using NHibernate.Util;
using Vaiona.Web.Mvc.Modularity;

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
                // The users (who are not curators) will not receive the description of the status entry item
                if (!userIsCurator && curationEntry.Type.Equals(CurationEntryType.StatusEntryItem))
                {
                    responseEntries.Add(new CurationEntryModel(curationEntry)
                    {
                        Description = string.Empty,
                        Notes = new List<CurationNoteModel>()
                    });
                } else {
                    // add the curationEntryModel
                    responseEntries.Add(new CurationEntryModel(curationEntry));
                }
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

            var curationLabels = ModuleManager.GetModuleSettings("DDM").GetValueByKey<CurationConfiguration>("curation")?.CurationLabels;

            curationLabels = curationLabels == null ? new List<CurationLabel>() : curationLabels;

            return new CurationModel()
            {
                DatasetId = datasetVersion.Id,
                DatasetTitle = datasetVersion.Title,
                DatasetVersionDate = datasetVersion.Timestamp,
                CurationEntries = responseEntries,
                CurationUsers = userMap.Values,
                CurationLabels = curationLabels
            };
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

        private static int userRights(long userId, long datasetId)
        {
            int rights = 0;
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            {
                rights = entityPermissionManager.GetEffectiveRightsAsync(userId, datasetId).Result;
            }
            return rights;
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

            // check if user has rights to access the dataset
            //if (!(userRights(user.Id, datasetid) > 0))
            //    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User has no rights to access the dataset");

            CurationModel responseContent;

            using (var curationManager = new CurationManager())
            using (var datasetManager = new DatasetManager())
            using (var userManager = new UserManager())
            {
                var userWithGroups = userManager.Users
                    .Where(u => u.Id == user.Id)
                    .Fetch(u => u.Groups)
                    .SingleOrDefault();

                var curationEntries = curationManager.CurationEntryRepository.Get().Where(c => c.Dataset.Id == datasetid).ToList();
                var datasetVersion = datasetManager.GetDatasetLatestVersion(datasetid);
                responseContent = AnonymizedCurationResponseContent(datasetVersion, curationEntries, userWithGroups);
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

            // get CurationEntries
            using (var curationManager = new CurationManager())
            using (var datasetManager = new DatasetManager())
            {
                var datasets = datasetManager.DatasetRepo.Get();
                var versions = datasetManager.DatasetVersionRepo.Get();
                var curationEntries = curationManager.CurationEntryRepository.Get();

                var query = from d in datasets
                            join v in versions
                                on d.Id equals v.Dataset.Id into versionGroup
                            join ce in curationEntries
                                on d.Id equals ce.Dataset.Id into curationGroup
                            select new
                            {
                                DatasetId = d.Id,
                                DatasetName = versionGroup
                                    .Where(v => v.Status == DatasetVersionStatus.CheckedIn)
                                    .OrderByDescending(v => v.Timestamp)
                                    .Select(v => v.Title)
                                    .FirstOrDefault() ?? string.Empty,
                                StatusEntry = curationGroup
                                    .FirstOrDefault(x => x.Type == CurationEntryType.StatusEntryItem),
                                AllEntries = curationGroup,
                                FilteredEntries = curationGroup
                                    .Where(e => e.Type != CurationEntryType.StatusEntryItem && e.Type != CurationEntryType.None)
                            } into temp
                            select new
                            {
                                temp.DatasetId,
                                temp.DatasetName,
                                notesComments = temp.StatusEntry != null && temp.StatusEntry.Notes != null
                                    ? temp.StatusEntry.Notes.Select(n => n.Comment).ToList()
                                    : new List<string>(),
                                CurationStarted = temp.StatusEntry != null,
                                UserIsDone = temp.StatusEntry != null ? temp.StatusEntry.UserIsDone : false,
                                IsApproved = temp.StatusEntry != null ? temp.StatusEntry.IsApproved : false,
                                LastChangeDatetime_Curator = temp.AllEntries.Any()
                                    ? temp.AllEntries.Max(e => e.LastChangeDatetime_Curator)
                                    : (DateTime?)null,
                                LastChangeDatetime_User = temp.AllEntries.Any()
                                    ? temp.AllEntries.Max(e => e.LastChangeDatetime_User)
                                    : (DateTime?)null,
                                Count_UserIsDone_True_IsApproved_True = temp.FilteredEntries
                                    .Count(e => e.UserIsDone && e.IsApproved),
                                Count_UserIsDone_True_IsApproved_False = temp.FilteredEntries
                                    .Count(e => e.UserIsDone && !e.IsApproved),
                                Count_UserIsDone_False_IsApproved_True = temp.FilteredEntries
                                    .Count(e => !e.UserIsDone && e.IsApproved),
                                Count_UserIsDone_False_IsApproved_False = temp.FilteredEntries
                                    .Count(e => !e.UserIsDone && !e.IsApproved)
                            };

                var curationLabels = ModuleManager.GetModuleSettings("DDM").GetValueByKey<CurationConfiguration>("curation")?.CurationLabels;

                curationLabels = curationLabels == null ? new List<CurationLabel>() : curationLabels;

                return Request.CreateResponse(HttpStatusCode.OK, new { datasets = query.ToList(), curationLabels });

            }
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
            using (var userManager = new UserManager())
            {
                var c = curationManager.CurationEntries.FirstOrDefault(ce => ce.Id == curationEntryModel.Id);
                if (c == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "curationEntry not found");

                var userWithGroups = userManager.Users
                    .Where(u => u.Id == user.Id)
                    .Fetch(u => u.Groups)
                    .SingleOrDefault();

                // check if user has rights to access the dataset
                //if (!(userRights(user.Id, c.Dataset.Id) > 0))
                //    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User has no rights to access the dataset");

                var newCurationEntry = curationManager.Update(
                        curationEntryModel.Id,
                        curationEntryModel.Topic,
                        curationEntryModel.Type,
                        curationEntryModel.Name,
                        curationEntryModel.Description,
                        curationEntryModel.Solution,
                        curationEntryModel.Position,
                        curationEntryModel.Source,
                        notes,
                        curationEntryModel.UserIsDone,
                        curationEntryModel.IsApproved,
                        userWithGroups
                );

                var response = AnonymizeCurationEntryModel(curationEntryModel, newCurationEntry, userWithGroups);

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

            // check if user has rights to access the dataset
            //if (!(userRights(user.Id, curationEntryModel.DatasetId) > 0))
            //    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User has no rights to access the dataset");

            using (var curationManager = new CurationManager())
            using (var userManager = new UserManager())
            {
                var userWithGroups = userManager.Users
                    .Where(u => u.Id == user.Id)
                    .Fetch(u => u.Groups)
                    .SingleOrDefault();

                if (!CurationEntry.GetCurationUserType(userWithGroups).Equals(CurationUserType.Curator))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User not permitted to create curation entries");

                List<CurationNote> notes;
                if (curationEntryModel.Notes == null || !curationEntryModel.Notes.Any())
                    notes = new List<CurationNote>();
                else
                    notes = curationEntryModel.Notes.Select(n => new CurationNote(user, n.Comment)).ToList(); // all notes will be created for the current user

                if (curationEntryModel.Type == CurationEntryType.StatusEntryItem)
                {
                    if (curationManager.CurationEntries.Any(ce => ce.Dataset.Id == curationEntryModel.DatasetId && ce.Type == CurationEntryType.StatusEntryItem))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "A status entry item already exists for this dataset. Only one status entry item is allowed per dataset.");
                    }
                }
                else if (!curationManager.CurationEntries.Any(ce => ce.Dataset.Id == curationEntryModel.DatasetId))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, "No curation entry exists for this dataset. A status entry item is required before creating other curation entries.");
                }

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
                        curationEntryModel.UserIsDone,
                        curationEntryModel.IsApproved,
                        userWithGroups
                    );

                var response = AnonymizeCurationEntryModel(curationEntryModel, newCurationEntry, userWithGroups);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

    }
}