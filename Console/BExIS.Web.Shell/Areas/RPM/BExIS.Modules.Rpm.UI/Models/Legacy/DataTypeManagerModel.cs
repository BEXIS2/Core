﻿using System.Linq;
using System.Web;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using BExIS.IO.DataType.DisplayPattern;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class DataTypeModel
    {
        public Dlm.Entities.DataStructure.DataType dataType { get; set; }
        public DataTypeDisplayPattern pattern { get; set; }

    public DataTypeModel()
        {
            dataType = new Dlm.Entities.DataStructure.DataType();
            pattern = null;
        }
        public DataTypeModel(long Id)
        {
            DataTypeManager dtm = null;
            try
            {
                dtm = new DataTypeManager();
                dataType = dtm.Repo.Get(Id);
                pattern = DataTypeDisplayPattern.Materialize(dataType.Extra);
            }
            finally
            {
                dtm.Dispose();
            }
        }

    }
}