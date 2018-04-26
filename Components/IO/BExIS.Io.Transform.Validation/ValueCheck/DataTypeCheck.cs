using System;
using System.Globalization;
using System.Text.RegularExpressions;
using BExIS.IO.Transform.Validation.Exceptions;
using System.Linq;

/// <summary>
///
/// </summary>        
namespace BExIS.IO.Transform.Validation.ValueCheck
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class DataTypeCheck:IValueCheck
    {
        # region private

        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";
        private DecimalCharacter decimalCharacter;
        private string pattern;

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
                    return appliedTo;
                }
            }

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            public string DataType
            {
                get { return dataType; }
            }

            public DecimalCharacter GetDecimalCharacter
            {
                get { return decimalCharacter; }
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
                switch (dataType)
                {
                    case "Int16":
                        {
                            try
                            {
                                if (!String.IsNullOrEmpty(value)) Convert.ToInt16(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
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
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }

                            return Convert.ToInt32(value);
                        }

                    case "Int64":
                        {
                            try
                            {
                                if (!String.IsNullOrEmpty(value)) Convert.ToInt64(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }

                            return Convert.ToInt64(value);
                        }

                    case "Double":
                        {
                            //double convertedValue = 0;

                            //if(double.TryParse(value,NumberStyles.Number,CultureInfo.InvariantCulture, out convertedValue))
                            //{

                            //Try to figure out the structure and then parse as double - return Error if structure doesn't fit or parsing fails
                            try
                            {
                                if (decimalCharacter.Equals(DecimalCharacter.point))
                                {
                                    string[] temp = value.Split('.');
                                    if (temp.Length <= 2)
                                    {
                                        if (!temp[temp.Length - 1].Contains(','))
                                        {
                                            return Convert.ToDouble(value, new CultureInfo("en-US"));
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }
                                    }
                                    else
                                    {
                                        return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                                    }
                                }

                                if (decimalCharacter.Equals(DecimalCharacter.comma))
                                {
                                    string[] temp = value.Split(',');
                                    if (temp.Length <= 2)
                                    {
                                        if (!temp[temp.Length - 1].Contains('.'))
                                        {
                                            return Convert.ToDouble(value, new CultureInfo("de-DE"));
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }

                                    }
                                    else
                                    {
                                        return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                                    }
                                }
                                return Convert.ToDouble(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                            }
                               
                            //}
                            //else
                            //{
                            //    return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });
                            //}
                        }

                    case "Decimal":
                        {
                            /*
                             * Same idea as for double but for decimal you have to explicitly allow 
                             * scientific notation with the flags NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint
                             **/
                            try
                            {
                                if (decimalCharacter.Equals(DecimalCharacter.point))
                                {
                                    string[] temp = value.Split('.');
                                    if (temp.Length <= 2)
                                    {
                                        if (!temp[temp.Length - 1].Contains(','))
                                        {
                                            return Decimal.Parse(value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"));
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }
                                    }
                                    else
                                    {
                                        return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                                    }
                                }

                                if (decimalCharacter.Equals(DecimalCharacter.comma))
                                {
                                    string[] temp = value.Split(',');
                                    if (temp.Length <= 2)
                                    {
                                        if (!temp[temp.Length - 1].Contains('.'))
                                        {
                                            return Decimal.Parse(value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, new CultureInfo("de-DE"));
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }

                                    }
                                    else
                                    {
                                        return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                                    }
                                }
                                return Decimal.Parse(value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                            }
                        }

                    case "DateTime": 
                        {
                            DateTime dateTime;
                            if (IOUtility.IsDate(value, pattern, out dateTime))
                            {
                                return dateTime;
                            }

                            return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });

                        }

                    case "Char":
                        {
                            char converted;
                            if (Char.TryParse(value, out converted))
                            {
                                return converted;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }

                    case "String":
                        {
                            return value;
                        }
                        
                    //TODO Boolean check
                    case "Boolean":
                        {
                            //Accept 0 and 1
                            if(value == "0")
                            {
                                return false;
                            }
                            else if(value == "1")
                            {
                                return true;
                            }
                            else
                            {
                                //Try to parse, e.g. "true", "True" or "TRUE"
                                Boolean converted;
                                if (Boolean.TryParse(value, out converted))
                                {
                                    return converted;
                                }
                                else
                                {
                                    return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                                }
                            }
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
        /// <param name="pattern"></param>
        public DataTypeCheck(string name, string dataType, DecimalCharacter decimalCharacter, string pattern="")
        {
            this.appliedTo = ValueType.Number;
            this.name = name;
            this.dataType = dataType;
            this.decimalCharacter = decimalCharacter;
            this.pattern = pattern;
        }
    }
}
