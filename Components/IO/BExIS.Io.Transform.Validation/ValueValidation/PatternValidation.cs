using BExIS.IO.Transform.Validation.Exceptions;
using System.Text.RegularExpressions;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation.ValueValidation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class PatternValidation : IValueValidation
    {
        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";
        private string pattern = "";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public ValueType AppliedTo
        {
            get { return appliedTo; }
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Pattern
        {
            get { return pattern; }
        }

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
            if (value != null)
            {
                Regex rx = new Regex(pattern);

                if (rx.IsMatch(value.ToString()))
                {
                    return null;
                }
                else return new Error(ErrorType.Value, "Value not matched on variable pattern.", new object[] { name, value, row, dataType });
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
        /// <param name="pattern"></param>
        public PatternValidation(string name, string dataType, string pattern)
        {
            this.appliedTo = ValueType.All;
            this.name = name;
            this.dataType = dataType;
            this.pattern = pattern;
        }
    }
}