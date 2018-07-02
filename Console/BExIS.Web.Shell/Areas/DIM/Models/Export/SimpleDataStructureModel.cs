﻿using BExIS.Dlm.Entities.DataStructure;
using BExIS.Modules.Dim.UI.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Export
{
    public class SimpleDataStructureModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DataTable Data { get; set; }

        public SimpleDataStructureModel(StructuredDataStructure sds)
        {
            UIHelper helper = new UIHelper();

            Id = sds.Id;
            Name = sds.Name;
            Data = helper.ConvertStructuredDataStructureToDataTable(sds);
        }
    }
}