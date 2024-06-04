using BExIS.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation.ValueValidation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class DomainValidation : IValueValidation
    {
        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";
        private List<string> checkList = new List<string>();

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
        public List<string> CheckList
        {
            get { return checkList; }
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
                if (CheckList.Contains(value.ToString())) return null;
                else return new Error(ErrorType.Value, "Value is not in domain", new object[] { name, value, row, dataType });
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
            this.appliedTo = ValueType.All;
            this.name = name;
            this.dataType = dataType;
            this.checkList = checkList;
        }
    }
}