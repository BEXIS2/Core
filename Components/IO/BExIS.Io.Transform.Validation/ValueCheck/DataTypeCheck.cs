using System;
using BExIS.Io.Transform.Validation.Exceptions;


namespace BExIS.Io.Transform.Validation.ValueCheck
{
    public class DataTypeCheck:IValueCheck
    {
        # region parameter
        private ValueType _appliedTo = new ValueType();
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
                            try
                            {

                                if (!String.IsNullOrEmpty(value)) Convert.ToDouble(value);
                            }
                            catch (Exception ex)
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });
                            }

                            return Convert.ToDouble(value);
                        }
                    case "DateTime": 
                        {
                            try
                            {
                                DateTime.Parse(value);
                            }
                            catch
                            {
                                return new Error(ErrorType.Value, "Can not convert to", new object[] { _name, value, row, _dataType });
                            }

                            return DateTime.Parse(value);
                        }
                    case "String":
                        {
                            return value;
                        }

                }
            }
            return value;
        }

        public DataTypeCheck(string name, string dataType)
        {
            _appliedTo = ValueType.Number;
            _name = name;
            _dataType = dataType;
        }
    }
}
