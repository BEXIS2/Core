using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class SecurityQuestionCreateModel
    {

        [Required]
        [Display(Name = "Security Question")]
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
                Question = question.Question
            };
        }
    }

    public class SecurityQuestionSelectListModel
    {
        public long Id { get; set; }

        public List<SecurityQuestionSelectListItemModel> SecurityQuestionList { get; set; }

        public SecurityQuestionSelectListModel()
        {
            SecurityQuestionManager securityQuestionManager = new SecurityQuestionManager();
            SecurityQuestionList = securityQuestionManager.GetAllSecurityQuestions().Select(t => SecurityQuestionSelectListItemModel.Convert(t)).ToList<SecurityQuestionSelectListItemModel>();
        }
    }
}