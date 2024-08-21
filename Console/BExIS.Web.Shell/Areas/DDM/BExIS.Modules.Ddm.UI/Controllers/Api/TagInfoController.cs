using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
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
    public class TagInfoController : ApiController
    {
        // GET: TagInfos
        [JsonNetFilter]
        [HttpGet, GetRoute("api/taginfo/{id}")]
        public async Task<HttpResponseMessage> Get(long id)
        {
            try
            {
                if(id <= 0) throw new ArgumentException("id is not valid");

                using (DatasetManager datasetmanager = new DatasetManager())
                { 
                    List<TagInfoModel> tags = new List<TagInfoModel>();
                    TagInfoHelper _helper = new TagInfoHelper();
                    var versions = datasetmanager.GetDatasetVersions(id);

                    if (versions != null)
                    {
                        tags = _helper.ConvertTo(versions);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, tags);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [JsonNetFilter]
        [HttpPost, PostRoute("api/taginfo")]
        public async Task<HttpResponseMessage> Post(TagInfoModel model)
        {
            try
            {
                if(model == null) throw new ArgumentException("model is not valid");
                if(model.TagId <= 0) throw new ArgumentException("TagId is not valid");

                using (var tagManager = new TagManager())
                using (var datasetManager = new DatasetManager())
                {
                    // update tag
                    var tag = tagManager.Repo.Get(model.TagId);
                    TagInfoHelper _helper = new TagInfoHelper();
                    tag = _helper.Update(model, tag);

                    var result = tagManager.Update(tag);

                    // update Version
                    var dsv = datasetManager.GetDatasetVersion(model.VersionId);
                    dsv.ChangeDescription = model.ReleaseNote;

                    datasetManager.UpdateDatasetVersion(dsv);

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