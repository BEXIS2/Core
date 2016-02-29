using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authentication
{
    public class Authenticator : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }

        public virtual string AssemblyPath { get; set; }
        public virtual string ClassPath { get; set; }

        public virtual string ConnectionString { get; set; }

        public virtual AuthenticatorType AuthenticatorType { get; set; }

        #endregion

        #region Associations

        public virtual ICollection<User> Users { get; set; }

        #endregion

        #region Methods

        public Authenticator()
        {
            Users = new List<User>();
        }

        #endregion
    }
}
