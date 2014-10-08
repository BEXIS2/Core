using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Security
{
    public class Authenticator : BaseEntity
    {
        #region Attributes

        public virtual string Alias { get; set; }

        public virtual string ProjectPath { get; set; }
        public virtual string ClassPath { get; set; }

        public virtual string ConnectionString { get; set; }

        public virtual bool Locked { get; set; }

        #endregion

        #region Associations

        public virtual ICollection<User> Users { get; set; }

        #endregion

        #region Methods

        public Authenticator()
        {
            Users = new List<User>();
        }

        public Authenticator(string alias, string projectPath, string classPath, string connectionString)
        {
            this.Alias = alias;
            this.ProjectPath = projectPath;
            this.ClassPath = classPath;
            this.ConnectionString = connectionString;
        }

        #endregion
    }
}
