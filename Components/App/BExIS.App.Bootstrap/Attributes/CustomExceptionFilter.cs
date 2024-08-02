﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;

namespace BExIS.App.Bootstrap.Attributes
{
    public class CustomExceptionFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var exception = filterContext.Exception;
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var statusText = HttpStatusCode.InternalServerError.ToString();

            if (exception is HttpException httpException)
            {
                statusCode = httpException.GetHttpCode();
            }

            var result = new JsonResult
            {
                Data = new
                { 
                    status = statusCode,
                    statusText,
                    error = exception.Message,
                    stackTrace = exception.StackTrace
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            filterContext.Result = result;
            filterContext.HttpContext.Response.StatusCode = statusCode;
            filterContext.ExceptionHandled = true;
        }
    }
}
