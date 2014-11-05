using System.Collections.Generic;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using Vaiona.Entities.Common;
     
namespace BExIS.Security.Entities.Subjects
{  
    public abstract class Subject : BaseEntity
    {
        #region Attributes
      
        public virtual string Name { get; set; }

        #endregion


        #region Associations
       
        public virtual ICollection<Permission> Permissions { get; set; }

        #endregion


        #region Methods
      
        public Subject()
        {
            Permissions = new List<Permission>();
        }

        #endregion
    }
}