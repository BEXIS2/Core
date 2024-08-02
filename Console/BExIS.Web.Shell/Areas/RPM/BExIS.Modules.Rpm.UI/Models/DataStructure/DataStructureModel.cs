using System;
using System.Collections.Generic;

namespace BExIS.Modules.Rpm.UI.Models.DataStructure
{
    public class DataStructureModel
    {
        public long Id { get; set; }

        /// <summary>
        /// title of the data structure
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// description of the data structure
        /// </summary>
        public string Description { get; set; }

        public List<long> LinkedTo { get; set; }

        public DataStructureModel()
        {
            Id = 0;
            Title = String.Empty;
            Description = String.Empty;
            LinkedTo = new List<long>();
        }
    }
}