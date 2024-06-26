﻿using BExIS.Modules.Sam.UI.Models;
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
    public class UsersController : ApiController
    {
        // GET: Groups
        [HttpGet, GetRoute("api/users/{id}")]
        public async Task<HttpResponseMessage> GetById(long id)
        {
            try
            {
                using (var userManager = new UserManager())
                {
                    var user = await userManager.FindByIdAsync(id);

                    if (user == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"user with id: {id} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadUserModel.Convert(user));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/users")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var userManager = new UserManager())
                {
                    var users = userManager.Users.ToList();

                    var model = users.Select(u => ReadUserModel.Convert(u));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, PostRoute("api/users")]
        public async Task<HttpResponseMessage> Post(CreateUserModel model)
        {
            try
            {
                using (var userManager = new UserManager())
                {
                    var user = new User()
                    {
                        UserName = model.UserName,
                        Email = model.Email
                    };

                    var result = userManager.CreateAsync(user);

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