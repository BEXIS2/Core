using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Entities.Subjects
{
    public class Group : Subject
    {
        #region Attributes

        public virtual string Description { get; set; }

        #endregion

        #region Associations

        public virtual ICollection<User> Users { get; set; }

        #endregion

        #region Methods

        public Group()
        {
            Users = new List<User>();
        }

        #endregion
    }
}
