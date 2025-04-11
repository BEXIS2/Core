using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Globalization;
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
    public class DataTypeCheck : IValueCheck
    {
        # region private

        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";
        private DecimalCharacter decimalCharacter;
        private string pattern;
        private CultureInfo culture;
        public IOUtility IOUtility = new IOUtility();

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

        #endregion get

        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public object Execute(string value, int row=0)
        {
            if (!String.IsNullOrEmpty(value))
            {
                switch (dataType)
                {
                    case "Int16":
                        {
                            short i = 0;

                            if (Int16.TryParse(value, out i))
                            {
                                return i;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }

                    case "Int32":
                        {
                            int i = 0;

                            if (Int32.TryParse(value, out i))
                            {
                                return i;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }

                    case "Int64":
                        {
                            long i = 0;

                            if (Int64.TryParse(value, out i))
                            {
                                return i;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }
                    case "UInt16":
                        {
                            UInt16 i = 0;

                            if (UInt16.TryParse(value, out i))
                            {
                                return i;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }
                    case "UInt32":
                        {
                            UInt32 i = 0;

                            if (UInt32.TryParse(value, out i))
                            {
                                return i;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }
                    case "UInt64":
                        {
                            UInt64 i = 0;

                            if (UInt64.TryParse(value, out i))
                            {
                                return i;
                            }
                            else
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType });
                            }
                        }
                    case "Double":
                        {
                            //Try to figure out the structure and then parse as double - return Error if structure doesn't fit or parsing fails
                            try
                            {
                                if (decimalCharacter.Equals(DecimalCharacter.point))
                                {
                                    // when point is a decimal character then it should only exist once or non
                                    string[] temp = value.Split('.');
                                    if (temp.Length <= 2)
                                    {
                                        // check if comma exist as a seperator
                                        if (!temp[temp.Length - 1].Contains(','))
                                        {
                                            //check lenght of the string and compare to the max storage of the datatype
                                            if (value.Length > 15)
                                                return new Error(ErrorType.Value, "The value of the number is outside the value range of double. Change it do decimal.", new object[] { name, value, row, dataType });

                                            double d = 0;

                                            if (double.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out d))
                                            {
                                                return d;
                                            }
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }
                                    }

                                    return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                                }

                                if (decimalCharacter.Equals(DecimalCharacter.comma))
                                {
                                    string[] temp = value.Split(',');
                                    if (temp.Length <= 2)
                                    {
                                        if (!temp[temp.Length - 1].Contains('.'))
                                        {
                                            //chek lenght of the string and compare to the max storage of the datatype
                                            if (value.Length > 15)
                                                return new Error(ErrorType.Value, "the value of the number is outside the value range of double. Change it do decimal.", new object[] { name, value, row, dataType });

                                            double d = 0;
                                            if (double.TryParse(value, NumberStyles.Any, new CultureInfo("de-DE"), out d))
                                            {
                                                return d;
                                            }
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }
                                    }

                                    return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                                }

                                return Convert.ToDouble(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
                            }
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
                                            //chek lenght of the string and compare to the max storage of the datatype
                                            if (value.Length > 28)
                                                return new Error(ErrorType.Value, "the value of the number is outside the value range of decimal.", new object[] { name, value, row, dataType });

                                            //convert to decimal and compare both string lenghts
                                            return Decimal.Parse(value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.Float, new CultureInfo("en-US"));
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
                                            //chek lenght of the string and compare to the max storage of the datatype
                                            if (value.Length > 28)
                                                return new Error(ErrorType.Value, "the value of the number is outside the value range of decimal.", new object[] { name, value, row, dataType });
                                            decimal d = 0;
                                            //convert to decimal and compare both string lenghts
                                            if (Decimal.TryParse(value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, new CultureInfo("de-DE"), out d))
                                            {
                                                return d;
                                            }
                                        }
                                        else
                                        {
                                            return new Error(ErrorType.Value, "False decimal character.", new object[] { name, value, row, dataType });
                                        }
                                    }

                                    return new Error(ErrorType.Value, "Can not convert to.", new object[] { name, value, row, dataType });
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

                            if (!string.IsNullOrEmpty(pattern))
                            {
                                if (IOUtility.ConvertToDate(value, pattern, out dateTime, culture))
                                {
                                    return dateTime;
                                }
                            }
                            else
                            {
                                if (IOUtility.TryConvertDate(value, out dateTime))
                                {
                                    return dateTime;
                                }
                            }
                            if (!string.IsNullOrEmpty(pattern))
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { name, value, row, dataType, pattern });
                            else
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
                            if (value == "0")
                            {
                                return false;
                            }
                            else if (value == "1")
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
        public DataTypeCheck(string name, string dataType, DecimalCharacter decimalCharacter, string pattern = "", CultureInfo cultureInfo = null)
        {
            this.appliedTo = ValueType.Number;
            this.name = name;
            this.dataType = dataType;
            this.decimalCharacter = decimalCharacter;
            this.pattern = pattern;
            this.culture = cultureInfo;
        }
    }
}