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
        public virtual ICollection<StructuredDataVariableUsage> VariableUsages { get; set; } // StructuredDataStructure is the controller of this association
        public virtual Parameter Indexer { get; set; } // 0..1,  the data type of the parameter must be compatible with Category, see CM Staructured Data and its associated relevent classes.

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
            //sample: VariableUsages.First().Variable.ParameterUsages.First().Parameter.
            Datasets = new List<Dataset>();
            VariableUsages = new List<StructuredDataVariableUsage>();
            IndexerType = indexerType;
        }

        public override void Validate()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
