using System;
using System.Globalization;
using System.Text.RegularExpressions;
using BExIS.Io.Transform.Validation.Exceptions;
using System.Linq;

/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Validation.ValueCheck
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class DataTypeCheck:IValueCheck
    {
        # region parameter
        private ValueType _appliedTo = new ValueType();
        private string _name = "";
        private string _dataType = "";
        private DecimalCharacter _decimalCharacter;

        #region get

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            public ValueType AppliedTo
            {
                get
                {
                    return _appliedTo;
                }
            }

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            public string Name
            {
                get { return _name; }
            }

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            public string DataType
            {
                get { return _dataType; }
            }

            public DecimalCharacter GetDecimalCharacter
            {
                get { return _decimalCharacter; }
            }
            

            #endregion

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public object Execute(string value, int row)
        {
            if (!String.IsNullOrEmpty(value))
            {
                switch (_dataType)
                {
                    case "Int16":
                        {
                            try
                            {
                                if (!String.IsNullOrEmpty(value)) Convert.ToInt16(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });
                            }

                            return Convert.ToInt16(value);
                        }

                    case "Int32":
                        {
                            try
                            {
                                if (!String.IsNullOrEmpty(value)) Convert.ToInt32(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });
                            }

                            return Convert.ToInt32(value);
                        }

                    case "Double":
                        {
                            //double convertedValue = 0;

                            //if(double.TryParse(value,NumberStyles.Number,CultureInfo.InvariantCulture, out convertedValue))
                            //{
                                if (_decimalCharacter.Equals(DecimalCharacter.point))
                                {
                                    string[] temp = value.Split('.');
                                    if (temp.Length <= 2)
                                    {
                                        if (temp[temp.Length - 1].All(Char.IsDigit))
                                        {
                                            return Convert.ToDouble(value, new CultureInfo("en-US"));
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { _name, value, row, _dataType });
                                        }
                                    }
                                    else
                                    {
                                        return new Error(ErrorType.Value, "Can not convert to.", new object[] { _name, value, row, _dataType });
                                    } 
                                }

                                if (_decimalCharacter.Equals(DecimalCharacter.comma))
                                {
                                    string[] temp = value.Split(',');
                                    if (temp.Length <= 2)
                                    {
                                        if (temp[temp.Length - 1].All(Char.IsDigit))
                                        {
                                            return Convert.ToDouble(value, new CultureInfo("de-DE"));
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { _name, value, row, _dataType });
                                        }

                                    }
                                    else
                                    {
                                        return new Error(ErrorType.Value, "Can not convert to.", new object[] { _name, value, row, _dataType });
                                    }
                                }

                            //}
                            //else
                            //{
                            //    return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });
                            //}

                            return Convert.ToDouble(value);
                        }
                    case "DateTime": 
                        {

                            DateTime dateTime;

                            if(DateTime.TryParse(value,out dateTime))
                            {
                                return dateTime;
                            }

                            if(DateTime.TryParse(value,new CultureInfo("de-DE", false),DateTimeStyles.None,out dateTime))
                            {
                                return dateTime;
                            }

                            if(DateTime.TryParse(value,new CultureInfo("en-US", false),DateTimeStyles.None,out dateTime))
                            {
                                return dateTime;
                            }

                            if (DateTime.TryParse(value,CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            {
                                return dateTime;
                            }

                            return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });

                        }
                    case "String":
                        {
                            return value;
                        }

                }
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        public DataTypeCheck(string name, string dataType, DecimalCharacter decimalCharacter)
        {
            _appliedTo = ValueType.Number;
            _name = name;
            _dataType = dataType;
            _decimalCharacter = decimalCharacter;
        }
    }
}
