using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
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
        [HttpGet, GetRoute("api/groups/{id}")]
        public async Task<HttpResponseMessage> GetById(long id)
        {
            try
            {
                using (var groupManager = new GroupManager())
                {
                    var group = await groupManager.FindByIdAsync(id);

                    if (group == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"group with id: {id} does not exist.");

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
                {
                    var groups = groupManager.Groups.ToList();

                    var model = groups.Select(g => ReadGroupModel.Convert(g));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, GetRoute("api/groups")]
        public async Task<HttpResponseMessage> Post(CreateGroupModel model)
        {
            try
            {
                using (var groupManager = new GroupManager())
                {
                    var group = new Group()
                    {
                        Description = model.Description,
                        Name = model.Name
                    };

                    var result = groupManager.CreateAsync(group);

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}