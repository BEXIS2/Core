using System;

namespace BExIS.Io.Transform.Validation.Exceptions
{
    public class Error
    {
        private string _name = "";
        private string _value = "";
        private int _row = 0;

        //MetadataAttributeNumber
        private int _number = 0;
        //MetadataPAckageNumber
        private string _package = "";

        private string _dataType = "";
        private string _issue = "";
        private ErrorType _errorType;

        public Error(ErrorType errorType, string issue)
        {
            _issue = issue;
            _errorType = errorType;
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
        /// <param name="valueList">Value :(0 = name, 1 = value, 2 = row, 3 = datatype)
        /// MetadataAttribute: (0=name, 1=value, 2=attributeNumber, 3= packageNumber)
        /// </param>
        /// 
        public Error(ErrorType errorType ,string issue, params object[] valueList)
        {
            _errorType = errorType;
            _issue = issue;

            if (errorType.Equals(ErrorType.Value))
            {
                if (valueList[0] != null) _name = valueList[0].ToString();
                if (valueList[1] != null) _value = valueList[1].ToString();
                if (valueList[2] != null) _row = Convert.ToInt32(valueList[2]);
                if (valueList[3] != null) _dataType = valueList[3].ToString();
            }

            if (errorType.Equals(ErrorType.MetadataAttribute))
            {
                if (valueList[0] != null) _name = valueList[0].ToString();
                if (valueList[1] != null) _value = valueList[1].ToString();
                if (valueList[2] != null) _number = Convert.ToInt32(valueList[2].ToString());
                if (valueList[3] != null) _package = valueList[3].ToString();
            }
        }

        public override string ToString()
        {
            switch(_errorType)
            {
                case ErrorType.Value: return String.Format("{0} : Variable : {1} , Value : {2}, in Row : {3}, DataType : {4}", _issue, _name, _value, _row.ToString(), _dataType);
                case ErrorType.Dataset: return String.Format("{0} : {1}", _issue, _name);
                case ErrorType.Datastructure: return String.Format("{0} : {1}", _issue, _name);
                case ErrorType.MetadataAttribute: return String.Format("(Attribute number {3} name = {0} in {4} with value = {1} ) : {2}", _name, _value, _issue , _number, _package);
                default: return String.Format("{0}", _issue);
            }
        }

        
    }

    public enum ErrorType
    {
        Dataset,
        Datastructure,
        Value,
        MetadataAttribute,
        Other
    }
}
