using System.Collections.Generic;

namespace BExIS.Security.Entities
{
    public class Role : Subject
    {
        #region Attributes

        public virtual string Description { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<User> Users { get; set; }

        #endregion


        #region Methods

        public Role()
        {
            Users = new List<User>();
        }

        #endregion

    }
}
