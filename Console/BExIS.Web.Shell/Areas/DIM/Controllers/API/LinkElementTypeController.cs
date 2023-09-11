using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Versions;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata of a datasets and get the result in XML.
    /// </summary>
    public class LinkElementTypeController : ApiController
    {

        [BExISApiAuthorize]
        [GetRoute("api/LinkElementType")]
        public IEnumerable<LEType> Get()
        {
            List<LEType> tmp = new List<LEType>();
            foreach (int i in Enum.GetValues(typeof(LinkElementType)))
            {
                String name = Enum.GetName(typeof(LinkElementType), i);
                tmp.Add(new LEType
                {
                    Name = name,
                    Number = i
                });
            }

            return tmp;
        }

        public class LEType
        {
            public long Number { get; set; }
            public string Name { get; set; }
        }

    }
}