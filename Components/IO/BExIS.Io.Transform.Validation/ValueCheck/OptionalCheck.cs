using BExIS.IO.Transform.Validation.Exceptions;
using System;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation.ValueCheck
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class OptionalCheck : IValueCheck
    {
        #region private

        private ValueType appliedTo = new ValueType();
        private bool optional = false;
        private string name = "";
        private string dataType = "";

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

        #endregion get

        #endregion private

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
            if (String.IsNullOrEmpty(value) && this.optional == false)
            {
                // create Error Object
                Error e = new Error(ErrorType.Value, "Is empty and not optional ", new object[] { name, "empty", row, dataType });
                return e;
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
        /// <param name="optional"></param>
        public OptionalCheck(string name, string dataType, bool optional)
        {
            this.appliedTo = ValueType.All;
            this.optional = optional;
            this.name = name;
            this.dataType = dataType;
        }
    }
}