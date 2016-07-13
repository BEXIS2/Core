using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IBM.Data.DB2;
using IBM.Data.DB2Types;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.RPM.Output;
using System.Xml;

namespace BExISMigration
{
    public class DataStructureCreator
    {
        public DataStructure CreateDataStructure(string dataSetID, DataTable mapVariables, List<string> variableNames)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataContainerManager attributeManager = new DataContainerManager();
            StructuredDataStructure dataStructure = new StructuredDataStructure();
            UnitManager unitManager = new UnitManager();

            // values of DataStructure
            ExcelTemplateProvider provider = new ExcelTemplateProvider();
            string name = "oldBExIS" + dataSetID;
            string description = "old BExIS datastructure, created for " + dataSetID + ", possibly used by other"; //metadata.Title;
            string xsdFileName = "";//"xsdFileName";
            string xslFileName = "";//"xslFileName";
            DataStructureCategory indexerType = DataStructureCategory.Generic;

            // if dataStructure not exists
            StructuredDataStructure existDS = existingDataStructures(mapVariables, dataSetID);
            if (existDS.Name == null)
            {
                // create dataStructure
                dataStructure = dataStructureManager.CreateStructuredDataStructure(name, description, xsdFileName, xslFileName, indexerType, null);

                foreach (string varName in variableNames)
                {
                    // values of variables
                    string attName = "";
                    string convFactor = "";
                    string varDescription = "";
                    int Block = -999;
                    long AttributeId = -999, UnitId = -999, VarUnitId = -999;
                    foreach (DataRow mapRow in mapVariables.Select("DatasetId = '" + dataSetID + "'"))
                    {
                        if (mapRow["Name"].ToString() == varName)
                        {
                            attName = mapRow["Attribute"].ToString();
                            convFactor = mapRow["ConvFactor"].ToString();
                            varDescription = mapRow["Description"].ToString();
                            Block = int.Parse(mapRow["Block"].ToString());
                            if (attName.Length > 1) // if not mapped yet
                            {
                                AttributeId = Convert.ToInt64(mapRow["AttributeId"].ToString());
                                UnitId = Convert.ToInt64(mapRow["UnitId"].ToString());
                                if (mapRow["Unit"].ToString().Length > 0)
                                    VarUnitId = Convert.ToInt64(mapRow["VarUnitId"].ToString());
                            }
                        }
                    }

                    if (AttributeId > 0 && Block == 0) // if not mapped yet AND variable is in block 0
                    {
                        // find existing attribute for each variable
                        DataAttribute attribute = attributeManager.DataAttributeRepo.Get(AttributeId);

                        Unit varUnit = null;
                        if (VarUnitId > 0)
                        {
                            varUnit = unitManager.Repo.Get(VarUnitId);
                        }

                        // add variables to dataStructure
                        Variable variable = dataStructureManager.AddVariableUsage(dataStructure, attribute, true, varName, null, null, varDescription, varUnit);
                        dataStructure.Variables.Add(variable);
                    }
                }
                provider.CreateTemplate(dataStructure);
                return dataStructure;
            }
            else
            {
                return existDS;
            }
        }


        // create unstructured DataStructures
        public DataStructure CreateDataStructure(string fileType)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            UnStructuredDataStructure dataStructure = new UnStructuredDataStructure();

            // values of DataStructure
            string name = fileType;
            string description = "old BExIS unstrctured data structure: " + fileType;

            UnStructuredDataStructure existDS = dataStructureManager.UnStructuredDataStructureRepo.Get(s => name.Equals(s.Name)).FirstOrDefault();
            if (existDS == null)
            {
                // create dataStructure
                return dataStructureManager.CreateUnStructuredDataStructure(name, description);
            }
            else
            {
                return existDS;
            }
        }


        private StructuredDataStructure existingDataStructures(DataTable mapVariables, string dataSetID)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = new StructuredDataStructure();

            // get alldatasetId respective variables-rows from mapped-variables-table
            DataRow[] mappedRows = mapVariables.Select("DatasetId = '" + dataSetID + "' AND Block = 0");

            // get all datastrcutures from repo which has the same number of variables
            List<StructuredDataStructure> existDatastructures = dataStructureManager.StructuredDataStructureRepo.Get(s =>
                mappedRows.Count().Equals(s.Variables.Count)).ToList();

            foreach (StructuredDataStructure existDs in existDatastructures)
            {
                bool isEqualStructure = true;
                //foreach (DataRow mapRow in mapVariables.Rows)
                foreach (DataRow mapRow in mappedRows)
                {
                    int exvarno = existDs.Variables.Where(v =>
                        Convert.ToInt64(mapRow["UnitId"]).Equals(v.Unit.Id) &&
                        mapRow["Name"].ToString().Equals(v.Label) &&
                        Convert.ToInt64(mapRow["AttributeId"]).Equals(v.DataAttribute.Id)
                        ).Count();
                    if (exvarno == 0)
                    {
                        isEqualStructure &= false;
                        break;
                    }
                }
                if (isEqualStructure)
                {
                    dataStructure = existDs;
                    break;
                }
            }

            return dataStructure;
        }
    }
}
