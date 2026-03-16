using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers.API
{
    public class GroupsController : ApiController
    {
        private readonly GroupManager _groupManager;

        public GroupsController(GroupManager groupManager)
        {
            _groupManager = groupManager;
        }

        // GET: Groups
        [HttpGet, GetRoute("api/groups/{groupId}")]
        public async Task<HttpResponseMessage> GetById(long groupId)
        {
            try
            {
                var group = await _groupManager.FindByIdAsync(groupId);

                if (group == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"group with id: {groupId} does not exist.");

                return Request.CreateResponse(HttpStatusCode.OK, ReadGroupModel.Convert(group));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/groups")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                var groups = _groupManager.Roles.ToList();

                var model = groups.Select(g => ReadGroupModel.Convert(g));

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, PostRoute("api/groups")]
        public async Task<HttpResponseMessage> Post(CreateGroupModel model)
        {
            try
            {
                var group = new Group()
                {
                    Description = model.Description,
                    Name = model.Name
                };

                await _groupManager.CreateAsync(group);

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, PutRoute("api/groups/{groupId}")]
        public async Task<HttpResponseMessage> PutByIdAsync(long groupId, UpdateGroupModel model)
        {
            var group = await _groupManager.FindByIdAsync(groupId) ?? throw new ArgumentNullException();

            group.Name = model.Name;
            group.Description = model.Description;

            await _groupManager.UpdateAsync(group);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete, DeleteRoute("api/groups/{groupId}")]
        public async Task<HttpResponseMessage> DeleteByIdAsync(long groupId)
        {
            var deleted = _groupManager.Delete(groupId);

            if (deleted)
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}