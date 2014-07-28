using System;
using BExIS.Io.Transform.Validation.Exceptions;

/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Validation.ValueCheck
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class OptionalCheck:IValueCheck
    {
        # region parameter

            private ValueType _appliedTo = new ValueType();
            private bool _optional = false;
            private string _name = "";
            private string _dataType = "";

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
            if (String.IsNullOrEmpty(value) && this._optional == false)
            {
                // create Error Object
                Error e = new Error(ErrorType.Value, "Is empty and not optional ", new object[] { _name, "empty", row, _dataType });
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
            _appliedTo = ValueType.All;
            _optional = optional;
            _name = name;
            _dataType = dataType;

        }

    }
}
