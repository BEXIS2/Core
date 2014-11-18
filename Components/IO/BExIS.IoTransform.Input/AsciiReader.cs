using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using BExIS.Io.Transform.Validation.DSValidation;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;

/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Input
{
    /// <summary>
    /// this class is used to read and validate ascii files
    /// </summary>
    /// <remarks></remarks>        
    public class AsciiReader:DataReader
    {
        /// <summary>
        /// Read the whole file line by line until no more come. 
        /// Convert the lines into a datatuple based on the datastructure.
        /// Return value is a list of datatuples
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="file">Stream of the file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <returns>List of datatuples</returns>
        public List<DataTuple> ReadFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId)
        {
            this.file = file;
            this.fileName = fileName;
            this.info = fri;
            this.structuredDataStructure = sds;
            this.datasetId = datasetId;

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
                    int index = fri.Variables;
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);

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


        /// <summary>
        /// Read line by line based on a packageSize. 
        /// Convert the lines into a datatuple based on the datastructure.
        /// Return value is a list of datatuples
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="file">Stream of the file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="packageSize"></param>
        /// <returns>List of datatuples</returns>
        public List<DataTuple> ReadFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId, int packageSize)
        {

            // clear list of datatuples
            this.dataTuples = new List<DataTuple>();
            this.variableIdentifierRows = new List<List<string>>();
            this.SubmitedVariableIdentifiers = new List<VariableIdentifier>();

            this.file = file;
            this.fileName = fileName;
            this.info = fri;
            this.structuredDataStructure = sds;
            this.datasetId = datasetId;

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
                    int items = 0;
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);

                    //int end = packageSize;
                    //int start = 1;
                    // read to position
                    if (Position == 1)
                    {
                        Position = this.info.Data;
                    }

                    Stopwatch _timer = Stopwatch.StartNew();


                    /// <summary>
                    /// go to current position is reached at the line
                    /// </summary>
                    /// <remarks></remarks>        
                    for (int i = 1; i < Position; i++)
                    {
                        string l = streamReader.ReadLine();

                        if (i == this.info.Variables)
                        {
                            variableIdentifierRows.Add(RowToList(l, seperator));
                            ConvertAndAddToSubmitedVariableIdentifier();
                        }
                    }

                    _timer.Stop();
                    Debug.WriteLine(" ");
                    Debug.WriteLine("*****************************************************************");
                    Debug.WriteLine(" position : " + Position+"    -->  Timer: "+ _timer.Elapsed.TotalSeconds.ToString() );

                    _timer = Stopwatch.StartNew();

                        /// <summary>
                        /// read each line as long as the packet size is not reached
                        /// generating a datatuple from the line
                        /// </summary>
                        /// <remarks></remarks>        
                        while ((line = streamReader.ReadLine()) != null && items <= packageSize-1)
                        {
                            if (Position >= this.info.Data)
                            {
                                // return List of VariablesValues, and error messages
                                this.dataTuples.Add(ReadRow(RowToList(line, seperator), index));
                            }

                            Position++;
                            index++;
                            items++;
                        }

                        _timer.Stop();

                        Debug.WriteLine(" created datatuples : " + _timer.Elapsed.TotalSeconds.ToString());
                        

                }
            }

            
            return this.dataTuples;
        }

        /// <summary>
        /// Get all values from the file of each variable in variable list
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="file">Stream of the file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="variableList">List of variables</param>
        /// <param name="packageSize">size of a package</param>
        /// <returns></returns>
        public List<List<string>> ReadValuesFromFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId, List<long> variableList, int packageSize)
        {
            this.file = file;
            this.fileName = fileName;
            this.info = fri;
            this.structuredDataStructure = sds;
            this.datasetId = datasetId;

            List<List<string>> listOfSelectedvalues = new List<List<string>>();

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
                Stopwatch totalTime = Stopwatch.StartNew();

                using (StreamReader streamReader = new StreamReader(file))
                {
                    string line;
                    //int index = fri.Variables;
                    int index = 1;
                    int items = 0;
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);

                    //int end = packageSize;
                    //int start = 1;
                    // read to position
                    if (Position == 1)
                    {
                        Position = this.info.Data;
                    }

                    Stopwatch _timer = Stopwatch.StartNew();


                    /// <summary>
                    /// go to current position is reached at the line
                    /// </summary>
                    /// <remarks></remarks>        
                    for (int i = 1; i < Position; i++)
                    {
                        string l = streamReader.ReadLine();

                        if (i == this.info.Variables)
                        {
                            variableIdentifierRows.Add(RowToList(l, seperator));
                            ConvertAndAddToSubmitedVariableIdentifier();
                        }
                    }


                    // go to every line
                    while ((line = streamReader.ReadLine()) != null && items <= packageSize - 1)
                    {

                        //// is position of datastructure?
                        //if (index == this.info.Variables)
                        //{
                        //    variableIdentifierRows.Add(RowToList(line, seperator));
                        //    ConvertAndAddToSubmitedVariableIdentifier();
                        //}

                        // is position = or over startposition of data?
                        if (Position >= this.info.Data)
                        {
                            Stopwatch rowTime = Stopwatch.StartNew();

                            // return List of VariablesValues, and error messages
                            listOfSelectedvalues.Add(GetValuesFromRow(RowToList(line, seperator), index, variableList));

                            rowTime.Stop();
                            //Debug.WriteLine("index : "+index+"   ---- Total Time of primary key check " + rowTime.Elapsed.TotalSeconds.ToString());
                        }

                        Position++;
                        index++;
                        items++;

                    }

                    _timer.Stop();

                    Debug.WriteLine(" get values for primary key check datatuples : " + _timer.Elapsed.TotalSeconds.ToString());
                }

                totalTime.Stop();
                Debug.WriteLine(" Total Time of primary key check " + totalTime.Elapsed.TotalSeconds.ToString());
            }

            return listOfSelectedvalues;
        }

        /// <summary>
        /// Get all values from the file of each variable in variable list
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="file">Stream of the file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="variableList">List of variables</param>
        /// <returns></returns>
        //public List<List<string>> ReadValuesFromFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId, List<long> variableList)
        //{
        //    this.file = file;
        //    this.fileName = fileName;
        //    this.info = fri;
        //    this.structuredDataStructure = sds;
        //    this.datasetId = datasetId;

        //    List<List<string>> listOfSelectedvalues = new List<List<string>>();

        //    // Check params
        //    if (this.file == null)
        //    {
        //        this.errorMessages.Add(new Error(ErrorType.Other, "File not exist"));
        //    }
        //    if (!this.file.CanRead)
        //    {
        //        this.errorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
        //    }
        //    if (this.info.Variables <= 0)
        //    {
        //        this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Variable can´t be 0"));
        //    }
        //    if (this.info.Data <= 0)
        //    {
        //        this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
        //    }

        //    if (this.errorMessages.Count == 0)
        //    {
        //        Stopwatch totalTime = Stopwatch.StartNew();

        //        using (StreamReader streamReader = new StreamReader(file))
        //        {
        //            string line;
        //            //int index = fri.Variables;
        //            int index = 1;
                  
        //            char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);

    
        //            // go to every line
        //            while ((line = streamReader.ReadLine()) != null )
        //            {

        //                // is position of datastructure?
        //                if (index == this.info.Variables)
        //                {
        //                    variableIdentifierRows.Add(RowToList(line, seperator));
        //                    ConvertAndAddToSubmitedVariableIdentifier();
        //                }

        //                // is position = or over startposition of data?
        //                if (index >= this.info.Data)
        //                {
        //                    // return List of VariablesValues, and error messages
        //                    listOfSelectedvalues.Add(GetValuesFromRow(RowToList(line, seperator), index, variableList));
        //                }

        //                index++;
        //            }
        //        }

        //        //Debug.WriteLine(" Total Time of primary key check " + totalTime.Elapsed.TotalSeconds.ToString());
        //    }

        //    return listOfSelectedvalues;
        //}


        #region validate

        /// <summary>
        /// Validate the whole file line by line until no more come. 
        /// Convert the lines into a datatuple based on the datastructure.
        /// Return value is a list of datatuples
        /// </summary>
        /// <remarks>A list of errorMessages is filled when the fil is not valid </remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="file">Stream of the file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        public void ValidateFile(Stream file, string fileName, AsciiFileReaderInfo fri, StructuredDataStructure sds, long datasetId)
        {
            this.file = file;
            this.fileName = fileName;
            this.info = fri;
            this.structuredDataStructure = sds;
            this.datasetId = datasetId;

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
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);
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


        /// <summary>
        /// Validate the datastructure
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="TextSeparator"/>
        /// <param name="line">line which include the datastructure as names</param>       
        /// <param name="sep">TextSeparator as Character</param>       
        protected bool ValidateDatastructure(string line, char sep )
        {

            /// <summary>
            /// the incoming line should be the datastructure line from a file
            /// this line converted into a list of strings which incluide the variable names 
            /// </summary>
            /// <remarks></remarks>        
            variableIdentifierRows.Add(RowToList(line, sep));

            /// <summary>
            /// Convert a list of variable names to 
            /// VariableIdentifiers 
            /// </summary>
            /// <remarks>SubmitedVariableIdentifiers is the list</remarks>  
            ConvertAndAddToSubmitedVariableIdentifier();


            /// <summary>
            /// Validate datastructure with the SubmitedVariableIdentifiers from file 
            /// </summary>
            /// <remarks></remarks>        
            List<Error> ecList = ValidateComparisonWithDatatsructure(SubmitedVariableIdentifiers);


            if (ecList != null)
            {
                if (ecList.Count > 0)
                {
                    this.errorMessages = this.errorMessages.Concat(ecList).ToList();
                    return false;
                }
                else return true;
            }
            else return true;
        }

        /// <summary>
        /// Convert a list of variable names to 
        /// VariableIdentifiers
        /// </summary>
        /// <seealso cref="VariableIdentifier"/>
        /// <param name="variablesRow"></param>
        private void ConvertAndAddToSubmitedVariableIdentifier()
        {
            
            if (variableIdentifierRows != null)
            {
                foreach (List<string> l in variableIdentifierRows)
                {
                    //create headerVariables
                    if (SubmitedVariableIdentifiers.Count == 0)
                    {
                        foreach (string s in l)
                        {
                            VariableIdentifier hv = new VariableIdentifier();
                            hv.name = s;
                            SubmitedVariableIdentifiers.Add(hv);
                        }
                    }
                    else
                    {
                        foreach (string s in l)
                        {
                            int id = Convert.ToInt32(s);
                            int index = l.IndexOf(s);
                            SubmitedVariableIdentifiers.ElementAt(index).id = id;
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
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="line">Row as a string</param>
        /// <param name="seperator">Character used as TextSeparator</param>
        /// <returns>List of values</returns>
        private List<string> RowToList(string line, char seperator)
        {
            //if (this.info != null)
            //{
            //    AsciiFileReaderInfo fileReaderInfo = (AsciiFileReaderInfo)this.info;
            //    if (fileReaderInfo != null)
            //    {
            //        List<string> temp = new List<string>();
            //        temp = TextMarkerHandling(line, seperator, AsciiFileReaderInfo.GetTextMarker(fileReaderInfo.TextMarker));
            //        return temp;

            //    }
            //    else return line.Split(seperator).ToList();
            //}
            //else 

            return line.Split(seperator).ToList();
        }


        /// <summary>
        /// If a seperator is present in a text which is highlighted with highlighter (bsp quotes), 
        /// which is a special case which is treated in this function
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="TextSeparator"/>
        /// <seealso cref="TextMarker"/>
        /// <param name="row">a row that needs to be checked </param>       
        /// <param name="separator">Character as Delimeter for the line (TextSeparator)</param>       
        /// <param name="textmarker">Character as TextMarker for the line</param>       
        /// <returns>List of values as a string list</returns>
        private List<string> TextMarkerHandling(string row, char separator, char textmarker)
        {
            List<string> values = row.Split(separator).ToList();

            /// <summary>
            /// check if the row contains a textmarker
            /// </summary>
            /// <remarks></remarks>        
            if (row.Contains(textmarker))
            {
                string tempValue = "";
                bool startText = false;
                
                List<string> temp = new List<string>();

                foreach (string v in values)
                {

                    /// <summary>
                    /// check if the value v contains a textmarker
                    /// and generate a new string which include all values between
                    /// the first Text marker and the last TextMarker
                    /// </summar>
                    /// <remarks></remarks>    
                    if (v.Contains(textmarker))
                    {

                        if (v.ToCharArray().First().Equals(textmarker))
                        {
                            tempValue = v;
                            startText = true;
                        }

                        if (v.ToCharArray().Last().Equals(textmarker))
                        {
                            tempValue += separator+v;
                            temp.Add(tempValue.Trim(textmarker));
                            startText = false;
                        }

                    }
                    else
                    {
                        if (startText) 
                            tempValue += separator + v;
                        else temp.Add(v);
                    }
                }

                return temp;
            }

            return values;
        }

  

        #endregion

    }
}
