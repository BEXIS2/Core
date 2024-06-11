using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Xml.Serialization;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.Data
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class ParameterValue : DataValue
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Int64 ParameterId { get; set; } // when Parameter is not loaded. It happens when mapping from Xml data is performed after loading object from DB

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [XmlIgnore]
        public VariableValue VariableValue { get; set; } // reference to the containing variable value

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [XmlIgnore]
        public Parameter Parameter
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}