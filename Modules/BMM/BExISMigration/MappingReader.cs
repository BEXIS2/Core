using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace BExISMigration
{
    public class MappingReader
    {
        private static Microsoft.Office.Interop.Excel.Application appExcel;
        private static Workbook newWorkbook = null;
        private static _Worksheet objsheet = null;

        // read variables from TSV (tab seperated values) mapping file
        public System.Data.DataTable readVariablesTSV(string filePath, System.Data.DataTable mappedAttributes, System.Data.DataTable mappedUnits)
        {
            string mappingFile = filePath + @"\variableMapping.txt";
            long startRow = 2;

            System.Data.DataTable mappedVariables = new System.Data.DataTable();
            mappedVariables.Columns.Add("DatasetId", typeof(string));
            mappedVariables.Columns.Add("Name", typeof(string));
            mappedVariables.Columns.Add("Description", typeof(string));
            mappedVariables.Columns.Add("Block", typeof(int));
            mappedVariables.Columns.Add("Attribute", typeof(string));
            mappedVariables.Columns.Add("Unit", typeof(string));
            mappedVariables.Columns.Add("ConvFactor", typeof(string));
            mappedVariables.Columns.Add("AttributeId", typeof(long));
            mappedVariables.Columns.Add("UnitId", typeof(long));
            mappedVariables.Columns.Add("VarUnitId", typeof(long));

            System.IO.StreamReader file = new System.IO.StreamReader(mappingFile, System.Text.Encoding.Default);

            int l = 1;
            string readLine;
            while ((readLine = file.ReadLine()) != null)
            {
                if (l >= startRow)
                {
                    String[] readRow = readLine.Split('\t');
                    System.Data.DataRow newRow = mappedVariables.NewRow();
                    newRow["DatasetId"] = readRow[0]; //A
                    newRow["Name"] = readRow[1]; //B
                    newRow["Description"] = readRow[4]; //E
                    string AttributeShortName = readRow[5]; //F
                    newRow["Attribute"] = AttributeShortName;
                    string variableUnit = readRow[6]; //G
                    newRow["Unit"] = variableUnit;
                    newRow["ConvFactor"] = readRow[7]; //H
                    newRow["Block"] = int.Parse(readRow[8]); //I
                    if (AttributeShortName.Length > 1) // if not mapped yet
                    {
                        // select related attribute as row from mappedAttributes Table
                        System.Data.DataRow mappedAttributesRow = mappedAttributes.Select("ShortName = '" + AttributeShortName + "'").First<System.Data.DataRow>();
                        // add related attributeId and unitId to the mappedVariables Table
                        newRow["AttributeId"] = Convert.ToInt64(mappedAttributesRow["AttributeId"]);
                        newRow["UnitId"] = Convert.ToInt64(mappedAttributesRow["UnitId"]);
                    }
                    if (variableUnit.Length > 0) // if dimensioned
                    {
                        // select related unit as row of mappedUnits
                        variableUnit = variableUnit.Normalize(NormalizationForm.FormD);
                        System.Data.DataRow mappedUnitsRow = mappedUnits.Select("Abbreviation = '" + variableUnit + "'").First<System.Data.DataRow>();
                        // add related UnitId of variable Unit to the mappedVariables Table
                        newRow["VarUnitId"] = Convert.ToInt64(mappedUnitsRow["UnitId"]);
                    }
                    mappedVariables.Rows.Add(newRow);
                }
                l++;
            }

            file.Close();

            return mappedVariables;
        }


        // read variables from XLSX mapping file for only one dataset
        public System.Data.DataTable readVariables(string filePath, string dataSetID, System.Data.DataTable mappedAttributes, System.Data.DataTable mappedUnits)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";
            string sheetName = "mapping";
            long startRow = 2;
            long endRow = startRow;

            System.Data.DataTable mappedVariables = new System.Data.DataTable();
            mappedVariables.Columns.Add("DatasetId", typeof(string));
            mappedVariables.Columns.Add("Name", typeof(string));
            mappedVariables.Columns.Add("Description", typeof(string));
            mappedVariables.Columns.Add("Block", typeof(int));
            mappedVariables.Columns.Add("Attribute", typeof(string));
            mappedVariables.Columns.Add("Unit", typeof(string));
            mappedVariables.Columns.Add("ConvFactor", typeof(string));
            mappedVariables.Columns.Add("AttributeId", typeof(long));
            mappedVariables.Columns.Add("UnitId", typeof(long));
            mappedVariables.Columns.Add("VarUnitId", typeof(long));

            excel_init(mappingFile, sheetName, startRow, ref endRow);
            for (long i = startRow; i < endRow; i++)
            {
                System.Data.DataRow newRow = mappedVariables.NewRow();
                if (getValue(i.ToString(), "A") == dataSetID)
                {
                    newRow["DatasetId"] = getValue(i.ToString(), "A");
                    newRow["Name"] = getValue(i.ToString(), "B");
                    newRow["Description"] = getValue(i.ToString(), "E");
                    string AttributeShortName = getValue(i.ToString(), "F");
                    newRow["Attribute"] = AttributeShortName;
                    string variableUnit = getValue(i.ToString(), "G");
                    newRow["Unit"] = variableUnit;
                    newRow["ConvFactor"] = getValue(i.ToString(), "H");
                    newRow["Block"] = int.Parse(getValue(i.ToString(), "I"));
                    if (AttributeShortName.Length > 1) // if not mapped yet
                    {
                        // select related attribute as row from mappedAttributes Table
                        System.Data.DataRow mappedAttributesRow = mappedAttributes.Select("ShortName = '" + AttributeShortName + "'").First<System.Data.DataRow>();
                        // add related attributeId and unitId to the mappedVariables Table
                        newRow["AttributeId"] = Convert.ToInt64(mappedAttributesRow["AttributeId"]);
                        newRow["UnitId"] = Convert.ToInt64(mappedAttributesRow["UnitId"]);
                    }
                    if (variableUnit.Length > 0) // if dimensioned
                    {
                        // select related unit as row of mappedUnits
                        System.Data.DataRow mappedUnitsRow = mappedUnits.Select("Abbreviation = '" + variableUnit + "'").First<System.Data.DataRow>();
                        // add related UnitId of variable Unit to the mappedVariables Table
                        newRow["VarUnitId"] = Convert.ToInt64(mappedUnitsRow["UnitId"]);
                    }
                    mappedVariables.Rows.Add(newRow);
                }
            }

            newWorkbook.Close();

            return mappedVariables;
        }


        // read variables from XLSX mapping file
        public System.Data.DataTable readVariables(string filePath, System.Data.DataTable mappedAttributes, System.Data.DataTable mappedUnits)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";
            string sheetName = "mapping";
            long startRow = 2;
            long endRow = startRow;

            System.Data.DataTable mappedVariables = new System.Data.DataTable();
            mappedVariables.Columns.Add("DatasetId", typeof(string));
            mappedVariables.Columns.Add("Name", typeof(string));
            mappedVariables.Columns.Add("Description", typeof(string));
            mappedVariables.Columns.Add("Block", typeof(int));
            mappedVariables.Columns.Add("Attribute", typeof(string));
            mappedVariables.Columns.Add("Unit", typeof(string));
            mappedVariables.Columns.Add("ConvFactor", typeof(string));
            mappedVariables.Columns.Add("AttributeId", typeof(long));
            mappedVariables.Columns.Add("UnitId", typeof(long));
            mappedVariables.Columns.Add("VarUnitId", typeof(long));

            excel_init(mappingFile, sheetName, startRow, ref endRow);
            for (long i = startRow; i < endRow; i++)
            {
                System.Data.DataRow newRow = mappedVariables.NewRow();
                newRow["DatasetId"] = getValue(i.ToString(), "A");
                newRow["Name"] = getValue(i.ToString(), "B");
                newRow["Description"] = getValue(i.ToString(), "E");
                string AttributeShortName = getValue(i.ToString(), "F");
                newRow["Attribute"] = AttributeShortName;
                string variableUnit = getValue(i.ToString(), "G");
                newRow["Unit"] = variableUnit;
                newRow["ConvFactor"] = getValue(i.ToString(), "H");
                newRow["Block"] = int.Parse(getValue(i.ToString(), "I"));
                if (AttributeShortName.Length > 1) // if not mapped yet
                {
                    // select related attribute as row from mappedAttributes Table
                    System.Data.DataRow mappedAttributesRow = mappedAttributes.Select("ShortName = '" + AttributeShortName + "'").First<System.Data.DataRow>();
                    // add related attributeId and unitId to the mappedVariables Table
                    newRow["AttributeId"] = Convert.ToInt64(mappedAttributesRow["AttributeId"]);
                    newRow["UnitId"] = Convert.ToInt64(mappedAttributesRow["UnitId"]);
                }
                if (variableUnit.Length > 0) // if dimensioned
                {
                    // select related unit as row of mappedUnits
                    System.Data.DataRow mappedUnitsRow = mappedUnits.Select("Abbreviation = '" + variableUnit + "'").First<System.Data.DataRow>();
                    // add related UnitId of variable Unit to the mappedVariables Table
                    newRow["VarUnitId"] = Convert.ToInt64(mappedUnitsRow["UnitId"]);
                }
                mappedVariables.Rows.Add(newRow);
            }

            newWorkbook.Close();

            return mappedVariables;
        }


        // read attributes from excel mapping file
        public System.Data.DataTable readAttributes(string filePath, System.Data.DataTable mappedUnits, System.Data.DataTable mappedDataTypes)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";
            string sheetName = "attributes";
            long startRow = 5;
            long endRow = startRow;

            System.Data.DataTable mappedAttributes = new System.Data.DataTable();
            mappedAttributes.Columns.Add("Name", typeof(string));
            mappedAttributes.Columns.Add("ShortName", typeof(string));
            mappedAttributes.Columns.Add("Description", typeof(string));
            mappedAttributes.Columns.Add("IsMultipleValue", typeof(string));
            mappedAttributes.Columns.Add("IsBuiltIn", typeof(string));
            mappedAttributes.Columns.Add("Owner", typeof(string));
            mappedAttributes.Columns.Add("ContainerType", typeof(string));
            mappedAttributes.Columns.Add("MeasurementScale", typeof(string));
            mappedAttributes.Columns.Add("EntitySelectionPredicate", typeof(string));
            mappedAttributes.Columns.Add("Self", typeof(string));
            mappedAttributes.Columns.Add("DataType", typeof(string));
            mappedAttributes.Columns.Add("Unit", typeof(string));
            mappedAttributes.Columns.Add("Methodology", typeof(string));
            mappedAttributes.Columns.Add("Constraints", typeof(string));
            mappedAttributes.Columns.Add("ExtendedProperties", typeof(string));
            mappedAttributes.Columns.Add("GlobalizationInfos", typeof(string));
            mappedAttributes.Columns.Add("AggregateFunctions", typeof(string));
            mappedAttributes.Columns.Add("DataTypeId", typeof(long));
            mappedAttributes.Columns.Add("UnitId", typeof(long));
            mappedAttributes.Columns.Add("AttributeId", typeof(long));

            excel_init(mappingFile, sheetName, startRow, ref endRow);
            for (long i = startRow; i < endRow; i++)
            {
                System.Data.DataRow newRow = mappedAttributes.NewRow();
                newRow["Name"] = getValue(i.ToString(), "A");
                newRow["ShortName"] = getValue(i.ToString(), "B");
                newRow["Description"] = getValue(i.ToString(), "C");
                newRow["IsMultipleValue"] = getValue(i.ToString(), "D");
                newRow["IsBuiltIn"] = getValue(i.ToString(), "E");
                newRow["Owner"] = getValue(i.ToString(), "F");
                newRow["ContainerType"] = getValue(i.ToString(), "G");
                newRow["MeasurementScale"] = getValue(i.ToString(), "H");
                newRow["EntitySelectionPredicate"] = getValue(i.ToString(), "I");
                newRow["Self"] = getValue(i.ToString(), "J");
                string DataType = getValue(i.ToString(), "K");
                newRow["DataType"] = DataType;
                string UnitAbbreviation = getValue(i.ToString(), "L");
                newRow["Unit"] = UnitAbbreviation;
                newRow["Methodology"] = getValue(i.ToString(), "M");
                newRow["Constraints"] = getValue(i.ToString(), "N");
                newRow["ExtendedProperties"] = getValue(i.ToString(), "O");
                newRow["GlobalizationInfos"] = getValue(i.ToString(), "P");
                newRow["AggregateFunctions"] = getValue(i.ToString(), "Q");
                // add DataTypesId and UnitId to the mappedAttributes Table
                newRow["DataTypeId"] = Convert.ToInt64(mappedDataTypes.Select("Name = '" + DataType + "'").First<System.Data.DataRow>()["DataTypesId"]);
                newRow["UnitId"] = Convert.ToInt64(mappedUnits.Select("Abbreviation = '" + UnitAbbreviation + "'").First<System.Data.DataRow>()["UnitId"]);
                mappedAttributes.Rows.Add(newRow);
            }

            newWorkbook.Close();

            return mappedAttributes;
        }


        // read units from excel mapping file
        public System.Data.DataTable readUnits(string filePath, System.Data.DataTable mappedDimensions)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";
            string sheetName = "unitMapping";
            long startRow = 5;
            long endRow = startRow;

            System.Data.DataTable mappedUnits = new System.Data.DataTable();
            mappedUnits.Columns.Add("Name", typeof(string));
            mappedUnits.Columns.Add("Abbreviation", typeof(string));
            mappedUnits.Columns.Add("Description", typeof(string));
            mappedUnits.Columns.Add("DimensionName", typeof(string));
            mappedUnits.Columns.Add("MeasurementSystem", typeof(string));
            mappedUnits.Columns.Add("DataTypes", typeof(string));
            mappedUnits.Columns.Add("DimensionId", typeof(long));
            mappedUnits.Columns.Add("UnitId", typeof(long));


            excel_init(mappingFile, sheetName, startRow, ref endRow);
            for (long i = startRow; i < endRow; i++)
            {
                System.Data.DataRow newRow = mappedUnits.NewRow();
                newRow["Name"] = getValue(i.ToString(), "A");
                newRow["Abbreviation"] = getValue(i.ToString(), "B");
                newRow["Description"] = getValue(i.ToString(), "C");
                string DimensionName = getValue(i.ToString(), "D");
                newRow["DimensionName"] = DimensionName;
                newRow["MeasurementSystem"] = getValue(i.ToString(), "E");
                newRow["DataTypes"] = getValue(i.ToString(), "F");
                newRow["DimensionId"] = Convert.ToInt64(mappedDimensions.Select("Name = '" + DimensionName + "'").First<System.Data.DataRow>()["DimensionId"]);
                mappedUnits.Rows.Add(newRow);
            }

            newWorkbook.Close();

            return mappedUnits;
        }




        // read dimensions from excel mapping file
        public System.Data.DataTable readDimensions(string filePath)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";
            string sheetName = "dimensions";
            long startRow = 2;
            long endRow = startRow;

            System.Data.DataTable mappedDimensions = new System.Data.DataTable();
            mappedDimensions.Columns.Add("Id", typeof(string));
            mappedDimensions.Columns.Add("Name", typeof(string));
            mappedDimensions.Columns.Add("Description", typeof(string));
            mappedDimensions.Columns.Add("Syntax", typeof(string));
            mappedDimensions.Columns.Add("DimensionId", typeof(long));

            excel_init(mappingFile, sheetName, startRow, ref endRow);
            for (long i = startRow; i < endRow; i++)
            {
                System.Data.DataRow newRow = mappedDimensions.NewRow();
                newRow["Id"] = getValue(i.ToString(), "A");
                newRow["Name"] = getValue(i.ToString(), "B");
                newRow["Syntax"] = getValue(i.ToString(), "C");
                newRow["Description"] = "";
                mappedDimensions.Rows.Add(newRow);
            }

            newWorkbook.Close();

            return mappedDimensions;
        }


        // read data types from excel mapping file
        public System.Data.DataTable readDataTypes(string filePath)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";
            string sheetName = "dataTypes";
            long startRow = 5;
            long endRow = startRow;

            System.Data.DataTable mappedDataTypes = new System.Data.DataTable();
            mappedDataTypes.Columns.Add("Name", typeof(string));
            mappedDataTypes.Columns.Add("Description", typeof(string));
            mappedDataTypes.Columns.Add("SystemType", typeof(string));
            mappedDataTypes.Columns.Add("DataTypesId", typeof(long));

            excel_init(mappingFile, sheetName, startRow, ref endRow);
            for (long i = startRow; i < endRow; i++)
            {
                System.Data.DataRow newRow = mappedDataTypes.NewRow();
                newRow["Name"] = getValue(i.ToString(), "A");
                newRow["Description"] = getValue(i.ToString(), "B");
                newRow["SystemType"] = getValue(i.ToString(), "C");
                mappedDataTypes.Rows.Add(newRow);
            }

            newWorkbook.Close();

            return mappedDataTypes;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////
        // helper methods
        #region helper methods

        //Method to initialize opening Excel
        static void excel_init(String path, string sheetName, long startRow, ref long endRow)
        {
            appExcel = new Microsoft.Office.Interop.Excel.Application();

            if (System.IO.File.Exists(path))
            {
                newWorkbook = appExcel.Workbooks.Open(path, true, true);
                objsheet = (_Worksheet)appExcel.Sheets[sheetName];
                string rangeCell = "A" + startRow.ToString();
                Range range = objsheet.get_Range(rangeCell);
                range = range.get_End(XlDirection.xlDown);
                endRow = range.Row + 1;
            }
        }

        //Method to get a cell value
        static string getValue(string rowId, string columId)
        {
            string cellname = columId + rowId;
            string value = string.Empty;
            try
            {
                value = objsheet.get_Range(cellname).get_Value().ToString();
            }
            catch
            {
                value = "";
            }
            return value;
        }

        #endregion
    }
}
