using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Curation;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.Modules.Dcm.UI.Models.Curation;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using Vaiona.Entities.Common;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Controllers
{

    public class CurationEntryController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        [BExISApiAuthorize]
        [JsonNetFilter]
        [GetRoute("api/datasets/{datasetid}/curationentries")]
        public async Task<HttpResponseMessage> Get(long datasetid)
        {
            if (datasetid == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "id should be greater then 0");

            using (var curationManager = new CurationManager())
            {
                List<CurationEntry> curationEntries = curationManager.CurationEntryRepository.Get().Where(c => c.Dataset.Id == datasetid).ToList();
                List<CurationEntryModel> models = new List<CurationEntryModel>();

                List<CurationNoteModel> noteModels = new List<CurationNoteModel>();

                foreach (var ce in curationEntries)
                {
                    var cem = new CurationEntryModel()
                    {
                        Id = ce.Id,
                        Topic = ce.Topic,
                        Type = ce.Type,
                        DatasetId = ce.Dataset.Id,
                        Name = ce.Name,
                        Description = ce.Description,
                        Solution = ce.Solution,
                        Position = ce.Position,
                        Source = ce.Source,
                        CreationDate = ce.CreationDate,
                        CreatorId = ce.Creator.Id,
                        UserlsDone = ce.UserlsDone,
                        IsApproved = ce.IsApproved,
                        LastChangeDatetime_User = ce.LastChangeDatetime_User,
                        LastChangeDatetime_Curator = ce.LastChangeDatetime_Curator
                    };

                    if (ce.Notes != null && ce.Notes.Any())
                    {
                        var x = new List<CurationNoteModel>();
                        ce.Notes.ToList().ForEach(n => x.Add(new CurationNoteModel()
                        {
                            Id = n.Id,
                            UserType = n.UserType,
                            CreationDate = n.CreationDate,
                            Comment = n.Comment,
                            UserId = n.User.Id
                        }));

                        cem.Notes = x;
      
                    }

                    models.Add(cem);

                }

                var response = Request.CreateResponse(HttpStatusCode.OK, models);
                return response;
                 
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [GetRoute("api/curationentries")]
        public async Task<HttpResponseMessage> Get()
        {
       
            using (var curationManager = new CurationManager())
            {
                List<CurationEntry> curationEntries = curationManager.CurationEntryRepository.Get().ToList();
                List<CurationEntryModel> models = new List<CurationEntryModel>();

                List<CurationNoteModel> noteModels = new List<CurationNoteModel>();

                foreach (var ce in curationEntries)
                {
                    var cem = new CurationEntryModel()
                    {
                        Id = ce.Id,
                        Topic = ce.Topic,
                        Type = ce.Type,
                        DatasetId = ce.Dataset.Id,
                        Name = ce.Name,
                        Description = ce.Description,
                        Solution = ce.Solution,
                        Position = ce.Position,
                        Source = ce.Source,
                        CreationDate = ce.CreationDate,
                        CreatorId = ce.Creator.Id,
                        UserlsDone = ce.UserlsDone,
                        IsApproved = ce.IsApproved,
                        LastChangeDatetime_User = ce.LastChangeDatetime_User,
                        LastChangeDatetime_Curator = ce.LastChangeDatetime_Curator
                    };

                    if (ce.Notes != null && ce.Notes.Any())
                    {
                        var x = new List<CurationNoteModel>();
                        ce.Notes.ToList().ForEach(n => x.Add(new CurationNoteModel()
                        {
                            Id = n.Id,
                            UserType = n.UserType,
                            CreationDate = n.CreationDate,
                            Comment = n.Comment,
                            UserId = n.User.Id
                        }));

                        cem.Notes = x;

                    }

                    models.Add(cem);

                }

                var response = Request.CreateResponse(HttpStatusCode.OK, models);
                return response;

            }
        }

        // PUT: api/data/5
        [BExISApiAuthorize]
        [PutRoute("api/curationentries")]
        [JsonNetFilter]
        [HttpPut]
        public async Task<HttpResponseMessage> Put(CurationEntryModel curationEntry)
        {
            if (curationEntry == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "curationEntry is null");
            if (curationEntry.Id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed,"id should be greater then 0");

            using (var curationManager = new CurationManager())
            using (var userManager = new UserManager())
            {
                var c = curationManager.CurationEntries.FirstOrDefault(ce => ce.Id == curationEntry.Id);
                if (c == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "curationEntry not found");

                curationManager.Update(
                    c.Id,
                    c.Topic,
                    c.Name,
                    c.Description,
                    c.Solution,
                    c.Position,
                    c.Source,
                    c.Notes,
                    c.UserlsDone,
                    c.IsApproved);

                return Request.CreateResponse(HttpStatusCode.OK, curationEntry);
            }
           
        }

        [BExISApiAuthorize]
        [PostRoute("api/curationentries")]
        [JsonNetFilter]
        [HttpPost]
        public async Task<HttpResponseMessage> Post(CurationEntryModel curationEntry)
        {
            if (curationEntry == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "curationEntry is null");
            if (curationEntry.Id > 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "curationEntry id must be 0");
            if (curationEntry.DatasetId == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "id should be greater then 0");

            using (var curationManager = new CurationManager())
            using (var userManager = new UserManager())
            {
                List<CurationNote> notes = new List<CurationNote>();

                if (curationEntry.Notes != null)
                {
                    curationEntry.Notes.ToList().ForEach(n => notes.Add(new CurationNote()
                    {
                        Id = n.Id,
                        UserType = n.UserType,
                        CreationDate = n.CreationDate,
                        Comment = n.Comment,
                        User = userManager.FindByIdAsync(n.UserId).Result
                    }));
                }
                curationManager.Create(
                    curationEntry.Topic,
                    curationEntry.Type,
                    curationEntry.DatasetId,
                    curationEntry.Name,
                    curationEntry.Description,
                    curationEntry.Solution,
                    curationEntry.Position,
                    curationEntry.Source,
                    notes,
                    curationEntry.CreatorId,
                    curationEntry.UserlsDone,
                    curationEntry.IsApproved
                    );

                return Request.CreateResponse(HttpStatusCode.OK, curationEntry);
            }

        }

    }
}