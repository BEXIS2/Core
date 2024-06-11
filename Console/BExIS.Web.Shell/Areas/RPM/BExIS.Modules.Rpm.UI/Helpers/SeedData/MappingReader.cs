//using Microsoft.Office.Interop.Excel;
//using BExIS.IO.Transform;
//using BExIS.IO.Transform.Input;

using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BExIS.Modules.Rpm.UI.Helpers.SeedData
{
    public class MappingReader
    {
        //private static Microsoft.Office.Interop.Excel.Application appExcel;
        //private static Workbook newWorkbook = null;
        //private static _Worksheet objsheet = null;

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

            System.IO.StreamReader file = new System.IO.StreamReader(mappingFile, System.Text.Encoding.UTF8);

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
        //public System.Data.DataTable readVariables(string filePath, string dataSetID, System.Data.DataTable mappedAttributes, System.Data.DataTable mappedUnits)
        //{
        //    string mappingFile = filePath + @"\variableMapping.xlsx";
        //    string sheetName = "mapping";
        //    long startRow = 2;
        //    long endRow = startRow;

        //    System.Data.DataTable mappedVariables = new System.Data.DataTable();
        //    mappedVariables.Columns.Add("DatasetId", typeof(string));
        //    mappedVariables.Columns.Add("Name", typeof(string));
        //    mappedVariables.Columns.Add("Description", typeof(string));
        //    mappedVariables.Columns.Add("Block", typeof(int));
        //    mappedVariables.Columns.Add("Attribute", typeof(string));
        //    mappedVariables.Columns.Add("Unit", typeof(string));
        //    mappedVariables.Columns.Add("ConvFactor", typeof(string));
        //    mappedVariables.Columns.Add("AttributeId", typeof(long));
        //    mappedVariables.Columns.Add("UnitId", typeof(long));
        //    mappedVariables.Columns.Add("VarUnitId", typeof(long));

        //    excel_init(mappingFile, sheetName, startRow, ref endRow);

        //    for (long i = startRow; i < endRow; i++)
        //    {
        //        System.Data.DataRow newRow = mappedVariables.NewRow();
        //        if (getValue(i.ToString(), "A") == dataSetID)
        //        {
        //            newRow["DatasetId"] = getValue(i.ToString(), "A");
        //            newRow["Name"] = getValue(i.ToString(), "B");
        //            newRow["Description"] = getValue(i.ToString(), "E");
        //            string AttributeShortName = getValue(i.ToString(), "F");
        //            newRow["Attribute"] = AttributeShortName;
        //            string variableUnit = getValue(i.ToString(), "G");
        //            newRow["Unit"] = variableUnit;
        //            newRow["ConvFactor"] = getValue(i.ToString(), "H");
        //            newRow["Block"] = int.Parse(getValue(i.ToString(), "I"));
        //            if (AttributeShortName.Length > 1) // if not mapped yet
        //            {
        //                // select related attribute as row from mappedAttributes Table
        //                System.Data.DataRow mappedAttributesRow = mappedAttributes.Select("ShortName = '" + AttributeShortName + "'").First<System.Data.DataRow>();
        //                // add related attributeId and unitId to the mappedVariables Table
        //                newRow["AttributeId"] = Convert.ToInt64(mappedAttributesRow["AttributeId"]);
        //                newRow["UnitId"] = Convert.ToInt64(mappedAttributesRow["UnitId"]);
        //            }
        //            if (variableUnit.Length > 0) // if dimensioned
        //            {
        //                // select related unit as row of mappedUnits
        //                System.Data.DataRow mappedUnitsRow = mappedUnits.Select("Abbreviation = '" + variableUnit + "'").First<System.Data.DataRow>();
        //                // add related UnitId of variable Unit to the mappedVariables Table
        //                newRow["VarUnitId"] = Convert.ToInt64(mappedUnitsRow["UnitId"]);
        //            }
        //            mappedVariables.Rows.Add(newRow);
        //        }
        //    }

        //    newWorkbook.Close();

        //    return mappedVariables;
        //}

        // read variables from XLSX mapping file
        //public System.Data.DataTable readVariables(string filePath, System.Data.DataTable mappedAttributes, System.Data.DataTable mappedUnits)
        //{
        //    string mappingFile = filePath + @"\variableMapping.xlsx";
        //    string sheetName = "mapping";
        //    long startRow = 2;
        //    long endRow = startRow;

        //    System.Data.DataTable mappedVariables = new System.Data.DataTable();
        //    mappedVariables.Columns.Add("DatasetId", typeof(string));
        //    mappedVariables.Columns.Add("Name", typeof(string));
        //    mappedVariables.Columns.Add("Description", typeof(string));
        //    mappedVariables.Columns.Add("Block", typeof(int));
        //    mappedVariables.Columns.Add("Attribute", typeof(string));
        //    mappedVariables.Columns.Add("Unit", typeof(string));
        //    mappedVariables.Columns.Add("ConvFactor", typeof(string));
        //    mappedVariables.Columns.Add("AttributeId", typeof(long));
        //    mappedVariables.Columns.Add("UnitId", typeof(long));
        //    mappedVariables.Columns.Add("VarUnitId", typeof(long));

        //    excel_init(mappingFile, sheetName, startRow, ref endRow);
        //    for (long i = startRow; i < endRow; i++)
        //    {
        //        System.Data.DataRow newRow = mappedVariables.NewRow();
        //        newRow["DatasetId"] = getValue(i.ToString(), "A");
        //        newRow["Name"] = getValue(i.ToString(), "B");
        //        newRow["Description"] = getValue(i.ToString(), "E");
        //        string AttributeShortName = getValue(i.ToString(), "F");
        //        newRow["Attribute"] = AttributeShortName;
        //        string variableUnit = getValue(i.ToString(), "G");
        //        newRow["Unit"] = variableUnit;
        //        newRow["ConvFactor"] = getValue(i.ToString(), "H");
        //        newRow["Block"] = int.Parse(getValue(i.ToString(), "I"));
        //        if (AttributeShortName.Length > 1) // if not mapped yet
        //        {
        //            // select related attribute as row from mappedAttributes Table
        //            System.Data.DataRow mappedAttributesRow = mappedAttributes.Select("ShortName = '" + AttributeShortName + "'").First<System.Data.DataRow>();
        //            // add related attributeId and unitId to the mappedVariables Table
        //            newRow["AttributeId"] = Convert.ToInt64(mappedAttributesRow["AttributeId"]);
        //            newRow["UnitId"] = Convert.ToInt64(mappedAttributesRow["UnitId"]);
        //        }
        //        if (variableUnit.Length > 0) // if dimensioned
        //        {
        //            // select related unit as row of mappedUnits
        //            System.Data.DataRow mappedUnitsRow = mappedUnits.Select("Abbreviation = '" + variableUnit + "'").First<System.Data.DataRow>();
        //            // add related UnitId of variable Unit to the mappedVariables Table
        //            newRow["VarUnitId"] = Convert.ToInt64(mappedUnitsRow["UnitId"]);
        //        }
        //        mappedVariables.Rows.Add(newRow);
        //    }

        //    newWorkbook.Close();

        //    return mappedVariables;
        //}

        /// <summary>
        /// read attributes from csv file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mappedUnits"></param>
        /// <param name="mappedDataTypes"></param>
        /// <returns></returns>
        public System.Data.DataTable readAttributes(string filePath)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";

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

            if (File.Exists(filePath + "\\attributes.csv") && File.Exists(filePath + "\\datatypes.csv") && File.Exists(filePath + "\\units.csv"))
            {
                using (StreamReader reader = new StreamReader(filePath + "\\attributes.csv", Encoding.UTF8, true))
                {
                    string line = "";
                    //jump over the first row
                    line = reader.ReadLine();

                    UnitManager unitManager = null;
                    DataTypeManager dataTypeManager = null;
                    try
                    {
                        unitManager = new UnitManager();
                        dataTypeManager = new DataTypeManager();

                        while ((line = reader.ReadLine()) != null)
                        {
                            // (char)59 = ';'
                            string[] vars = line.Split((char)59);

                            System.Data.DataRow newRow = mappedAttributes.NewRow();
                            newRow["Name"] = vars[0];
                            newRow["ShortName"] = vars[1];
                            newRow["Description"] = vars[2];
                            newRow["IsMultipleValue"] = vars[3];
                            newRow["IsBuiltIn"] = vars[4];
                            newRow["Owner"] = vars[5];
                            newRow["ContainerType"] = vars[6];
                            newRow["MeasurementScale"] = vars[7];
                            newRow["EntitySelectionPredicate"] = vars[8];
                            newRow["Self"] = vars[9];
                            string DataType = vars[10];
                            newRow["DataType"] = DataType;
                            string UnitAbbreviation = vars[11];
                            newRow["Unit"] = UnitAbbreviation;
                            newRow["Methodology"] = vars[12];
                            newRow["Constraints"] = vars[13];
                            newRow["ExtendedProperties"] = vars[14];
                            newRow["GlobalizationInfos"] = vars[15];
                            newRow["AggregateFunctions"] = vars[16];
                            // add DataTypesId and UnitId to the mappedAttributes Table
                            newRow["DataTypeId"] = dataTypeManager.Repo.Get().Where(dt => dt.Name.ToLower().Equals(DataType.ToLower())).FirstOrDefault().Id;
                            Unit unit = unitManager.Repo.Get().Where(u => u.Abbreviation.Equals(UnitAbbreviation)).FirstOrDefault();
                            if (unit != null)
                                newRow["UnitId"] = unitManager.Repo.Get().Where(u => u.Abbreviation.Equals(UnitAbbreviation)).FirstOrDefault().Id;
                            else
                                newRow["UnitId"] = 1;
                            mappedAttributes.Rows.Add(newRow);
                        }
                    }
                    finally
                    {
                        unitManager.Dispose();
                        dataTypeManager.Dispose();
                    }
                }
            }
            return mappedAttributes;
        }

        /// <summary>
        /// read units from csv file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mappedDimensions"></param>
        /// <returns></returns>
        public System.Data.DataTable readUnits(string filePath)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";

            System.Data.DataTable mappedUnits = new System.Data.DataTable();
            mappedUnits.Columns.Add("Name", typeof(string));
            mappedUnits.Columns.Add("Abbreviation", typeof(string));
            mappedUnits.Columns.Add("Description", typeof(string));
            mappedUnits.Columns.Add("DimensionName", typeof(string));
            mappedUnits.Columns.Add("MeasurementSystem", typeof(string));
            mappedUnits.Columns.Add("DataTypes", typeof(string));
            mappedUnits.Columns.Add("DimensionId", typeof(long));
            mappedUnits.Columns.Add("UnitId", typeof(long));

            if (File.Exists(filePath + "\\units.csv") && File.Exists(filePath + "\\dimensions.csv"))
            {
                using (StreamReader reader = new StreamReader(filePath + "\\units.csv", Encoding.UTF8, true))
                {
                    UnitManager unitmanager = null;
                    try
                    {
                        unitmanager = new UnitManager();
                        string line = "";
                        //jump over the first row
                        line = reader.ReadLine();
                        Dimension dim = new Dimension();

                        while ((line = reader.ReadLine()) != null)
                        {
                            // (char)59 = ';'
                            string[] vars = line.Split((char)59);

                            System.Data.DataRow newRow = mappedUnits.NewRow();
                            newRow["Name"] = vars[0];
                            newRow["Abbreviation"] = vars[1];
                            newRow["Description"] = vars[2];
                            string DimensionName = vars[3];
                            newRow["DimensionName"] = DimensionName;
                            newRow["MeasurementSystem"] = vars[4];
                            newRow["DataTypes"] = vars[5];
                            dim = unitmanager.DimensionRepo.Get().Where(d => d.Name.ToLower().Equals(DimensionName.ToLower())).FirstOrDefault();
                            if (dim != null)
                                newRow["DimensionId"] = dim.Id;
                            else
                                newRow["DimensionId"] = 1;
                            mappedUnits.Rows.Add(newRow);
                        }
                    }
                    finally
                    {
                        unitmanager.Dispose();
                    }
                }
            }
            return mappedUnits;
        }

        /// <summary>
        /// read dimensions from csv file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public System.Data.DataTable readDimensions(string filePath)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";

            System.Data.DataTable mappedDimensions = new System.Data.DataTable();
            mappedDimensions.Columns.Add("Id", typeof(string));
            mappedDimensions.Columns.Add("Name", typeof(string));
            mappedDimensions.Columns.Add("Description", typeof(string));
            mappedDimensions.Columns.Add("Syntax", typeof(string));
            mappedDimensions.Columns.Add("DimensionId", typeof(long));

            if (File.Exists(filePath + "\\dimensions.csv"))
            {
                using (StreamReader reader = new StreamReader(filePath + "\\dimensions.csv", Encoding.UTF8, true))
                {
                    string line = "";
                    //jump over the first row
                    line = reader.ReadLine();

                    while ((line = reader.ReadLine()) != null)
                    {
                        // (char)59 = ';'
                        string[] vars = line.Split((char)59);

                        System.Data.DataRow newRow = mappedDimensions.NewRow();
                        newRow["Id"] = vars[0];
                        newRow["Name"] = vars[1];
                        newRow["Syntax"] = vars[2];
                        newRow["Description"] = vars[3];
                        mappedDimensions.Rows.Add(newRow);
                    }
                }
            }

            return mappedDimensions;
        }

        /// <summary>
        /// Read data types from csv file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public System.Data.DataTable readDataTypes(string filePath)
        {
            string mappingFile = filePath + @"\variableMapping.xlsx";

            System.Data.DataTable mappedDataTypes = new System.Data.DataTable();
            mappedDataTypes.Columns.Add("Name", typeof(string));
            mappedDataTypes.Columns.Add("Description", typeof(string));
            mappedDataTypes.Columns.Add("SystemType", typeof(string));
            mappedDataTypes.Columns.Add("DisplayPattern", typeof(string));
            mappedDataTypes.Columns.Add("DataTypesId", typeof(long));

            if (File.Exists(filePath + "\\datatypes.csv"))
            {
                using (StreamReader reader = new StreamReader(filePath + "\\datatypes.csv", Encoding.UTF8, true))
                {
                    string line = "";
                    //jump over the first row
                    line = reader.ReadLine();

                    while ((line = reader.ReadLine()) != null)
                    {
                        // (char)59 = ';'
                        string[] vars = line.Split((char)59);

                        System.Data.DataRow newRow = mappedDataTypes.NewRow();
                        newRow["Name"] = vars[0].Trim();
                        newRow["Description"] = vars[1].Trim();
                        newRow["SystemType"] = vars[2].Trim();
                        newRow["DisplayPattern"] = vars[3].Trim();
                        mappedDataTypes.Rows.Add(newRow);
                    }
                }
            }

            return mappedDataTypes;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        // helper methods
        //    #region helper methods

        //    //Method to initialize opening Excel
        //    static void excel_init(String path, string sheetName, long startRow, ref long endRow)
        //    {
        //        appExcel = new Microsoft.Office.Interop.Excel.Application();

        //        if (System.IO.File.Exists(path))
        //        {
        //            newWorkbook = appExcel.Workbooks.Open(path, true, true);
        //            objsheet = (_Worksheet)appExcel.Sheets[sheetName];
        //            string rangeCell = "A" + startRow.ToString();
        //            Range range = objsheet.get_Range(rangeCell);
        //            range = range.get_End(XlDirection.xlDown);
        //            endRow = range.Row + 1;
        //        }
        //    }

        //    //Method to get a cell value
        //    static string getValue(string rowId, string columId)
        //    {
        //        string cellname = columId + rowId;
        //        string value = string.Empty;
        //        try
        //        {
        //            value = objsheet.get_Range(cellname).get_Value().ToString();
        //        }
        //        catch
        //        {
        //            value = "";
        //        }
        //        return value;
        //    }

        //    #endregion
    }
}