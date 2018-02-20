using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Sam.UI.Models
{
    public struct DatasetStatModel
    {
        public long Id { get; set; }
        public DatasetStatus Status { get; set; }
        public long NoOfRows { get; set; }
        public long NoOfCols { get; set; }
        public bool IsSynced { get; set; }
    }
}