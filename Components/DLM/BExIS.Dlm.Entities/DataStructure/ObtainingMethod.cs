using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    /// Name and descroption, and etc. of methods of obtaining data
    /// like: Measurement, Observation, Processing, Simulation, Interpretation and Description
    /// </summary>
    public class ObtainingMethod: BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
