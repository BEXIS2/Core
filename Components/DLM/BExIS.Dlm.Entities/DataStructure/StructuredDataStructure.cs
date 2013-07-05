using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Dlm.Entities.DataStructure
{   
    public class StructuredDataStructure: DataStructure
    {

        #region Methods

        public virtual DataStructureCategory IndexerType { get; set; }
        public virtual ICollection<VariableUsage> VariableUsages { get; set; } // StructuredDataStructure is the controller of this association
        /*
        This relationship is used in respective to Data Structure Category.
        For time series it determines the time variable, for Coverage it determines the Location variable and for samplings it determines which variable will hold the information about the samples.
        In cases that time or space coverage is clear by other variables or parameters or is not important, it's possible to not introduce an Indexer.
         */
        public virtual VariableUsage Indexer { get; set; } // 0..1,  the data type of the target data attribute must be compatible with indexer type, see CM Staructured Data and its associated relevent classes.

        // relationdhip to Metadata Structure
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
            VariableUsages = new List<VariableUsage>();
            IndexerType = indexerType;
        }

        public override void Validate()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
