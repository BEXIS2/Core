using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    /// Name and descroption, and etc. of methods of obtaining data
    /// like: Measurement, Observation, Processing, Simulation, Interpretation and Description
    /// </summary>
    /// <remarks></remarks>
    public class ObtainingMethod : BaseEntity
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Description { get; set; }
    }
}