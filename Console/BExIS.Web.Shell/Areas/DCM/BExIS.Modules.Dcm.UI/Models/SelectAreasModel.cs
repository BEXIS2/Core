﻿using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class SelectAreasModel
    {
        public StepInfo StepInfo { get; set; }
        public List<string> DataArea { get; set; }
        public string HeaderArea { get; set; }
        public Dictionary<Uri, String> SheetUriDictionary { get; set; }
        public string activeSheetUri { get; set; }

        public List<Error> ErrorList { get; set; }

        public SelectAreasModel()
        {
            ErrorList = new List<Error>();
        }
    }
}