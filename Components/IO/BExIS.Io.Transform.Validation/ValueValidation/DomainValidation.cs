using System.Collections.Generic;
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
    public class DomainValidation:IValueValidation
    {
        private ValueType _appliedTo = new ValueType();
        private string _name = "";
        private string _dataType = "";
        private List<string> _checkList = new List<string>();

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
        public List<string> CheckList
        {
            get { return _checkList; }
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
                if(CheckList.Contains(value.ToString())) return null;
                else return new Error(ErrorType.Value,"Value is not in domain", new object[] { _name, value, row, _dataType });
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
        /// <param name="checkList"></param>
        public DomainValidation(string name, string dataType, List<string> checkList)
        {
            _appliedTo = ValueType.All;
            _name = name;
            _dataType = dataType;
            _checkList = checkList;
        }
    }
}
