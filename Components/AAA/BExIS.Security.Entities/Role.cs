using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Entities
{
    public class Role : Subject
    {
        #region Attributes



        #endregion


        #region Associations

        public virtual ICollection<User> Users { get; set; }

        #endregion


        #region Methods

        public Role()
        {
            Users = new List<User>();
            FeatureRules = new List<FeatureRule>();
        }

        #endregion

    }
}
