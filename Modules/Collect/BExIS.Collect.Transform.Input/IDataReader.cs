using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.DCM.Transform.Validation;
using BExIS.DCM.Transform.Validation.DSValidation;
using BExIS.Dlm.Entities.DataStructure;


namespace BExIS.DCM.Transform.Input
{
    public interface IDataReader
    {


        FileStream Open(string fileName);

        DataTuple ReadRow(List<string> row, int indexOfRow);

        List<Error> ValidateRow(List<string> row, int indexOfRow);
        
        List<Error> ValidateComparisonWithDatatsructure(List<VariableIdentifier> row);

        /*
        ValueValidationManager CreateValueValidationManager(string varName, string dataType, bool optional, Variable variable);

        StructuredDataVariableUsage GetVariableUsage(VariableIdentifier hv);

        List<VariableIdentifier> GetDatastructureVariableIdentifiers();

        List<VariableIdentifier> GetDatastructureAsListOfVariableIdentifers(ICollection<StructuredDataVariableUsage> VariableUsageCollection);*/
    }
}
