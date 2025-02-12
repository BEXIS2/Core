using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Validation;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Input
{
    /// <summary>
    /// DataReader is an abstract class that has functions for reading and validate the rows.
    /// </summary>
    /// <remarks>Convert list of strings to datatuple takes place here.
    /// Most of the functions work with a list of strings.</remarks>
    public abstract class DataReader : IDataReader
    {
        #region public

        /// <summary>
        /// if a few errors occur, they are stored here
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="Error"/>
        public List<Error> ErrorMessages { get; set; }

        /// <summary>
        /// Store the position of the last readed row
        /// </summary>
        /// <remarks>used for converting rows to datatuples with packet size</remarks>
        /// <seealso cref=""/>
        public int Position { get; set; }

        //public bool EndOfFile { get; protected set; }

        public int NumberOfRows { get; set; }
        public int NumberOSkippedfRows { get; set; }

        #endregion public

        #region protected

        /// <summary>
        /// File to be read as stream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="Stream"/>
        protected Stream FileStream { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected string FileName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected StructuredDataStructure StructuredDataStructure { get; set; }

        /// <summary>
        /// stores additional information that are needed to read the FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiReaderInfo"/>
        /// <seealso cref="ExcelReaderInfo"/>
        protected FileReaderInfo Info { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected List<DataTuple> DataTuples = new List<DataTuple>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected DatasetManager DatasetManager;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected long DatasetId = 0;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected List<List<string>> VariableIdentifierRows = new List<List<string>>();

        /// <summary>
        /// VariableIndentifiers from  FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected List<VariableIdentifier> SubmitedVariableIdentifiers = new List<VariableIdentifier>();

        /// <summary>
        /// VariableIndentifiers from DataStructure
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected List<VariableIdentifier> DataStructureVariableIndentifiers = new List<VariableIdentifier>();

        /// <summary>
        /// Dictionary with variable id as key and and a ValueValidationManager for each variable
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        protected Dictionary<long, ValueValidationManager> ValueValidationManagerDic = new Dictionary<long, ValueValidationManager>();

        protected IOUtility IOUtility;

        #endregion protected

        #region private

        private IList<VariableInstance> variableList;
        private bool sameOrderLikeStructure = false;

        #endregion private

        public DataReader(StructuredDataStructure structuredDatastructure, FileReaderInfo fileReaderInfo) : this(structuredDatastructure, fileReaderInfo, new IOUtility(), new DatasetManager())
        {
        }

        public DataReader(StructuredDataStructure structuredDatastructure, FileReaderInfo fileReaderInfo, IOUtility iOUtility) : this(structuredDatastructure, fileReaderInfo, iOUtility, new DatasetManager())
        {
        }

        public DataReader(StructuredDataStructure structuredDatastructure, FileReaderInfo fileReaderInfo, DatasetManager datasetManager) : this(structuredDatastructure, fileReaderInfo, new IOUtility(), datasetManager)
        {
        }

        public DataReader(StructuredDataStructure structuredDatastructure, FileReaderInfo fileReaderInfo, IOUtility iOUtility, DatasetManager datasetManager)
        {
            DatasetManager = datasetManager;
            IOUtility = iOUtility;
            StructuredDataStructure = structuredDatastructure;
            Info = fileReaderInfo;
            ErrorMessages = new List<Error>();
            Position = 1;
        }

        #region IDataReader Member

        /// <summary>
        /// If FileStream exist open a FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="File"/>
        /// <param ="fileName">Full path of the FileStream</param>
        public virtual FileStream Open(string fileName)
        {
            if (File.Exists(fileName))
                return File.Open(fileName, FileMode.Open, FileAccess.Read);
            else
                return null;
        }

        /// <summary>
        /// Read Row and convert each value into a variableValue
        /// and each row to a Datatuple
        /// </summary>
        /// <param name="row">List of values in one row</param>
        /// <param name="indexOfRow">Currently row index</param>
        /// <returns>DataTuple</returns>
        public DataTuple ReadRow(List<string> row, int indexOfRow)
        {
            if (row == null) return null;
            if (row.Count == 1 && string.IsNullOrEmpty(row.ElementAt(0))) return null;
            if (row.Count > this.StructuredDataStructure.Variables.Count || row.Count < this.StructuredDataStructure.Variables.Count) throw new Exception("Number of values different then the number of values.");

            DataTuple dt = new DataTuple();
            string value = "";

            // convert row to List<VariableValue>
            for (int i = 0; i < row.Count(); i++)
            {
                VariableIdentifier variableIdentifier = this.SubmitedVariableIdentifiers.ElementAt(i);

                long variableId = 0;
                if (variableIdentifier.id > 0)
                    variableId = this.SubmitedVariableIdentifiers.ElementAt(i).id;
                else
                    variableId = getVariableUsage(variableIdentifier).Id;

                //if the value is a missing value get the placeholder
                ValueValidationManager validationManager = ValueValidationManagerDic[variableId];
                if (!validationManager.ValueIsMissingValueGetPlaceHolder(row[i], i, out value)) // jump over this code if its a missing value
                {
                    // if variable from systemtype datatime
                    // maybee needs to convert into the default datetime culture format
                    if (this.StructuredDataStructure.Variables.Where(p => p.Id.Equals(variableId)).FirstOrDefault().DataType.SystemType.Equals("DateTime"))
                    {
                        var variable = this.StructuredDataStructure.Variables.Where(p => p.Id.Equals(variableId)).FirstOrDefault();
                        Dlm.Entities.DataStructure.DataType dataType = variable.DataType;

                        if (variable.DisplayPatternId>0)
                        {
                            DataTypeDisplayPattern dp = variable.DisplayPatternId > 0 ? DataTypeDisplayPattern.Get(variable.DisplayPatternId) : null;
                            if (dp != null && !string.IsNullOrEmpty(dp.StringPattern)) value = IOUtility.ConvertToDateUS(row[i], dp.StringPattern);
                            else value = IOUtility.ConvertDateToCulture(row[i]);
                        }
                        else
                        {
                            value = IOUtility.ConvertDateToCulture(row[i]);
                        }
                    }
                    else
                    {
                        if (this.StructuredDataStructure.Variables.Where(p => p.Id.Equals(variableId)).FirstOrDefault().DataType.SystemType.Equals("Double") ||
                            this.StructuredDataStructure.Variables.Where(p => p.Id.Equals(variableId)).FirstOrDefault().DataType.SystemType.Equals("Decimal") ||
                            this.StructuredDataStructure.Variables.Where(p => p.Id.Equals(variableId)).FirstOrDefault().DataType.SystemType.Equals("Float"))
                        {
                            var datatype = this.StructuredDataStructure.Variables.Where(p => p.Id.Equals(variableId)).FirstOrDefault().DataType.SystemType;
                            value = row[i];

                            if (Info.Decimal.Equals(DecimalCharacter.comma))
                            {
                                if (value.Contains(".")) value = value.Replace(".", "");
                                if (value.Contains(",")) value = value.Replace(',', '.');
                            }

                            if (Info.Decimal.Equals(DecimalCharacter.point))
                            {
                                if (value.Contains(",")) value = value.Remove(',');
                            }

                            switch (datatype)
                            {
                                case "Double":
                                    {
                                        double tmp = 0;
                                        if (double.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out tmp))
                                            value = tmp.ToString("G16", new CultureInfo("en-US"));
                                        break;
                                    }

                                case "Decimal":
                                    {
                                        decimal tmp = 0;
                                        if (decimal.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out tmp))
                                            value = "" + tmp.ToString("G29", new CultureInfo("en-US"));
                                        break;
                                    }
                                case "Float":
                                    {
                                        float tmp = 0;
                                        if (float.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out tmp))
                                            value = "" + tmp.ToString("G7", new CultureInfo("en-US"));
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            value = row[i].Trim();
                        }
                    }
                }

                dt.VariableValues.Add(DatasetManager.CreateVariableValue(value, "", DateTime.Now, DateTime.Now, new ObtainingMethod(), variableId, new List<ParameterValue>()));
            }

            return dt;
        }

        /// <summary>
        /// Read Row an return only values in where the variable is in identifiers.
        /// </summary>
        /// <param name="row">List of values in one ro</param>
        /// <param name="indexOfRow">Currently row index</param>
        /// <param name="identifiers">list of variableids</param>
        /// <returns></returns>
        protected List<string> GetValuesFromRow(List<string> row, int indexOfRow, List<long> identifiers)
        {
            List<string> temp = new List<string>();
            VariableIdentifier variableIdentifier;
            List<string> tempNames = new List<string>();
            variableList = StructuredDataStructure.Variables.ToList();

            // convert row to List<VariableValue>
            for (int i = 0; i < row.Count(); i++)
            {
                variableIdentifier = this.SubmitedVariableIdentifiers.ElementAt(i);
                long id = variableIdentifier.id;

                /// <summary>
                /// if id == 0 this happen when the incoming FileStream is a text oder csv FileStream
                /// no id for vartiables existing
                /// </summary>
                /// <remarks></remarks>
                if (id == 0)
                {
                    foreach (long idX in identifiers)
                    {
                        string tempName = variableList.Where(v => v.Id.Equals(idX)).First().Label;
                        if (tempName.Equals(variableIdentifier.name))
                        {
                            temp.Add(row[i]);
                        }
                    }
                }
                else
                {
                    /// <summary>
                    /// if you have the ids of the submitted VariableIdentifiers
                    /// you can check against the ids
                    /// </summary>
                    /// <remarks></remarks>
                    if (identifiers.Contains(id))
                    {
                        temp.Add(row[i]);
                    }
                }
            }

            return temp;
        }

        #region validation

        private void setValidationInformation()
        {
        }

        private VariableIdentifier hv = new VariableIdentifier();
        private ValueValidationManager validationManager;
        private List<Error> temp = new List<Error>();
        private List<Error> temp2 = new List<Error>();
        private List<Error> errors = new List<Error>();
        private Variable var = null;
        private string v;
        private object value;

        /// <summary>
        /// Validate a row
        /// </summary>
        /// <seealso cref=""/>
        /// <param name="row">List of strings</param>
        /// <param name="indexOfRow">Index of row</param>
        /// <returns>List of errors or null</returns>
        public List<Error> ValidateRow(List<string> row, int indexOfRow)
        {
            errors = new List<Error>();

            // number of variables in datastructure
            int numOfVariables = this.StructuredDataStructure.Variables.Count();

            // number of current row values
            int rowCount = row.Count();

            // number of is equal
            if (numOfVariables.Equals(rowCount))
            {
                int valuePosition = 0;
                for (int i = 0; i < row.Count; i++)
                {
                    v = row[i];
                    try
                    {
                        hv = SubmitedVariableIdentifiers.ElementAt(i);
                        var = getVariableUsage(hv);
                        validationManager = ValueValidationManagerDic[var.Id];

                        //check if the value is a missing value
                        if (!validationManager.ValueIsMissingValue(v, indexOfRow))
                        {
                            // returns the checked value and update the error list if error appears
                            value = validationManager.CheckValue(v, indexOfRow, ref temp);

                            if (temp.Count == 0)
                            {
                                //temp = validationManager.ValidateValue(v, indexOfRow);
                                temp = validationManager.ValidateConstraints(value, indexOfRow);
                                //temp = temp.Union(temp2).ToList();
                            }

                            if (temp.Any()) errors = errors.Union(temp).ToList();
                        }
                        valuePosition++;
                    }
                    catch (Exception ex)
                    {
                        Error e = new Error(ErrorType.Other, "Error : " + ex.Message);
                        errors.Add(e);
                    }
                }
            }
            // different value lenght
            else
            {
                Error e = new Error(ErrorType.Other, "Number of Values different as number of variables");
                errors.Add(e);
            }

            NumberOfRows++;

            return errors;
        }

        /// <summary>
        /// Compare Datastructure with Submited Variables
        /// And create a Dictionary of ValueValidationmanagers
        /// </summary>
        /// <remarks></remarks>
        /// <param name="row"></param>
        /// <returns></returns>
        public List<Error> ValidateComparisonWithDatatsructure(List<VariableIdentifier> variableIdentifers)
        {
            List<Error> matchErrors = new List<Error>();
            List<Error> orderErrors = new List<Error>();
            List<Error> errors = new List<Error>();

            try
            {
                List<VariableIdentifier> source = getDatastructureVariableIdentifiers();

                //check if all variables exist in the incoming and the existing datastructure
                DatastructureMatchCheck dmc = new DatastructureMatchCheck();
                matchErrors = dmc.Execute(SubmitedVariableIdentifiers, source, this.StructuredDataStructure.Name);

                if (matchErrors != null) errors.AddRange(matchErrors);

                if (matchErrors == null || !matchErrors.Any())
                {
                    // check the equivalent order of the strutcures
                    DatastructureOrderCheck dso = new DatastructureOrderCheck();
                    orderErrors = dso.Execute(SubmitedVariableIdentifiers, source, this.StructuredDataStructure.Name);

                    sameOrderLikeStructure = orderErrors == null ? true : false;

                    if (orderErrors != null) errors.AddRange(orderErrors);
                }
            }
            catch
            {
            }

            if (!errors.Any())
            {
                for (int i = 0; i < variableIdentifers.Count; i++)
                {
                    VariableIdentifier hv = variableIdentifers.ElementAt(i);

                    if (hv != null)
                    {
                        VariableInstance sdvu = getVariableUsage(hv);

                        if (sdvu != null)
                        {
                            string varName = sdvu.Label;
                            bool optional = sdvu.IsValueOptional;
                            string dataType = sdvu.DataType.SystemType;

                            // change parameters to only sdvu
                            if (!this.ValueValidationManagerDic.ContainsKey(sdvu.Id))
                                this.ValueValidationManagerDic.Add(sdvu.Id, createValueValidationManager(varName, dataType, optional, sdvu));
                        }
                        else
                        {
                            errors.Add(new Error(ErrorType.Datastructure, "Error with name of Variables"));
                        }
                    }
                }//for
            }
            else
            {
                if (errors.Count > 0) return errors;
                else return null;
            }

            return null;
        }

        /// <summary>
        ///  Create ValueValidationManager of a Variable
        /// </summary>
        /// <remarks></remarks>
        /// <param name="varName"></param>
        /// <param name="dataType"></param>
        /// <param name="optional"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        private ValueValidationManager createValueValidationManager(string varName, string dataType, bool optional, VariableInstance variable)
        {
            string pattern = "";

            if (string.IsNullOrEmpty(varName))
            {
                varName = variable.Label;
            }

            DataTypeDisplayPattern displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Id.Equals(variable.DisplayPatternId)).FirstOrDefault(); //HH:mm:ss
            if (displayPattern != null) pattern = displayPattern.StringPattern;

            ValueValidationManager vvm = new ValueValidationManager(varName, dataType, optional, Info.Decimal, pattern, variable.MissingValues, CultureInfo.CurrentCulture, variable.VariableConstraints);

            return vvm;
        }

        #endregion validation

        /// <summary>
        /// Get VariableUsage based on VariableIdentifer
        /// </summary>
        /// <remarks></remarks>
        /// <param name="hv"></param>
        /// <returns></returns>
        private VariableInstance getVariableUsage(VariableIdentifier hv)
        {
            VariableInstance sdvu = new VariableInstance();

            if (hv.id != 0)
            {
                var dsVar = (from v in this.StructuredDataStructure.Variables
                             where v.Id == hv.id && v.Label == hv.name
                             select v).FirstOrDefault();
                if (dsVar != null) sdvu = dsVar;
            }
            else
            {
                var dsVar = (from v in this.StructuredDataStructure.Variables
                             where v.Label == hv.name
                             select v).FirstOrDefault();
                if (dsVar != null) sdvu = dsVar;
            }

            return sdvu;
        }

        /// <summary>
        /// Get VairableIdentifiers from datastructure
        /// </summary>
        /// <returns></returns>
        private List<VariableIdentifier> getDatastructureVariableIdentifiers()
        {
            if (this.DataStructureVariableIndentifiers == null | this.DataStructureVariableIndentifiers.Count == 0)
                return getDatastructureAsListOfVariableIdentifers(this.StructuredDataStructure.Variables);
            else
                return this.DataStructureVariableIndentifiers;
        }

        /// <summary>
        /// Get List of VariableIdentifers converted from Datastructure Variable
        /// </summary>
        /// <param name="VariableUsageCollection"></param>
        /// <returns></returns>
        private List<VariableIdentifier> getDatastructureAsListOfVariableIdentifers(ICollection<VariableInstance> Variables)
        {
            var tempList = from v in Variables
                           select new VariableIdentifier
                           {
                               name = v.Label,
                               id = v.Id,
                               systemType = v.DataType.SystemType
                           };

            return tempList.ToList();
        }

        #endregion IDataReader Member

        #region static methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="path"></param>
        public static bool FileExist(string path)
        {
            if (File.Exists(path))
                return true;
            else
                return false;
        }

        #endregion static methods

        #region getter setter

        public List<VariableIdentifier> SetSubmitedVariableIdentifiers(List<string> variableNames)
        {
            SubmitedVariableIdentifiers = new List<VariableIdentifier>();

            foreach (string s in variableNames)
            {
                VariableIdentifier vi = new VariableIdentifier();
                vi.name = s;
                SubmitedVariableIdentifiers.Add(vi);
            }

            return SubmitedVariableIdentifiers;
        }

        #endregion getter setter
    }
}