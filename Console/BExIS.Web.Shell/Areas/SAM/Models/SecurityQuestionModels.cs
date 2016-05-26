using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.UI.WebControls;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;

namespace BExIS.Web.Shell.Areas.SAM.Models
{
    public class SecurityQuestionCreateModel
    {
        public string Question { get; set; }
    }

    public class SecurityQuestionSelectListItemModel
    {
        public long Id { get; set; }

        public string Question { get; set; }

        public static SecurityQuestionSelectListItemModel Convert(SecurityQuestion question)
        {
            return new SecurityQuestionSelectListItemModel()
            {
                Id = question.Id,
                Question = question.Title
            };
        }
    }

    public class SecurityQuestionSelectListModel
    {
        public List<SecurityQuestionSelectListItemModel> SecurityQuestionList { get; set; }

        public SecurityQuestionSelectListModel()
        {
            SecurityQuestionManager securityQuestionManager = new SecurityQuestionManager();
            SecurityQuestionList = securityQuestionManager.GetAllSecurityQuestions().Select(t => SecurityQuestionSelectListItemModel.Convert(t)).ToList<SecurityQuestionSelectListItemModel>();
        }
    }
}