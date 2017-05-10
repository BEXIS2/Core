using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.DSValidation;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.DCM.Helpers
{
    public class EasyUploadExcelReader: ExcelReader
    {
        protected new List<VariableIdentifier> GetSubmitedVariableIdentifier(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            return this.SubmitedVariableIdentifiers;
        }

        public void setSubmitedVariableIdentifiers( List<VariableIdentifier> vi)
        {
            this.SubmitedVariableIdentifiers = vi;
        }
    }
}