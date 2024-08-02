using BExIS.IO.Transform.Validation.Exceptions;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface IValueValidation
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
        Error Execute(object value, int row);
    }

    /// <summary>
    ///
    /// </summary>
    public enum ValueType
    {
        Number,
        Date,
        String,
        All,
    }
}