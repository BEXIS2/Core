using BExIS.Dlm.Services.Party;
using BExIS.Modules.SAM.UI.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BEXIS.Modules.SAM.UI.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Services;

namespace BEXIS.Modules.SAM.UI.Controllers
{
    public class FormerMemberController : Controller
    {
        public ActionResult Index()
        {
            SettingsHelper helper = new SettingsHelper();

            List<FormerMemberUserModel> model = new List<FormerMemberUserModel>();
            string formerMemberRole = helper.GetValue("formerMemberRole").ToString();

            using (GroupManager groupManager = new GroupManager())
            using (UserManager userManager = new UserManager())
            using (var partyManager = new PartyManager())
            {
                var alumniGroup = groupManager.Groups.Where(g => g.Name.ToLower() == formerMemberRole.ToLower()).FirstOrDefault();
                if (alumniGroup != null)
                {
                    List<object> userObjectList = new List<object>();

                    foreach (User user in userManager.Users)
                    {
                        var party = partyManager.GetPartyByUser(user.Id);
                        if (party != null)
                            model.Add(new FormerMemberUserModel(user, FormerMemberStatus.IsFormerMember(user.Id, formerMemberRole), party));
                        else
                            model.Add(new FormerMemberUserModel(user, FormerMemberStatus.IsFormerMember(user.Id, formerMemberRole)));
                    }
                }
                else
                {
                    ViewData.ModelState.AddModelError("alumni role", "Error: Alumni group not set in SAM settings or does not exist (current group name:" + formerMemberRole + ")");
                }

                return View("ManageFormerMembers", model);
            }
        }

        /// <summary>
        /// Change status to defined former member role
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public void ChangeStatusToFormerMember(string userName)
        {
            SettingsHelper helper = new SettingsHelper();
            string formerMemberRole = helper.GetValue("formerMemberRole").ToString();

            if (userName != null)
            {
                using (UserManager userManager = new UserManager())
                {
                    var userTask = userManager.FindByNameAsync(userName);
                    userTask.Wait();
                    var user = userTask.Result;
                    bool isAlumni = FormerMemberStatus.IsFormerMember(user.Id, formerMemberRole);
                    if (!isAlumni)
                    {
                        FormerMemberStatus.ChangeToFormerMember(user, formerMemberRole);

                        //send mail
                        var es = new EmailService();
                        //build email with text blocks from the settings file
                        string mailTextBody =   helper.GetValue("mailTextTitle").ToString() + " " + user.DisplayName + "," + "<br/><br/>" +
                                                helper.GetValue("mailTextMainApplied").ToString() + "<br/><br/>" +
                                                helper.GetValue("mailTextClosing").ToString();

                        //if there is no subject defined in settings use system subject
                        string subject;
                        if (!string.IsNullOrEmpty(helper.GetValue("mailTextSubject").ToString()))
                            subject = helper.GetValue("mailTextSubject").ToString();
                        else
                            subject = MessageHelper.GetChangedRoleHeader(user.DisplayName, formerMemberRole, "set to");

                        es.Send(subject,
                                mailTextBody,
                                new List<string>() { user.Email },
                                new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                );

                    }
                }
            }
        }
        /// <summary>
        /// Remove status to defined former member role
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public void ChangeStatusToNonFormerMember(string userName)
        {
            SettingsHelper helper = new SettingsHelper();
            string formerMemberRole = helper.GetValue("formerMemberRole").ToString();

            if (userName != null)
            {
                using (UserManager userManager = new UserManager())
                {
                    var userTask = userManager.FindByNameAsync(userName);
                    userTask.Wait();
                    var user = userTask.Result;
                    bool isAlumni = FormerMemberStatus.IsFormerMember(user.Id, formerMemberRole);
                    if (isAlumni)
                    {
                        FormerMemberStatus.ChangeToNonFormerMember(user, formerMemberRole);

                        //send mail
                        var es = new EmailService();
                        //build email with text blocks from the settings file
                        string mailTextBody = helper.GetValue("mailTextTitle").ToString() + " " + user.DisplayName + "," + "<br/><br/>" +
                                              helper.GetValue("mailTextMainRevoked").ToString() + "<br/><br/>" +
                                             helper.GetValue("mailTextClosing").ToString();

                        //if there is no subject defined in settings use system subject
                        string subject;
                        if (!string.IsNullOrEmpty(helper.GetValue("mailTextSubject").ToString()))
                            subject = helper.GetValue("mailTextSubject").ToString();
                        else
                            subject = MessageHelper.GetChangedRoleHeader(user.DisplayName, formerMemberRole, "set to");

                        es.Send(subject,
                                mailTextBody,
                                new List<string>() { user.Email },
                                new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                );
                    }
                }
            }
        }

        [WebMethod]
        public JsonResult GetAllUsers()
        {
            List<FormerMemberUserModel> model = new List<FormerMemberUserModel>();

            using (UserManager userManager = new UserManager())
            using (var partyManager = new PartyManager())
            {
                List<FormerMemberUserModel> users = new List<FormerMemberUserModel>();

                foreach (User user in userManager.Users)
                {
                    var party = partyManager.GetPartyByUser(user.Id);
                    if (party != null)
                        users.Add(new FormerMemberUserModel(user, false, party));
                    else
                        users.Add(new FormerMemberUserModel(user, false));
                }

                return Json(users, JsonRequestBehavior.AllowGet);
            }
        }
    }
}