using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.DCM.Transform.Validation.DSValidation;
using BExIS.DCM.Transform.Validation.Exceptions;

namespace BExIS.DCM.Transform.Input
{
    

    public class AsciiReader:DataReader
    {
        
        // read file using template
        public List<DataTuple> ReadFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId)
        {
            this.file = file;
            this.fileName = fileName;
            this.info = fri;
            this.structuredDataStructure = sds;
            this._datasetId = datasetId;

             // Check params
            if (this.file == null)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.file.CanRead)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.info.Variables <= 0)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Variable can´t be 0"));
            }
            if (this.info.Data <= 0)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.errorMessages.Count == 0)
            {


                using (StreamReader streamReader = new StreamReader(file))
                {
                    string line;
                    int index = 1;
                    char seperator = getSeperatorCharacter(fri.Seperator);

                    while ((line = streamReader.ReadLine()) != null)
                    {

                        if (index == this.info.Variables)
                        {
                            variableIdentifierRows.Add(RowToList(line, seperator));
                            ConvertAndAddToSubmitedVariableIdentifier();
                        }

                        if (index >= this.info.Data)
                        {
                            // return List of VariablesValues, and error messages
                            this.dataTuples.Add(ReadRow(RowToList(line, seperator), index));
                        }

                        index++;

                    }
                }
            }

            return this.dataTuples;
        }

        #region validate

        public void ValidateFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId)
        {
            this.file = file;
            this.fileName = fileName;
            this.info = fri;
            this.structuredDataStructure = sds;
            this._datasetId = datasetId;

            // Check params
            if (this.file == null)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.file.CanRead)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.info.Variables <= 0)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Variable can´t be 0"));
            }
            if (this.info.Data <= 0)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.errorMessages.Count == 0)
            {
                using (StreamReader streamReader = new StreamReader(file))
                {
                    string line;
                    int index = 1;
                    char seperator = getSeperatorCharacter(fri.Seperator);
                    bool dsdIsOk = false;

                    while ((line = streamReader.ReadLine()) != null)
                    {

                        if (index == this.info.Variables)
                        {
                            dsdIsOk = ValidateDatastructure(line, seperator);
                        }

                        if (dsdIsOk && index >= this.info.Data)
                        {
                            this.errorMessages = this.errorMessages.Union(ValidateRow(RowToList(line, seperator), index)).ToList();
                        }

                        index++;

                    }
                }
            }
        }

        protected bool ValidateDatastructure(string line, char sep )
        {
            variableIdentifierRows.Add(RowToList(line, sep));

            ConvertAndAddToSubmitedVariableIdentifier();

            List<Error> ecList = ValidateComparisonWithDatatsructure(SubmitedVariableIndentifiers);

            if (ecList != null)
            {
                if (ecList.Count > 0)
                {
                    this.errorMessages.Concat(ecList);
                    return false;
                }
                else return true;
            }
            else return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variablesRow"></param>
        private void ConvertAndAddToSubmitedVariableIdentifier()
        {
            
            if (variableIdentifierRows != null)
            {
                foreach (List<string> l in variableIdentifierRows)
                {
                    //create headerVariables
                    if (SubmitedVariableIndentifiers.Count == 0)
                    {
                        foreach (string s in l)
                        {
                            VariableIdentifier hv = new VariableIdentifier();
                            hv.name = s;
                            SubmitedVariableIndentifiers.Add(hv);
                        }
                    }
                    else
                    {
                        foreach (string s in l)
                        {
                            int id = Convert.ToInt32(s);
                            int index = l.IndexOf(s);
                            SubmitedVariableIndentifiers.ElementAt(index).id = id;
                        }
                    }
                }
            }
        }

        #endregion

        #region helper methods
        
        /// <summary>
        /// Convert a row as a string to a list of strings
        /// </summary>
        /// <param name="line">Row as a string</param>
        /// <param name="seperator">Character used as delimiter</param>
        /// <returns></returns>
        private List<string> RowToList(string line, char seperator)
        {
            return line.Split(seperator).ToList();
        }
        
        /// <summary>
        /// Get a Textseparator as a Character
        /// </summary>
        /// <param name="sep"></param>
        /// <returns></returns>
        private char getSeperatorCharacter(TextSeperator sep)
        {
            switch (sep)
            {
                case TextSeperator.comma:
                    return ',';
                case TextSeperator.semicolon:
                    return ';';
                case TextSeperator.space:
                    return ' ';
                case TextSeperator.tab:
                default:
                    return '\t';
            }
        }

        #endregion

    }
}
