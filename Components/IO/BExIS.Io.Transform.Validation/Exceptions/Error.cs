﻿using System;

/// <summary>
///
/// </summary>        
namespace BExIS.IO.Transform.Validation.Exceptions
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class Error
    {
        private string _name = "";
        private string _value = "";
        private int _row = 0;

        //MetadataAttributeNumber
        private int _number = 0;
        //MetadataPAckageNumber
        private string _package = "";
        //MetadataPAckageLabel
        private string _packageLabel = "";

        private string _dataType = "";
        private string _datePattern = "";
        private string _issue = "";
        private ErrorType _errorType;

        public String getName()
        {
            return this._name;
        }

        public String getValue()
        {
            return this._value;
        }

        public int getRow()
        {
            return this._row;
        }

        public String getDataType()
        {
            return this._dataType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="errorType"></param>
        /// <param name="issue"></param>
        public Error(ErrorType errorType, string issue)
        {
            _issue = issue;
            _errorType = errorType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="errorType"></param>
        /// <param name="issue"></param>
        /// <param name="name"></param>
        public Error(ErrorType errorType, string issue, string name)
        {
            _name = name;
            _issue = issue;
        }

        /// <summary>
        /// This is a test
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="errorType"></param>
        /// <param name="issue"></param>
        /// <param name="valueList">Value :(0 = name, 1 = value, 2 = row, 3 = datatype)
        /// MetadataAttribute: (0=name, 1=value, 2=attributeNumber, 3= packageNumber)
        /// </param>
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
                if (valueList.Length > 4 && valueList[4] != null) _datePattern = valueList[4].ToString();
            }

            if (errorType.Equals(ErrorType.MetadataAttribute))
            {
                if (valueList[0] != null) _name = valueList[0].ToString();
                if (valueList[1] == null) _value = "null";
                else _value = valueList[1].ToString();
                if (valueList[2] != null) _number = Convert.ToInt32(valueList[2].ToString());
                if (valueList[3] != null) _package = valueList[3].ToString();
                if (valueList[4] != null) _packageLabel = valueList[4].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public string GetMessage()
        {
            return _issue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public override string ToString()
        {
            switch(_errorType)
            {
                case ErrorType.Value: return String.Format("{0} : Variable : {1} , Value : {2}, in Row : {3}, DataType : {4} {5}", _issue, _name, _value, _row.ToString(), _dataType, _datePattern);
                case ErrorType.Dataset: return String.Format("{0} ({1})", _issue, _name);
                case ErrorType.Datastructure: return String.Format("{0} ({1})", _issue, _name);
                case ErrorType.MetadataAttribute: return String.Format("(Attribute number {3} name = <b>{0}</b> in {5} with value = {1} ) : {2} in {5} Number {4}", _name, _value, _issue , _number, _package,_packageLabel);
                default: return String.Format("{0}", _issue);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public string ToHtmlString()
        {
            switch (_errorType)
            {
                case ErrorType.Value: return String.Format("{0} : Variable : {1} , Value : {2}, in Row : {3}, DataType : {4}", _issue, _name, _value, _row.ToString(), _dataType);
                case ErrorType.Dataset: return String.Format("{0} : {1}", _issue, _name);
                case ErrorType.Datastructure: return String.Format("{0} : {1}", _issue, _name);
                case ErrorType.MetadataAttribute:{

                    if (String.IsNullOrEmpty(_value))
                        return String.Format("in Package : <b>{5} ({4})</b><br> Attribute : <b>{0} ({3})</b> <br> with value = <b>{1}</b><br>{2} <br>  <hr>", _name, _value, _issue, _number, _package, _packageLabel);
                    else
                        return String.Format("in Package : <b>{5} ({4})</b><br> Attribute : <b>{0} ({3})</b> <br>{2} <br>  <hr>", _name, _value, _issue, _number, _package, _packageLabel);

                } 
                default: return String.Format("{0}", _issue);
            }
        }
      
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ErrorType
    {
        Dataset,
        Datastructure,
        Value,
        MetadataAttribute,
        Other
    }
}
