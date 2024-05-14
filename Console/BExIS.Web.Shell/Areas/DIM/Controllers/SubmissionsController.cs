using BExIS.Dim.Services.Submissions;
using BExIS.Modules.Dim.UI.Models.Submissions;
using BExIS.Security.Services.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class SubmissionsController : Controller
    {
        // GET: Submissions
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult Submissions_Select()
        {

            using (var submissionManager = new SubmissionManager())
            {
                try
                {
                    var submissions = new List<ReadGridRowSubmissionModel>();

                    foreach (var submission in submissionManager.Submissions)
                    {
                        submissions.Add(ReadGridRowSubmissionModel.Convert(submission));
                    }

                    return View(new GridModel<ReadGridRowSubmissionModel> { Data = submissions });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }   
        }
    }
}