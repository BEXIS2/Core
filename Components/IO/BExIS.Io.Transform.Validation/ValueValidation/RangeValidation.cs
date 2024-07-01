using BExIS.IO.Transform.Validation.Exceptions;
using System;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation.ValueValidation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class RangeValidation : IValueValidation
    {
        # region private

        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";
        private double min = 0;
        private double max = 0;

        #region get

        public ValueType AppliedTo
        {
            get
            {
                return appliedTo;
            }
        }

        public string Name
        {
            get { return name; }
        }

        public string DataType
        {
            get { return dataType; }
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
        public Error Execute(object value, int row)
        {
            if (value != null && value.ToString() != "")
            {
                if (dataType.Equals(TypeCode.Int16.ToString()) ||
                   dataType.Equals(TypeCode.Int32.ToString()))
                {
                    int tempValue = Convert.ToInt32(value);
                    if (tempValue < min || tempValue > max) return new Error(ErrorType.Value, "Not in Range", new object[] { name, value, row, dataType });
                }

                if (dataType.Equals(TypeCode.Double.ToString()))
                {
                    double tempValue = Convert.ToDouble(value);

                    if (tempValue < min || tempValue > max) return new Error(ErrorType.Value, "Not in Range", new object[] { name, value, row, dataType });
                }

                if (dataType.Equals(TypeCode.DateTime.ToString()))
                {
                    DateTime dt = DateTime.Parse(value.ToString());
                    double tempValue = dt.ToOADate();

                    if (tempValue < min || tempValue > max) return new Error(ErrorType.Value, "Not in Range", new object[] { name, value, row, dataType });
                }
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public RangeValidation(string name, string dataType, double min, double max)
        {
            this.appliedTo = ValueType.All;
            this.min = min;
            this.max = max;
            this.name = name;
            this.dataType = dataType;
        }
    }
}