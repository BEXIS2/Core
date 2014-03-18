using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BExIS.Io.Transform.Validation;
using BExIS.Io.Transform.Validation.DSValidation;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Io.Transform.Validation.ValueValidation;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Io.Transform.Input
{

    /// <summary>
    /// DataReader is an abstract class that has functions for reading and validate the rows.
    /// </summary>
    /// <remarks>Convert list of strings to datatuple takes place here. 
    /// Most of the functions work with a list of strings.</remarks>        
    public abstract class DataReader:IDataReader
    {
        
        #region public

        /// <summary>
        /// if a few errors occur, they are stored here
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="Error"/>        
        public List<Error> errorMessages { get; set; }


        /// <summary>
        /// Store the position of the last readed row 
        /// </summary>
        /// <remarks>used for converting rows to datatuples with packet size</remarks>
        /// <seealso cref=""/> 
        public int Position {get;set;}

        //public bool EndOfFile { get; protected set; }


        #endregion

        #region protected 


            /// <summary>
            /// File to be read as stream
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref="Stream"/>        
            protected Stream file { get; set; }
 
            protected string fileName { get; set; }

            protected StructuredDataStructure structuredDataStructure { get; set; }


            /// <summary>
            /// stores additional information that are needed to read the file
            /// </summary>
            /// <remarks></remarks>  
            /// <seealso cref="AsciiReaderInfo"/> 
            /// <seealso cref="ExcelReaderInfo"/> 
            protected FileReaderInfo info { get; set; }


            protected List<DataTuple> dataTuples = new List<DataTuple>();

            protected DatasetManager datasetManager = new DatasetManager();

            protected long datasetId = 0;

            protected List<List<string>> variableIdentifierRows = new List<List<string>>();

            /// <summary>
            /// VariableIndentifiers from  file
            /// </summary>
            /// <remarks></remarks>    
            protected List<VariableIdentifier> SubmitedVariableIdentifiers = new List<VariableIdentifier>();

            /// <summary>
            /// VariableIndentifiers from DataStructure
            /// </summary>
            /// <remarks></remarks>    
            protected List<VariableIdentifier> DataStructureVariableIndentifiers = new List<VariableIdentifier>();

            /// <summary>
            /// Dictionary with variable id as key and and a ValueValidationManager for each variable
            /// </summary>
            /// <remarks></remarks>    
            protected Dictionary<long,ValueValidationManager> ValueValidationManagerDic = new Dictionary<long ,ValueValidationManager>();

        #endregion

        #region private 
           
        #endregion
            

        //Contructor
        public DataReader()
        {
            this.errorMessages = new List<Error>();
            Position = 1;
        }

        #region IDataReader Member


        /// <summary>
        /// If file exist open a FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="File"/>
        /// <param ="fileName">Full path of the file</param>       
        public FileStream Open(string fileName)
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
            DataTuple dt = new DataTuple();
              
            // convert row to List<VariableValue>
            for(int i=0;i< row.Count();i++ )
            {
                    
                VariableIdentifier variableIdentifier = this.SubmitedVariableIdentifiers.ElementAt(i);
                long variableId = 0;
                if (variableIdentifier.id > 0)
                    variableId = this.SubmitedVariableIdentifiers.ElementAt(i).id;
                else
                    variableId = GetVariableUsage(variableIdentifier).Id;

                dt.VariableValues.Add(datasetManager.CreateVariableValue(row[i],"", DateTime.Now, DateTime.Now, new ObtainingMethod(), variableId, new List<ParameterValue>()));
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

            // convert row to List<VariableValue>
            for (int i = 0; i < row.Count(); i++)
            {
                VariableIdentifier variableIdentifier = this.SubmitedVariableIdentifiers.ElementAt(i);
                long id = variableIdentifier.id;

            

                /// <summary>
                /// if id == 0 this happen when the incoming file is a text oder csv file
                /// no id for vartiables existing
                /// </summary>
                /// <remarks></remarks>        
                if (id == 0)
                {
                    List<string> tempNames = new List<string>();
                    DataStructureManager dataStructureManager = new DataStructureManager();

                    foreach (long idX in identifiers)
                    {
                        string tempName = dataStructureManager.VariableRepo.Get().Where(v => v.Id.Equals(idX)).First().Label;
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
  
        /// <summary>
        /// Validate a row
        /// </summary>
        /// <seealso cref=""/>
        /// <param name="row">List of strings</param>
        /// <param name="indexOfRow">Index of row</param>
        /// <returns>List of errors or null</returns>
        public List<Error> ValidateRow(List<string> row, int indexOfRow)
        {

            List<Error> errors = new List<Error>();

            // number of variables in datastructure
            int NumOfVariables = this.structuredDataStructure.Variables.Count();

            // number of current row values
            int rowCount = row.Count();

            // number of is equal
            if (NumOfVariables.Equals(rowCount))
            {
                int valuePosition = 0;
                foreach (string v in row)
                {
                    try
                    {
                        VariableIdentifier hv = SubmitedVariableIdentifiers.ElementAt(row.IndexOf(v));
                        Variable sdvu = GetVariableUsage(hv);

                        ValueValidationManager validationManager = ValueValidationManagerDic[sdvu.Id];

                        List<Error> temp = validationManager.CheckValue(v, indexOfRow);

                        if (temp.Count == 0)
                        {
                            temp = validationManager.ValidateValue(v, indexOfRow);
                        }

                        if (temp != null) errors = errors.Union(temp).ToList();

                        valuePosition++;
                    }
                    catch
                    { 
                        //test
                        if (true)
                        {

                        }
                    
                    }
                }
            }
            // different value lenght
            else
            {
                Error e = new Error(ErrorType.Other, "Error in DataReader Validate Row. Number of Values different as number of variables");
                errors.Add(e);

            }

            return errors;
        }
             
        /// <summary>
        /// Compare Datastructure with Submited Variables
        /// And create a Dictionary of ValueValidationmanagers
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public List<Error> ValidateComparisonWithDatatsructure(List<VariableIdentifier> variableIdentifers)
        {
            List<Error> errors = new List<Error>();

            try
            {
                

                List<VariableIdentifier> source = GetDatastructureVariableIdentifiers();

                DatastructureMatchCheck dmc = new DatastructureMatchCheck();
                errors = dmc.Execute(SubmitedVariableIdentifiers, source, this.structuredDataStructure.Name);
            }
            catch { 
            
   
            }
            if (errors == null)
            {

                for (int i = 0; i < variableIdentifers.Count; i++)
                {
                    VariableIdentifier hv = variableIdentifers.ElementAt(i);

                    if (hv != null)
                    {
                        Variable sdvu = GetVariableUsage(hv);

                        if (sdvu != null)
                        {
                            string varName = sdvu.Label;
                            bool optional = sdvu.IsValueOptional;
                            string dataType = sdvu.DataAttribute.DataType.SystemType;

                            // change parameters to only sdvu
                            this.ValueValidationManagerDic.Add(sdvu.Id, CreateValueValidationManager(varName, dataType, optional, sdvu.DataAttribute));
                        }
                        else
                        {
                            errors.Add(new Error(ErrorType.Datastructure, "Error with name of Variable"));
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
        /// <param name="varName"></param>
        /// <param name="dataType"></param>
        /// <param name="optional"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        private ValueValidationManager CreateValueValidationManager(string varName, string dataType, bool optional, DataAttribute variable)
        {
            ValueValidationManager vvm = new ValueValidationManager(varName, dataType, optional);

            // add checks
            //vvm.NullOrOptionalCheck = new OptionalCheck(varName, dataType, optional);
            //vvm.DataTypeCheck = new DataTypeCheck(varName, dataType);

            //add validations based on contraints of the variable
            if (dataType.Equals(TypeCode.Int16.ToString()) ||
                dataType.Equals(TypeCode.Int32.ToString()) ||
                dataType.Equals(TypeCode.Double.ToString())||
                dataType.Equals(TypeCode.DateTime.ToString()))
            {
                //vvm.ValidationList.Add(new RangeValidation(varName,dataType, 1, 100000000));
            }

            if(dataType.Equals(TypeCode.String.ToString()))
            {
                List<string> l = new List<string>();
                l.Add("ABC");
                l.Add("HEG31");
                l.Add("HEG32");
                l.Add("HEG33");

                //vvm.ValidationList.Add(new DomainValidation(varName,dataType,l));

                //string re1="([a-z])";	// Any Single Word Character (Not Whitespace) 1
                //string re2="([a-z])";	// Any Single Word Character (Not Whitespace) 2
                //string re3="([a-z])";	// Any Single Word Character (Not Whitespace) 3
                //string re4="(\\d+)";	// Integer Number 1

                string pattern = "[H|Z][E|T][G|R]\\d+";

                //vvm.ValidationList.Add(new PatternValidation(varName, dataType, pattern));

            }

            return vvm;
        }

        #endregion

        /// <summary>
        /// Get VariableUsage based on VariableIdentifer
        /// </summary>
        /// <param name="hv"></param>
        /// <returns></returns>
        private Variable GetVariableUsage(VariableIdentifier hv)
        {
            Variable sdvu = new Variable();

            if (hv.id != 0)
            {
                var dsVar = (from v in this.structuredDataStructure.Variables
                             where v.Id == hv.id && v.Label == hv.name
                             select v).FirstOrDefault();
                if (dsVar != null) sdvu = dsVar;

            }
            else
            {
                var dsVar = (from v in this.structuredDataStructure.Variables
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
        private List<VariableIdentifier> GetDatastructureVariableIdentifiers()
        {
            if (this.DataStructureVariableIndentifiers == null | this.DataStructureVariableIndentifiers.Count==0)
                return GetDatastructureAsListOfVariableIdentifers(this.structuredDataStructure.Variables);
            else
                return this.DataStructureVariableIndentifiers;
        }

        /// <summary>
        /// Get List of VariableIdentifers converted from Datastructure Variable
        /// </summary>
        /// <param name="VariableUsageCollection"></param>
        /// <returns></returns>
        private List<VariableIdentifier> GetDatastructureAsListOfVariableIdentifers(ICollection<Variable> Variables)
        { 
            var tempList = from v in Variables
                           select new VariableIdentifier
                           {
                               name = v.Label,
                               id = v.Id
                           };

            return tempList.ToList();
        }
        #endregion

    }


}
