using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Entities.Data
{
    public abstract class DataValue //: BaseEntity
    {
        #region Attributes

        public object Value { get; set; }
        public DateTime SamplingTime { get; set; }
        public DateTime ResultTime { get; set; }
        public ObtainingMethod ObtainingMethod { get; set; }
        public string Note { get; set; } // any free note. especially in case of ObtainingMethod == Processing or Simulation, the process, formula or simulation model can be described here

        #endregion

        #region Associations

        #endregion

        #region Methods

        #endregion

    }
}
