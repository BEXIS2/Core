using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.UI.Helpers;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class MeaningController : Controller
    {
        // GET: Meanings
        public ActionResult Index()
        {
            string module = "rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        #region meanings

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult Get()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<Meaning> res = _meaningManager.getMeanings();
                return Json(res, JsonRequestBehavior.AllowGet);
            }

        }


        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult Create(Meaning data)
        {
            try {

                using (var _meaningManager = new MeaningManager())
                {
                    Meaning res = _meaningManager.addMeaning(data);
                    return Json(res);
                }

            }
            catch(Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }



        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult Update(Meaning data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    Meaning res = _meaningManager.editMeaning(data);
                    return Json(res);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpDelete]
        public JsonResult Delete(long id)
        {
            try
            {

                using (var _meaningManager = new MeaningManager())
                {
                    _meaningManager.deleteMeaning(id);
                    return Json(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }

        #endregion

        #region External Links


        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetLinks()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<ExternalLink> res = _meaningManager.getExternalLinks();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult CreateLink(ExternalLink data)
        {
            try
            {

                using (var _meaningManager = new MeaningManager())
                {
                    ExternalLink res = _meaningManager.addExternalLink(data);
                    return Json(res);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult UpdateExternalLink(ExternalLink data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    ExternalLink res = _meaningManager.editExternalLink(data);
                    return Json(res);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }



        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpDelete]
        public JsonResult DeleteLink(long id)
        {
            try
            {

                using (var _meaningManager = new MeaningManager())
                {
                    _meaningManager.deleteExternalLink(id);
                    return Json(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        #endregion;

        /*
        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/DetailExternalLinks")]
        [GetRoute("api/Meanings/DetailExternalLinks")]
        public HttpResponseMessage DetailExternalLinks()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getExternalLink(long.Parse(dict["id"])));
        }


        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/getPrefixCategory")]
        [GetRoute("api/Meanings/getPrefixCategory")]
        public HttpResponseMessage getPrefixCategory()
        {
            return cretae_response(_meaningManager.getPrefixCategory());
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/DetailPrefixCategory")]
        [GetRoute("api/Meanings/DetailPrefixCategory")]
        public HttpResponseMessage DetailPrefixCategory()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getPrefixCategory(long.Parse(dict["id"])));
        }
        private HttpResponseMessage cretae_response(object return_object)
        {
            if (return_object == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "bad request / problem occured");
            var response = Request.CreateResponse(HttpStatusCode.OK);
            string resp = JsonConvert.SerializeObject(return_object);

            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //set headers on the "response"
            return response;
        }

        private HttpResponseMessage cretae_response(List<Object> return_object)
        {
            if (return_object == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "bad request / problem occured");
            var response = Request.CreateResponse(HttpStatusCode.OK);
            string resp = JsonConvert.SerializeObject(return_object);

            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //set headers on the "response"
            return response;
        }
        */
    }
}