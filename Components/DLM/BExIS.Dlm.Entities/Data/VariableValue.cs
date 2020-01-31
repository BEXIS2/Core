﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using BExIS.Dlm.Entities.DataStructure;
using Newtonsoft.Json;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.Data
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class VariableValue: DataValue
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>   
        [JsonIgnore]
        [XmlIgnore]
        public DataTuple Tuple { get; set; } // reference to the containing tuple

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>     
        [JsonProperty("vid")]
        public Int64 VariableId { get; set; } // when variable is not loaded!

        #endregion

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        [XmlIgnore]
        [JsonIgnore]

        public IList<ParameterValue> ParameterValues { get; set; }


        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>    
        [XmlIgnore]
        [JsonIgnore]

        public DataAttribute DataAttribute
        {
            get
            {
                if (this.Tuple.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    return (this.Variable.DataAttribute);
                }
                return (null);
            }

        }


        private Variable _variable;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        [XmlIgnore]
        [JsonIgnore]

        public Variable Variable
        {
            get
            {
                if (this.Tuple.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    Variable u = (this.Tuple.DatasetVersion.Dataset.DataStructure.Self as StructuredDataStructure).Variables
                        .Where(p => p.Id.Equals(this.VariableId))
                        .Select(p => p).FirstOrDefault();
                    return (u);
                }

                return (null);
            }
        }

        #endregion

        #region Mathods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public VariableValue()
        {
            ParameterValues = new List<ParameterValue>();
        }

        #endregion

    }
}
