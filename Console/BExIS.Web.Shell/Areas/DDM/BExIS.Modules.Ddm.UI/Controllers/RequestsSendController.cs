using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class RequestsSendController : Controller
    {
        // GET: Request
        public JsonResult Send(long id, string intention)
        {
            RequestManager requestManager = new RequestManager();
            SubjectManager subjectManager = new SubjectManager();
            EntityManager entityManager = new EntityManager();
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                long userId = subjectManager.Subjects.Where(s => s.Name.Equals(HttpContext.User.Identity.Name)).Select(s => s.Id).First();
                long entityId = entityManager.Entities.Where(e => e.Name.ToLower().Equals("dataset")).First().Id;

                if (!requestManager.Exists(userId, entityId, id) ||
                    !(requestManager.Exists(userId, entityId, id, Security.Entities.Requests.RequestStatus.Open)))
                {
                    var request = requestManager.Create(userId, entityId, id, 3, intention);

                    if (request != null)
                    {
                        //reload request
                        long requestId = request.Id;
                        request = requestManager.FindById(requestId);

                        var datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                        string title = datasetVersion.Title;
                        if (string.IsNullOrEmpty(title)) title = "No Title available.";

                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;
                        string applicant = getPartyNameOrDefault();

                        //ToDo send emails to owner & requester
                        var es = new EmailService();
                        es.Send(MessageHelper.GetSendRequestHeader(id, applicant),
                            MessageHelper.GetSendRequestMessage(id, title, applicant, intention, request.Applicant.Email),
                            new List<string> { emailDescionMaker }, new List<string> { GeneralSettings.SystemEmail, request.Applicant.Email }, null, new List<string> { request.Applicant.Email }
                            );
                    }
                }
            }
            catch (Exception e)
            {
                Json(e.Message, JsonRequestBehavior.AllowGet);

                // send mail with error to sys admin
                var es = new EmailService();
                es.Send(MessageHelper.GetSendRequestHeader(id, getPartyNameOrDefault()),
                    MessageHelper.GetSendRequestMessage(id, "unknown", "unkown", e.Message + intention, "unknown"), new List<string> { GeneralSettings.SystemEmail }
                    );
            }
            finally
            {
                subjectManager.Dispose();
                requestManager.Dispose();
                entityManager.Dispose();
                datasetManager.Dispose();
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private string getPartyNameOrDefault()
        {
            var userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            if (userName != null)
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var userRepository = uow.GetReadOnlyRepository<User>();
                    var user = userRepository.Query(s => s.Name.ToUpperInvariant() == userName.ToUpperInvariant()).FirstOrDefault();

                    if (user != null)
                    {
                        return user.DisplayName;
                    }
                }
            }
            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }
    }
}