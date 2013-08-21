using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.DCM.Transform.Validation
{
    public interface IValueCheck
    {
        ValueType AppliedTo { get; }
        string Name { get; }
        string DataType { get; }
        object Execute(string value, int row);
    }

}
