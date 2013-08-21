using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.DCM.Transform.Validation.Exceptions;


namespace BExIS.DCM.Transform.Validation.ValueValidation
{
    public class RangeValidation:IValueValidation
    {
        # region parameter
        private ValueType _appliedTo = new ValueType();
        private string _name = "";
        private string _dataType = "";
        private double _min = 0;
        private double _max = 0;

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

        public Error Execute(object value, int row)
        {
            if (value!=null && value.ToString()!="")
            {
                if (_dataType.Equals(TypeCode.Int16.ToString()) ||
                   _dataType.Equals(TypeCode.Int32.ToString()))
                {
                    int tempValue = Convert.ToInt32(value);

                    if (tempValue < _min || tempValue > _max) return new Error(ErrorType.Value, "Not in Range", new object[] { _name, value, row, _dataType });
                }

                if (_dataType.Equals(TypeCode.Double.ToString()))
                {
                    double tempValue = Convert.ToDouble(value);

                    if (tempValue < _min || tempValue > _max) return new Error(ErrorType.Value, "Not in Range", new object[] { _name, value, row, _dataType });
                }

                if (_dataType.Equals(TypeCode.DateTime.ToString()))
                {
                    DateTime dt = DateTime.Parse(value.ToString());
                    double tempValue = dt.ToOADate();

                    if (tempValue < _min || tempValue > _max) return new Error(ErrorType.Value, "Not in Range", new object[] { _name, value, row, _dataType });
                }
            }

            return null;
        }

        public RangeValidation(string name, string dataType , double min, double max)
        {

            _appliedTo = ValueType.All;
            _min = min;
            _max = max;
            _name = name;
            _dataType = dataType;

        }


       
    }
}
