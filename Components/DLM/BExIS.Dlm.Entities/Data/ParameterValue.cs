using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Entities.Data
{
    public class ParameterValue: DataValue
    {
        public Int64 ParameterId { get; set; } // when Parameter is not loaded. It happens when mapping from Xml data is performed after loading object from DB
        public Parameter Parameter { get; set; }

    }
}
