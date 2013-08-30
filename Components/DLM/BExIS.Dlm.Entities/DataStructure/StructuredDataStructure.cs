using System.Collections.Generic;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Dlm.Entities.DataStructure
{   
    public class StructuredDataStructure: DataStructure
    {

        #region Attributes

        public virtual DataStructureCategory IndexerType { get; set; }
        public virtual ICollection<Variable> Variables { get; set; } // StructuredDataStructure is the controller of this association
        /*
        This relationship is used in respective to Data Structure Category.
        For time series it determines the time variable, for Coverage it determines the Location variable and for samplings it determines which variable will hold the information about the samples.
        In cases that time or space coverage is clear by other variables or parameters or is not important, it's possible to not introduce an Indexer.
         */
        public virtual Variable Indexer { get; set; } // 0..1,  the data type of the target data attribute must be compatible with indexer type, see CM Structured Data and its associated relevant classes.

        // relationship to Metadata Structure
        // Relationship to View
        // Relationship to Workflow?
        #endregion

        #region Methods

        public StructuredDataStructure(): this(DataStructureCategory.Generic)
        {
        }

        public StructuredDataStructure(DataStructureCategory indexerType = DataStructureCategory.Generic)
        {
            //sample: VariableUsages.First().DataAttribute.ParameterUsages.First().Parameter.
            Datasets = new List<Dataset>();
            Variables = new List<Variable>();
            IndexerType = indexerType;
        }

        //public override void Validate()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion
    }
}
