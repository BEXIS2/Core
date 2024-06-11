/// <summary>
///
/// </summary>
using BExIS.Dlm.Entities.Data;
using System.Collections.Generic;

namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class UnStructuredDataStructure : DataStructure
    {
        public UnStructuredDataStructure()
        {
            //sample: VariableUsages.First().DataAttribute.ParameterUsages.First().Parameter.
            Datasets = new List<Dataset>();
        }

        //public override void Validate()
        //{
        //    // at least one ContentDescriptor
        //    //throw new NotImplementedException();
        //}
    }
}