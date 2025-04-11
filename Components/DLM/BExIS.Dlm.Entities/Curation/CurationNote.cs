using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Curation
{
    public class CurationNote: BaseEntity
    {
        public virtual CurationUserType UserType { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string Comment { get; set; }
        public virtual User User { get; set; }

        public CurationNote()
        {
            Id = 0;
            UserType = CurationUserType.User;
            CreationDate = DateTime.Now;
            Comment = "";
            User = null;
        }

        public CurationNote(int id, CurationUserType userType, DateTime creationDate, string comment, User user)
        {
            Id = id;
            UserType = userType;
            CreationDate = creationDate;
            Comment = comment;
            User = user;
        }

        public CurationNote(User user, string comment)
        {
            Id = 0;
            UserType = CurationEntry.GetCurationUserType(user);
            CreationDate = DateTime.Now;
            Comment = comment;
            User = user;
        }

    }

    public enum CurationUserType
    {
        User,
        Curator
    }
}
