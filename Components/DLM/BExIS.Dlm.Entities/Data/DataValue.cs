using BExIS.Dlm.Entities.DataStructure;
using Newtonsoft.Json;
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
    public abstract class DataValue //: BaseEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [JsonProperty("v")]
        public object Value { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [XmlIgnoreAttribute]
        [JsonIgnore]
        public DateTime SamplingTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [XmlIgnoreAttribute]
        [JsonIgnore]
        public DateTime ResultTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [XmlIgnoreAttribute]
        [JsonIgnore]
        public ObtainingMethod ObtainingMethod { get; set; }

        /// <summary>
        /// any free note. especially in case of ObtainingMethod == Processing or Simulation, the process, formula or simulation model can be described here
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        [XmlIgnoreAttribute]
        [JsonIgnore]
        public string Note { get; set; }

        #endregion Attributes
    }
}