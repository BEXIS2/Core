using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mappings;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Web.Http;

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
                    Id = i
                });
            }

            return tmp;
        }

        public class LEType
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

    }
}