using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Services;
using BExIS.Utils.Route;
using System.Collections.Generic;
using System.Web.Http;

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