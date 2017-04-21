using BExIS.Security.Entities.Authentication;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace BExIS.Security.Entities.Subjects
{
    public class User : Subject, IUser<long>
    {
        public virtual int AccessFailedCount { get; set; }
        public virtual string Email { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual DateTime? LockoutEndDate { get; set; }
        public virtual ICollection<Login> Logins { get; set; }
        public virtual string Password { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual string SecurityStamp { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }

        public virtual string UserName
        {
            get { return Name; }
            set { Name = value; }
        }

        public User()
        {
            Groups = new List<Group>();
            Logins = new List<Login>();
        }
    }
}