using BExIS.Dlm.Entities.Party;
using BExIS.Security.Entities.Subjects;
using System;

namespace BEXIS.Modules.SAM.UI.Model
{
    public class FormerMemberUserModel
    {
        /// <summary>
        /// System username of a user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// System username of a user.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// True or false, is user in former member role.
        /// </summary>
        public bool IsFormerMember { get; set; }

        /// <summary>
        /// Start date of user party.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date  of user party.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Fullname if system user.
        /// </summary>
        public string Name { get; set; }

        public FormerMemberUserModel(User user, bool isFormerMember, Party party)
        {
            UserName = user.UserName;
            UserEmail = user.Email;
            IsFormerMember = isFormerMember;
            Name = user.FullName;
            StartDate = party.StartDate;
            EndDate = party.EndDate;
        }

        public FormerMemberUserModel(User user, bool isFormerMember)
        {
            UserName = user.UserName;
            IsFormerMember = isFormerMember;
            Name = user.FullName;
            StartDate = new DateTime();
            EndDate = new DateTime();
        }
    }
}