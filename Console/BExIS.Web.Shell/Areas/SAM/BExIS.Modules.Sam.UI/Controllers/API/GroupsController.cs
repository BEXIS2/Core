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
        // GET: Groups
        [HttpGet, GetRoute("api/groups/{groupId}")]
        public async Task<HttpResponseMessage> GetById(long groupId)
        {
            try
            {
                using (var groupManager = new GroupManager())
                using (var identityGroupService = new IdentityGroupService(groupManager))
                {
                    var group = await identityGroupService.FindByIdAsync(groupId);

                    if (group == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"group with id: {groupId} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadGroupModel.Convert(group));
                }
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
                using (var groupManager = new GroupManager())
                using (var identityGroupService = new IdentityGroupService(groupManager))
                {
                    var groups = identityGroupService.Roles.ToList();

                    var model = groups.Select(g => ReadGroupModel.Convert(g));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
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
                using (var groupManager = new GroupManager())
                using (var identityGroupService = new IdentityGroupService(groupManager))
                {
                    var group = new Group()
                    {
                        Description = model.Description,
                        Name = model.Name
                    };

                    await identityGroupService.CreateAsync(group);

                    return Request.CreateResponse(HttpStatusCode.Created);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, PutRoute("api/groups/{groupId}")]
        public async Task<HttpResponseMessage> PutByIdAsync(long groupId, UpdateGroupModel model)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                var group = await identityGroupService.FindByIdAsync(groupId) ?? throw new ArgumentNullException();

                group.Name = model.Name;
                group.Description = model.Description;

                await identityGroupService.UpdateAsync(group);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [HttpDelete, DeleteRoute("api/groups/{groupId}")]
        public async Task<HttpResponseMessage> DeleteByIdAsync(long groupId)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                await identityGroupService.DeleteByIdAsync(groupId);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}