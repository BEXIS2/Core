using Vaiona.Entities.Common;

namespace BExIS.Security.Entities
{
    public abstract class Subject : BaseEntity
    {
        #region Attributes

        // SUBJECT
        public virtual string Name { get; set; }

        public virtual string Comment { get; set; }

        #endregion


        #region Associations

        #endregion


        #region Methods

        #endregion
    }
}