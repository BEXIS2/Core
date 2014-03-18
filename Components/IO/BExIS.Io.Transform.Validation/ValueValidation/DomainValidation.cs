using System.Collections.Generic;
using BExIS.Io.Transform.Validation.Exceptions;

namespace BExIS.Io.Transform.Validation.ValueValidation
{
    public class DomainValidation:IValueValidation
    {
        private ValueType _appliedTo = new ValueType();
        private string _name = "";
        private string _dataType = "";
        private List<string> _checkList = new List<string>();
        

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

        public List<string> CheckList
        {
            get { return _checkList; }
        }


        public Error Execute(object value, int row)
        {
            
            if (value!=null)
            {
                if(CheckList.Contains(value.ToString())) return null;
                else return new Error(ErrorType.Value,"Value is not in domain", new object[] { _name, value, row, _dataType });
            }

            return null;
        }

        public DomainValidation(string name, string dataType, List<string> checkList)
        {
            _appliedTo = ValueType.All;
            _name = name;
            _dataType = dataType;
            _checkList = checkList;
        }
    }
}
