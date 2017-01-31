using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Data;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO;

namespace BExISMigration
{
    public class XmlDataReader : DataReader
    {
        public XmlDataReader()
        {
            Info = new FileReaderInfo();
            Info.Decimal = DecimalCharacter.point;
        }

        /// <summary>
        /// wandelt bexis primardaten (rows) in bexis 2 datatuples um
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataStructureId"></param>
        /// <param name="observationIndex"></param>
        /// <returns></returns>
        public DataTuple XmlRowReader(XmlDocument data, long dataStructureId, int observationIndex)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataTuple dataRow = new DataTuple();

            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            this.StructuredDataStructure = dataStructure;


            List<VariableIdentifier> variableIdentifierList = new List<VariableIdentifier>();

            foreach (Variable variable in dataStructure.Variables)
            {
                VariableIdentifier vi = new VariableIdentifier();

                vi.id = variable.Id;
                vi.name = variable.Label;
                vi.systemType = variable.DataAttribute.DataType.SystemType.ToString();

                variableIdentifierList.Add(vi);
            }

            this.SubmitedVariableIdentifiers = variableIdentifierList;

            List<string> dataList = XmlRowToStringList(data, variableIdentifierList);

            int startRow = 0;
            int indexOfRow = startRow + observationIndex;
            //List<Error> errors = ValidateRow(dataList, indexOfRow);
            //if (errors.Count == 0)
            {
                dataRow = ReadRow(dataList, indexOfRow);
            }

            return dataRow;
        }

        private List<string> XmlRowToStringList(XmlDocument data, List<VariableIdentifier> variableList)
        {
            List<string> dataList = new List<string>();

            foreach (VariableIdentifier variable in variableList)
            {
                foreach (XmlNode blockNode in data.ChildNodes)
                {
                    XmlNode node = BExIS.Xml.Helpers.XmlUtility.GetXmlNodeByName(blockNode, variable.name);
                    if (node != null)
                    {
                        dataList.Add(node.InnerText);
                    }
                    else
                    {
                        dataList.Add("");
                    }
                }
            }


            return dataList;
        }
    }
}
