using System.Text.RegularExpressions;
using BExIS.Io.Transform.Validation.Exceptions;

/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Validation.ValueValidation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class PatternValidation:IValueValidation
    {
        private ValueType _appliedTo = new ValueType();
        private string _name = "";
        private string _dataType = "";
        private string _pattern = "";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>             
        public ValueType AppliedTo
        {
            get { return _appliedTo; }
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public string Pattern
        {
            get { return _pattern; }
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
            if (value!=null)
            {
                Regex rx = new Regex(_pattern);

                if (rx.IsMatch(value.ToString()))
                {
                    return null;
                }
                else return new Error(ErrorType.Value, "Value not matched on variable pattern.", new object[] { _name, value, row, _dataType });
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
            _appliedTo = ValueType.All;
            _name = name;
            _dataType = dataType;
            _pattern = pattern;
        }
    
    }
}
