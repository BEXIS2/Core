using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.BIOSCHEMA;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Models;
using BExIS.Utils.Config;
using BExIS.Utils.Data;
using BExIS.Utils.Extensions;
using BExIS.Utils.NH.Querying;
using BExIS.Xml.Helpers;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : BaseController
    {
        public ActionResult ShowData(long id, int version = 0, bool asPartial = false, string versionName = "", double tag = 0)
        {
            return Redirect("/dcm/view?id=" + id + "&version=" + version + "&tag=" + tag);
        }
    }
}