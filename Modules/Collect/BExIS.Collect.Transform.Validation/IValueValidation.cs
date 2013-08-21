using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.DCM.Transform.Validation.Exceptions;

namespace BExIS.DCM.Transform.Validation
{
    public interface IValueValidation
    {
        ValueType AppliedTo  { get;}
        string Name { get; }
        string DataType { get; }
        Error Execute(object value, int row);
             
    }

    
    public enum ValueType
    { 
        Number,
        Date,
        String,
        All,
    }
}
