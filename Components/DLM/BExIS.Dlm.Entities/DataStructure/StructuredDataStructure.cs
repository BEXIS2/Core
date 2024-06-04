using BExIS.Dlm.Entities.Data;
using System.Collections.Generic;
using System.Xml;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class StructuredDataStructure : DataStructure
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DataStructureCategory IndexerType { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> StructuredDataStructure is the controller of this association </remarks>
        /// <seealso cref=""/>
        public virtual ICollection<VariableInstance> Variables { get; set; }

        /*
        This relationship is used in respective to Data Structure Category.
        For time series it determines the time variable, for Coverage it determines the Location variable and for samplings it determines which variable will hold the information about the samples.
        In cases that time or space coverage is clear by other variables or parameters or is not important, it's possible to not introduce an Indexer.
         */

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Variable Indexer { get; set; } // 0..1,  the data type of the target data attribute must be compatible with indexer type, see CM Structured Data and its associated relevant classes.

        /// <summary>
        /// Is the full path to the resources linked to the structure, mainly its templates.
        /// It contains a collection of Resource elements each follow this pattern: <Resources><Reource Type="Excel, CSV, ..." Edition="2010, 2, ..." Path="full path/ url to the file"></Resource> </Resources>
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual XmlNode TemplatePaths { get; set; }

        // relationship to Metadata Structure
        // Relationship to View
        // Relationship to Workflow?

        #endregion Attributes

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public StructuredDataStructure() : this(DataStructureCategory.Generic)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="indexerType"></param>
        public StructuredDataStructure(DataStructureCategory indexerType = DataStructureCategory.Generic)
        {
            //sample: VariableUsages.First().DataAttribute.ParameterUsages.First().Parameter.
            Datasets = new List<Dataset>();
            Variables = new List<VariableInstance>();
            IndexerType = indexerType;
        }

        //public override void Validate()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion Methods
    }
}