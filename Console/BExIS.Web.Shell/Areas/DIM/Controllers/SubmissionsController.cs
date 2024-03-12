using BExIS.Dim.Services.Submissions;
using BExIS.Modules.Dim.UI.Models.Submissions;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
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

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Submissions_Select(GridCommand command)
        {
            using (var submissionManager = new SubmissionManager())
            {
                var submissions = new List<ReadSubmissionModel>();
                int count = submissionManager.Submissions.Count();
                if (command != null)// filter subjects based on grid filter settings
                {
                    FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                    submissions = submissionManager.GetSubmissions(filter, orderBy, command.Page, command.PageSize, out count).Select(ReadSubmissionModel.Convert).ToList();
                }
                else
                {
                    submissions = submissionManager.Submissions.Select(ReadSubmissionModel.Convert).ToList();
                    count = submissionManager.Submissions.Count();

                }

                return View(new GridModel<ReadSubmissionModel> { Data = submissions, Total = count });
            }
        }
    
    
    }
}