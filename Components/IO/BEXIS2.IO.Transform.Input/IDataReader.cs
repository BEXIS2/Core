using BEXIS2.Dlm.Entities.Data;
using BEXIS2.IO.Transform.Validation.DSValidation;
using BEXIS2.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;
using System.IO;

/// <summary>
///
/// </summary>
namespace BEXIS2.IO.Transform.Input
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface IDataReader
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="fileName"></param>
        FileStream Open(string fileName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="row"></param>
        /// <param name="indexOfRow"></param>
        DataTuple ReadRow(List<string> row, int indexOfRow);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="row"></param>
        /// <param name="indexOfRow"></param>
        List<Error> ValidateRow(List<string> row, int indexOfRow);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="row"></param>
        List<Error> ValidateComparisonWithDatatsructure(List<VariableIdentifier> row);
    }
}