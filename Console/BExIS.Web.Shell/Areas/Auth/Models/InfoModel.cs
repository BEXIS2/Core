using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class InfoModel
    {
        public string WindowName { get; set; }
        public string Message { get; set; }

        public InfoModel(string windowName, string message)
        {
            WindowName = windowName;
            Message = message;
        }
    }
}