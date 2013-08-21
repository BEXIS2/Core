using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BExIS.DCM.Transform.Validation.Exceptions;

namespace BExIS.DCM.Transform.Validation.ValueValidation
{
    public class PatternValidation:IValueValidation
    {
        private ValueType _appliedTo = new ValueType();
        private string _name = "";
        private string _dataType = "";
        private string _pattern = "";
        

        public ValueType AppliedTo
        {
            get { return _appliedTo; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string DataType
        {
            get { return _dataType; }
        }

        public string Pattern
        {
            get { return _pattern; }
        }


        public Error Execute(object value, int row)
        {
            if (value!=null)
            {
                Regex rx = new Regex(_pattern);

                if (rx.IsMatch(value.ToString()))
                {
                    return null;
                }
                else return new Error(ErrorType.Value, "Value not matched on variable pattern.", new object[] { _name, value, row, _dataType });
            }
            return null;
        }

        public PatternValidation(string name, string dataType, string pattern)
        {
            _appliedTo = ValueType.All;
            _name = name;
            _dataType = dataType;
            _pattern = pattern;
        }
    
    }
}
