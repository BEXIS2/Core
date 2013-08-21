using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.DCM.Transform.Validation.ValueCheck
{
    public class OptionalCheck:IValueCheck
    {
        # region parameter

            private ValueType _appliedTo = new ValueType();
            private bool _optional = false;
            private string _name = "";
            private string _dataType = "";

            #region get
            
            public ValueType AppliedTo
            {
                get
                {
                    return _appliedTo;
                }
            }

            public string Name
            {
                get { return _name; }
            }

            public string DataType
            {
                get { return _dataType; }
            }
            
            #endregion

        #endregion

        public object Execute(string value, int row)
        {
            if (String.IsNullOrEmpty(value) && this._optional == false)
            {
                // create Error Object
                Error e = new Error(ErrorType.Value, "Is empty and not optional ", new object[] { _name, "empty", row, _dataType });
                return e;
            }
            return value;
        }

        public OptionalCheck(string name, string dataType, bool optional)
        {
            _appliedTo = ValueType.All;
            _optional = optional;
            _name = name;
            _dataType = dataType;

        }

    }
}
