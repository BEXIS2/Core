using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class ExtendedProperty : BaseEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Description { get; set; }

        #endregion

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual DataContainer DataContainer { get; set; }

        // Its not needed, make the database more complex, Javad/ 03.07.13
        //public virtual ICollection<Constraint> Constraints { get; set; }

        //// not to map
        //public IList<DomainValueConstraint> DomainValues
        //{
        //    get
        //    {
        //        return (this.Constraints.Distinct()
        //                    .Where(p => p.GetType().Equals(typeof(DomainValueConstraint)))
        //                    .ToList() as IList<DomainValueConstraint>
        //                );
        //    }
        //}

        //// not to map
        //public IList<ValidatorConstraint> Validators
        //{
        //    get
        //    {
        //        return (this.Constraints.Distinct()
        //                    .Where(p => p.GetType().Equals(typeof(ValidatorConstraint)))
        //                    .ToList() as IList<ValidatorConstraint>
        //                );
        //    }
        //}

        #endregion

        #region Mathods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public ExtendedProperty()
        {
            //Constraints = new List<Constraint>();
        }
        
        #endregion
    }
}
