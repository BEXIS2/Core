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
    public class ConceptOutController : ApiController
    {

        [BExISApiAuthorize]
        [GetRoute("api/Concept")]
        public IEnumerable<Concept> Get()
        {
            List<Concept> tmp = new List<Concept>();

            using (var conceptManager = new ConceptManager())
            {

                foreach (var c in conceptManager.MappingConceptRepo.Get())
                {
                    tmp.Add(new Concept()
                    {
                        Id = c.Id,
                        Name = c.Name
                    });
                }
            }

            return tmp;

        }

        public class Concept
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

    }
}