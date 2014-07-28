using System.Collections.Generic;
using System.IO;
using BExIS.Io.Transform.Validation.DSValidation;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Data;

/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Input
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

        /*
        ValueValidationManager CreateValueValidationManager(string varName, string dataType, bool optional, Variable variable);

        StructuredDataVariableUsage GetVariableUsage(VariableIdentifier hv);

        List<VariableIdentifier> GetDatastructureVariableIdentifiers();

        List<VariableIdentifier> GetDatastructureAsListOfVariableIdentifers(ICollection<StructuredDataVariableUsage> VariableUsageCollection);*/
    }
}
