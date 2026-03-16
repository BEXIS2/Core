using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using Microsoft.AspNet.Identity;
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
    public class UsersController : ApiController
    {
        private readonly UserManager _userManager;

        public UsersController(UserManager userManager)
        {
            _userManager = userManager;
        }

        // GET: Groups
        [HttpGet, GetRoute("api/users/{userId}")]
        public async Task<HttpResponseMessage> GetByIdAsync(long userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"user with id: {userId} does not exist.");

                return Request.CreateResponse(HttpStatusCode.OK, ReadUserModel.Convert(user));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/users/{userId}/groups")]
        public async Task<HttpResponseMessage> GetGroupsByUserIdAsync(long userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"user with id: {userId} does not exist.");

                var groups = user.Groups.Select(g => ReadGroupModel.Convert(g));

                return Request.CreateResponse(HttpStatusCode.OK, groups);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/users")]
        public async Task<HttpResponseMessage> GetAsync()
        {
            try
            {
                var users = _userManager.Users.ToList();

                var model = users.Select(u => ReadUserModel.Convert(u));

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, PostRoute("api/users")]
        public async Task<HttpResponseMessage> PostAsync(CreateUserModel model)
        {
            try
            {
                var user = new User()
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                await _userManager.CreateAsync(user);

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, PutRoute("api/users/{userId}")]
        public async Task<HttpResponseMessage> PutByIdAsync(long userId, UpdateUserModel model)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new ArgumentNullException();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPut, PutRoute("api/users/{userId}/groups")]
        public async Task<HttpResponseMessage> PutGroupsByUserIdAsync(long userId, List<string> groupNames)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new ArgumentNullException();

            foreach (var groupName in groupNames)
            {
                await _userManager.AddToRoleAsync(user.Id, groupName);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete, DeleteRoute("api/users/{userId}")]
        public async Task<HttpResponseMessage> DeleteByIdAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new ArgumentNullException();
            await _userManager.DeleteAsync(user);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete, DeleteRoute("api/users/{userId}/groups")]
        public async Task<HttpResponseMessage> DeleteByIdAsync(long userId, List<string> groupNames)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new ArgumentNullException();

            foreach (var groupName in groupNames)
            {
                await _userManager.RemoveFromRoleAsync(user.Id, groupName);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}