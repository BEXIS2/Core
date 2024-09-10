using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
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
using System.Web.Routing;


namespace BExIS.Modules.Ddm.UI.Controllers.API
{
    public class TagInfoEditController : ApiController
    {
        // GET: TagInfos
        [JsonNetFilter]
        [HttpGet, GetRoute("api/datasets/{id}/tags")]
        public async Task<HttpResponseMessage> Get(long id)
        {
            try
            {
                if(id <= 0) throw new ArgumentException("id is not valid");

                using (DatasetManager datasetmanager = new DatasetManager())
                { 
                    List<TagInfoEditModel> tags = new List<TagInfoEditModel>();
                    TagInfoHelper _helper = new TagInfoHelper();
                    var versions = datasetmanager.GetDatasetVersions(id);

                   
                    if (versions != null)
                    {
                        tags = _helper.ConvertTo(versions, datasetmanager);
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
        public async Task<HttpResponseMessage> Post(TagInfoEditModel model)
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
                    dsv.Show = model.Show;

                    datasetManager.UpdateDatasetVersion(dsv);


                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [JsonNetFilter]
        [HttpPut, PutRoute("api/taginfo")]
        public async Task<HttpResponseMessage> Put(TagInfoEditModel model, TagType type)
        {
            try
            {
                if (model == null) throw new ArgumentException("model is not valid");
                //if (model.TagId > 0) throw new ArgumentException("Tag allready exist");

                using (var tagManager = new TagManager())
                using (var datasetManager = new DatasetManager())
                {
                    var version = datasetManager.GetDatasetVersion(model.VersionId);

                    if (version == null) throw new ArgumentException("Version not exist");
                    //if(version.Tag != null) throw new ArgumentException("Version has allready a tag");

                    var latestTag = datasetManager.GetLatestTag(version.Dataset.Id);
                    var newTag = datasetManager.IncreaseTag(latestTag,type);

                    newTag = tagManager.Create(newTag);


                    // update all versions before without tag
                    var vIDs = datasetManager.GetAllVersionAfterLastTag(version.Dataset.Id, latestTag, version.Id);

                    foreach (var vID in vIDs)
                    {
                        var v = datasetManager.GetDatasetVersion(vID);
                        v.Tag = newTag;
                        datasetManager.UpdateDatasetVersion(v);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }

    public class TagInfoViewController : ApiController
    {
        // GET: TagInfos
        [JsonNetFilter]
        [HttpGet, GetRoute("api/datasets/{id}/tags/simple")]
        public async Task<HttpResponseMessage> Get(long id)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("id is not valid");

                using (DatasetManager datasetmanager = new DatasetManager())
                {
                    List<TagInfoViewModel> tags = new List<TagInfoViewModel>();
                    TagInfoHelper _helper = new TagInfoHelper();
                    var versions = datasetmanager.GetDatasetVersions(id);


                    if (versions != null)
                    {
                        tags = _helper.GetViews(versions, datasetmanager);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, tags);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}