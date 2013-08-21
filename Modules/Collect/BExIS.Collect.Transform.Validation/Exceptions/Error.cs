using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.DCM.Transform.Validation.Exceptions
{
    public class Error
    {
        private string _name = "";
        private string _value = "";
        private int _row = 0;
        private string _dataType = "";
        private string _issue = "";
        private ErrorType _errorType;

        public Error(ErrorType errorType, string issue)
        {
            _issue = issue;
        }

        public Error(ErrorType errorType, string issue, string name)
        {
            _name = name;
            _issue = issue;
        }

        /// <summary>
        /// This is a test
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="issue"></param>
        /// <param name="valueList">0 = name, 1 = value, 2 = row, 3 = datatype
        /// </param>
        /// 
        public Error(ErrorType errorType ,string issue, params object[] valueList)
        {
            _errorType = errorType;
            _issue = issue;
            if (valueList[0] != null) _name = valueList[0].ToString();
            if (valueList[1] != null) _value = valueList[1].ToString();
            if (valueList[2] != null) _row = Convert.ToInt32(valueList[2]);
            if (valueList[3] != null) _dataType = valueList[3].ToString();
        }

        public override string ToString()
        {
            switch(_errorType)
            {
                case ErrorType.Value: return String.Format("{0} : Variable : {1} , Value : {2}, in Row : {3}, DataType : {4}", _issue, _name, _value, _row.ToString(), _dataType);
                case ErrorType.Dataset: return String.Format("{0} : {1}", _issue, _name);
                case ErrorType.Datastructure: return String.Format("{0} : {1}", _issue, _name);
                default: return String.Format("{0}", _issue);
            }
        }

        
    }

    public enum ErrorType
    {
        Dataset,
        Datastructure,
        Value,
        Other
    }
}
