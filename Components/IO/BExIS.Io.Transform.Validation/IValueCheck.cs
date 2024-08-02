/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface IValueCheck
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        ValueType AppliedTo { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        string Name { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        string DataType { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        object Execute(string value, int row);
    }
}