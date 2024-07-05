using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO.DataType.DisplayPattern;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.IO.Transform.Output
{
    public class ExcelHelper
    {
        public static Stylesheet UpdateStylesheet(Stylesheet styleSheet, out List<StyleIndexStruct> styleIndex)
        {
            //start add numberingFormats after 164
            uint iExcelIndex = 164;
            var numberFormats = new NumberingFormats();
            styleIndex = new List<StyleIndexStruct>();

            if (styleSheet.CellFormats == null)
            {
                styleSheet.CellFormats = new CellFormats();
                styleSheet.CellFormats.Count = 0;
            }

            CellFormats cellFormats = styleSheet.Elements<CellFormats>().First();

            //number 0,00
            CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)2U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Decimal", Index = (uint)cellFormats.Count++, DisplayPattern = null });

            //number 0
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)1U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            //styleIndex.Add(new StyleIndexStruct() { Name = "Number", Index = (uint)(uint)cellFormats.ChildElements.Count + 1, DisplayPattern = null });
            styleIndex.Add(new StyleIndexStruct() { Name = "Number", Index = (uint)cellFormats.Count++, DisplayPattern = null });
            //text
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)49U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Text", Index = (uint)cellFormats.Count++, DisplayPattern = null });

            //default Date
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "DateTime", Index = (uint)cellFormats.Count++, DisplayPattern = null });

            //add cellformats from displaypattern
            foreach (var pattern in DataTypeDisplayPattern.Pattern)
            {
                //Excel special cases

                //add numberFormats from displaypattern
                var newNumberFortmat = new NumberingFormat
                {
                    NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                    FormatCode = StringValue.FromString(pattern.ExcelPattern)
                };
                numberFormats.Append(newNumberFortmat);

                //if (pattern.Name.Equals("DateTime")) uInt32Value = 22U;
                //if (pattern.Name.Equals("Date")) uInt32Value = 14U;
                //if (pattern.Name.Equals("Time")) uInt32Value = 21U;
                ///UInt32Value uInt32Value = 0U;

                cellFormat = new CellFormat() { NumberFormatId = newNumberFortmat.NumberFormatId, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
                cellFormat.Protection = new Protection();
                cellFormat.Protection.Locked = false;
                cellFormats.Append(cellFormat);

                DataTypeDisplayPattern tmp = new DataTypeDisplayPattern()
                {
                    Name = pattern.Name,
                    Systemtype = pattern.Systemtype,
                    StringPattern = pattern.ExcelPattern,
                    RegexPattern = pattern.RegexPattern
                };

                styleIndex.Add(new StyleIndexStruct() { Name = pattern.Name, Index = (uint)cellFormats.Count++, DisplayPattern = tmp });
            }

            styleSheet.NumberingFormats = numberFormats;

            return styleSheet;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="systemType"></param>
        /// <param name="styleIndex"></param>
        /// <returns></returns>
        public static uint GetExcelStyleIndex(BExIS.Dlm.Entities.DataStructure.DataType dataType, List<StyleIndexStruct> styleIndex, int displayPatternId)
        {
            if (dataType.SystemType == DataTypeCode.Double.ToString() || dataType.SystemType == DataTypeCode.Decimal.ToString())
                return styleIndex.Where(p => p.Name.Equals("Decimal")).FirstOrDefault().Index;
            if (dataType.SystemType == DataTypeCode.Int16.ToString() || dataType.SystemType == DataTypeCode.Int32.ToString() || dataType.SystemType == DataTypeCode.Int64.ToString() || dataType.SystemType == DataTypeCode.UInt16.ToString() || dataType.SystemType == DataTypeCode.Int32.ToString() || dataType.SystemType == DataTypeCode.Int64.ToString())
                return styleIndex.Where(p => p.Name.Equals("Number")).FirstOrDefault().Index;
            if (dataType.SystemType == DataTypeCode.String.ToString() || dataType.SystemType == DataTypeCode.Char.ToString())
                return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
            if (dataType.SystemType == DataTypeCode.DateTime.ToString())
            {
                if (dataType.Extra != null)
                {
                    DataTypeDisplayPattern pattern = DataTypeDisplayPattern.Get(displayPatternId);
                    if (pattern != null)
                    {
                        StyleIndexStruct tmp = styleIndex.Where(p => p.Name.Equals(pattern.Name)).FirstOrDefault();
                        return tmp.Index;
                    }
                }

                return styleIndex.Where(p => p.Name.Equals("DateTime")).FirstOrDefault().Index;
            }
            if (dataType.SystemType == DataTypeCode.Boolean.ToString())
                return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
            return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
        }

        public static uint GetExcelStyleIndex(string systemType, List<StyleIndexStruct> styleIndex)
        {
            if (systemType == DataTypeCode.Double.ToString() || systemType == DataTypeCode.Decimal.ToString())
                return styleIndex.Where(p => p.Name.Equals("Decimal")).FirstOrDefault().Index;
            if (systemType == DataTypeCode.Int16.ToString() || systemType == DataTypeCode.Int32.ToString() || systemType == DataTypeCode.Int64.ToString() || systemType == DataTypeCode.UInt16.ToString() || systemType == DataTypeCode.Int32.ToString() || systemType == DataTypeCode.Int64.ToString())
                return styleIndex.Where(p => p.Name.Equals("Number")).FirstOrDefault().Index;
            if (systemType == DataTypeCode.String.ToString() || systemType == DataTypeCode.Char.ToString())
                return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
            if (systemType == DataTypeCode.DateTime.ToString())
            {
                return styleIndex.Where(p => p.Name.Equals("DateTime")).FirstOrDefault().Index;
            }
            if (systemType == DataTypeCode.Boolean.ToString())
                return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
            return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
        }
    }
}