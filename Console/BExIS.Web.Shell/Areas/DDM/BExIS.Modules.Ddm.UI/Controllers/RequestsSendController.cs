using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;

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
                var user = BExISAuthorizeHelper.GetUserFromAuthorization(HttpContext);

                if(user == null) throw new HttpException(401, "Unauthorized");

                long userId = user.Id;
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

                        // collect all users with the configured party relationship to the dataset (e.g. data creators) to inform them
                        List<string> ccEmails = new List<string> { GeneralSettings.SystemEmail, request.Applicant.Email };
                        var bamSettings = ModuleManager.GetModuleSettings("bam");
                        string notificationRelationshipType = bamSettings.GetValueByKey("DataRequestNotificationRelationshipType")?.ToString();

                        if (!string.IsNullOrEmpty(notificationRelationshipType))
                        {
                            using (var partyManager = new PartyManager())
                            using (var uow = this.GetUnitOfWork())
                            {
                                var partyTypeRepository = uow.GetReadOnlyRepository<PartyType>();
                                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();
                                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();

                                var datasetPartyType = partyTypeRepository.Query(m => m.Title == "Dataset").FirstOrDefault();
                                if (datasetPartyType != null)
                                {
                                    var datasetParty = partyManager.Parties.FirstOrDefault(m => m.Name == id.ToString() && m.PartyType.Id == datasetPartyType.Id);
                                    if (datasetParty != null)
                                    {
                                        var relationships = partyRelationshipRepository.Query(
                                            m => m.PartyRelationshipType.Title == notificationRelationshipType &&
                                            m.TargetParty.Id == datasetParty.Id).ToList();

                                        foreach (var rel in relationships)
                                        {
                                            var partyUser = partyUserRepository.Query(m => m.Party.Id == rel.SourceParty.Id).FirstOrDefault();
                                            if (partyUser != null)
                                            {
                                                var user2 = uow.GetReadOnlyRepository<User>().Get(partyUser.UserId);
                                                if (user2 != null && !string.IsNullOrEmpty(user2.Email) &&
                                                    !ccEmails.Contains(user2.Email) &&
                                                    !user2.Email.Equals(emailDescionMaker, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    ccEmails.Add(user2.Email);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (var emailService = new EmailService())
                        {
                            emailService.Send(MessageHelper.GetSendRequestHeader(id, applicant),
                                MessageHelper.GetSendRequestMessage(id, title, applicant, intention, request.Applicant.Email),
                                new List<string> { emailDescionMaker }, ccEmails, null, new List<string> { request.Applicant.Email }
                                );
                        }
                            
                    }
                }
            }
            catch (Exception e)
            {
                Json(e.Message, JsonRequestBehavior.AllowGet);

                using(var emailService = new EmailService())
                {
                    emailService.Send(MessageHelper.GetSendRequestHeader(id, getPartyNameOrDefault()),
                    MessageHelper.GetSendRequestMessage(id, "unknown", "unkown", e.Message + intention, "unknown"), new List<string> { GeneralSettings.SystemEmail }
                    );
                }

                throw e;
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