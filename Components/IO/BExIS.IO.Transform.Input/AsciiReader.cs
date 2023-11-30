using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Vaiona.Logging.Aspects;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Input
{
    /// <summary>
    /// this class is used to read and validate ascii files
    /// </summary>
    ///test
    /// <remarks></remarks>
    public class AsciiReader : DataReader
    {
        private Encoding encoding = Encoding.Default;
        private AsciiFileReaderInfo fileReaderInfo;

        public AsciiReader(StructuredDataStructure structuredDatastructure, AsciiFileReaderInfo fileReaderInfo) : base(structuredDatastructure, fileReaderInfo)
        {
            fileReaderInfo = (AsciiFileReaderInfo)this.Info;
        }

        public AsciiReader(StructuredDataStructure structuredDatastructure, AsciiFileReaderInfo fileReaderInfo, IOUtility iOUtility) : base(structuredDatastructure, fileReaderInfo, iOUtility)
        {
            fileReaderInfo = (AsciiFileReaderInfo)this.Info;
        }

        public AsciiReader(StructuredDataStructure structuredDatastructure, AsciiFileReaderInfo fileReaderInfo, IOUtility iOUtility, DatasetManager datasetManager) : base(structuredDatastructure, fileReaderInfo, iOUtility, datasetManager)
        {
            fileReaderInfo = (AsciiFileReaderInfo)this.Info;
        }

        /// <summary>
        /// If FileStream exist open a FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="File"/>
        /// <param ="fileName">Full path of the FileStream</param>
        public override FileStream Open(string fileName)
        {
            // get Encoding first
            setEncoding(fileName);

            if (File.Exists(fileName))
                return File.Open(fileName, FileMode.Open, FileAccess.Read);
            else
                return null;
        }

        public List<List<string>> ReadFile(Stream file)
        {
            List<List<string>> tmp = new List<List<string>>();

            this.FileStream = file;
            AsciiFileReaderInfo fri = (AsciiFileReaderInfo)Info;

            // Check params
            if (this.FileStream == null)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.FileStream.CanRead)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.Info.Variables <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Variables can´t be 0"));
            }
            if (this.Info.Data <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.ErrorMessages.Count == 0)
            {
                using (StreamReader streamReader = new StreamReader(file, encoding))
                {
                    string line;
                    int index = fri.Variables;
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (index >= Info.Data)
                        {
                            // return List of VariablesValues, and error messages
                            if (!isEmpty(line, seperator))
                                tmp.Add(rowToList(line, seperator));
                        }

                        index++;
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Read the whole FileStream line by line until no more come.
        /// Convert the lines into a datatuple based on the datastructure.
        /// Return value is a list of datatuples
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="FileStream">Stream of the FileStream</param>
        /// <param name="fileName">name of the FileStream</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <returns>List of datatuples</returns>
        public List<DataTuple> ReadFile(Stream file, string fileName, long datasetId)
        {
            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;
            AsciiFileReaderInfo fri = (AsciiFileReaderInfo)Info;

            // Check params
            if (this.FileStream == null)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.FileStream.CanRead)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.Info.Variables <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Variables can´t be 0"));
            }
            if (this.Info.Data <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.ErrorMessages.Count == 0)
            {
                using (StreamReader streamReader = new StreamReader(file, encoding))
                {
                    string line;
                    int index = fri.Variables;
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (index == this.Info.Variables)
                        {
                            ValidateDatastructure(line, seperator);
                        }

                        if (index >= this.Info.Data)
                        {
                            // return List of VariablesValues, and error messages
                            if (!isEmpty(line, seperator))
                                this.DataTuples.Add(ReadRow(rowToList(line, seperator), index));
                        }

                        index++;
                    }
                }
            }

            return this.DataTuples;
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
        /// <param name="FileStream">Stream of the FileStream</param>
        /// <param name="fileName">name of the FileStream</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="packageSize"></param>
        /// <returns>List of datatuples</returns>
        [MeasurePerformance]
        public List<DataTuple> ReadFile(Stream file, string fileName, long datasetId, int packageSize)
        {
            // clear list of datatuples
            this.DataTuples = new List<DataTuple>();
            this.VariableIdentifierRows = new List<List<string>>();
            this.SubmitedVariableIdentifiers = new List<VariableIdentifier>();

            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;
            AsciiFileReaderInfo fri = (AsciiFileReaderInfo)Info;

            // Check params
            if (this.FileStream == null)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.FileStream.CanRead)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.Info.Variables <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Variables can´t be 0"));
            }
            if (this.Info.Data <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.ErrorMessages.Count == 0)
            {
                using (StreamReader streamReader = new StreamReader(file, encoding))
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
                        Position = this.Info.Data;
                    }

                    Stopwatch _timer = Stopwatch.StartNew();

                    /// <summary>
                    /// go to current position is reached at the line
                    /// </summary>
                    /// <remarks></remarks>
                    for (int i = 1; i < Position; i++)
                    {
                        string l = streamReader.ReadLine();

                        if (i == this.Info.Variables)
                        {
                            // validate the structure
                            // + create validationmanagers for the variables
                            // + identifer List
                            ValidateDatastructure(l, seperator);
                        }
                    }

                    _timer.Stop();
                    //Debug.WriteLine(" ");
                    //Debug.WriteLine("*****************************************************************");
                    //Debug.WriteLine(" position : " + Position + "    -->  Timer: " + _timer.Elapsed.TotalSeconds.ToString());

                    _timer = Stopwatch.StartNew();

                    /// <summary>
                    /// read each line as long as the packet size is not reached
                    /// generating a datatuple from the line
                    /// </summary>
                    /// <remarks></remarks>
                    while ((line = streamReader.ReadLine()) != null && line.Trim().Count() > 0 && items <= packageSize - 1)
                    {
                        if (Position >= this.Info.Data)
                        {
                            // return List of VariablesValues, and error messages
                            if (!isEmpty(line, seperator))
                                this.DataTuples.Add(ReadRow(rowToList(line, seperator), index));
                        }

                        Position++;
                        index++;
                        items++;
                    }

                    _timer.Stop();

                    //Debug.WriteLine(" created datatuples : " + _timer.Elapsed.TotalSeconds.ToString());
                }
            }

            return this.DataTuples;
        }

        /// <summary>
        /// Get all values from the FileStream of each variable in variable list
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="FileStream">Stream of the FileStream</param>
        /// <param name="fileName">name of the FileStream</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="variableList">List of variables</param>
        /// <param name="packageSize">size of a package</param>
        /// <returns></returns>
        public List<List<string>> ReadValuesFromFile(Stream file, string fileName, long datasetId, List<long> variableList, int packageSize)
        {
            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;
            AsciiFileReaderInfo fri = (AsciiFileReaderInfo)Info;

            List<List<string>> listOfSelectedvalues = new List<List<string>>();

            // Check params
            if (this.FileStream == null)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.FileStream.CanRead)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.Info.Variables <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Variables can´t be 0"));
            }
            if (this.Info.Data <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.ErrorMessages.Count == 0)
            {
                Stopwatch totalTime = Stopwatch.StartNew();

                using (StreamReader streamReader = new StreamReader(file, encoding))
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
                        Position = this.Info.Data;
                    }

                    Stopwatch _timer = Stopwatch.StartNew();

                    /// <summary>
                    /// go to current position is reached at the line
                    /// </summary>
                    /// <remarks></remarks>
                    for (int i = 1; i < Position; i++)
                    {
                        string l = streamReader.ReadLine();

                        if (i == this.Info.Variables)
                        {
                            VariableIdentifierRows.Add(rowToList(l, seperator));
                            convertAndAddToSubmitedVariableIdentifier();
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
                        if (Position >= this.Info.Data)
                        {
                            Stopwatch rowTime = Stopwatch.StartNew();

                            // return List of VariablesValues, and error messages
                            listOfSelectedvalues.Add(GetValuesFromRow(rowToList(line, seperator), index, variableList));

                            rowTime.Stop();
                            ////Debug.WriteLine("index : "+index+"   ---- Total Time of primary key check " + rowTime.Elapsed.TotalSeconds.ToString());
                        }

                        Position++;
                        index++;
                        items++;
                    }

                    _timer.Stop();

                    //Debug.WriteLine(" get values for primary key check datatuples : " + _timer.Elapsed.TotalSeconds.ToString());
                }

                totalTime.Stop();
                //Debug.WriteLine(" Total Time of primary key check " + totalTime.Elapsed.TotalSeconds.ToString());
            }

            return listOfSelectedvalues;
        }

        #region validate

        /// <summary>
        /// Validate the whole FileStream line by line until no more come.
        /// Convert the lines into a datatuple based on the datastructure.
        /// Return value is a list of datatuples
        /// </summary>
        /// <remarks>A list of errorMessages is filled when the fil is not valid </remarks>
        /// <seealso cref="AsciiFileReaderInfo"/>
        /// <seealso cref="DataTuple"/>
        /// <seealso cref="StructuredDataStructure"/>
        /// <param name="FileStream">Stream of the FileStream</param>
        /// <param name="fileName">name of the FileStream</param>
        /// <param name="fri">AsciiFileReaderInfo needed</param>
        /// <param name="sds">StructuredDataStructure</param>
        /// <param name="datasetId">Id of the dataset</param>
        public void ValidateFile(Stream file, string fileName, long datasetId)
        {
            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;
            AsciiFileReaderInfo fri = (AsciiFileReaderInfo)Info;

            // Check params
            if (this.FileStream == null)
            {
                this.ErrorMessages.Add(new Error(ErrorType.File, "File not exist"));
            }
            if (!this.FileStream.CanRead)
            {
                this.ErrorMessages.Add(new Error(ErrorType.File, "File is not readable"));
            }
            if (this.Info.Variables <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.FileReader, "Startrow of Variables can´t be 0"));
            }
            if (this.Info.Data <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.FileReader, "Startrow of Data can´t be 0"));
            }

            if (this.ErrorMessages.Count == 0)
            {
                using (StreamReader streamReader = new StreamReader(file, encoding))
                {
                    string line;
                    int index = 1;
                    char seperator = AsciiFileReaderInfo.GetSeperator(fri.Seperator);
                    bool dsdIsOk = false;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (index == this.Info.Variables)
                        {
                            dsdIsOk = ValidateDatastructure(line, seperator);

                            // if data is not in the correct order, create a dictionary with the new position

                        }

                        if (dsdIsOk && index >= this.Info.Data && !string.IsNullOrEmpty(line) && !isEmpty(line, seperator))
                        {
                            var r = rowToList(line, seperator);
                            var e = ValidateRow(r, index);
                            this.ErrorMessages = this.ErrorMessages.Union(e).ToList();

                            if (this.ErrorMessages.Count >= 1000) break;
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
        protected bool ValidateDatastructure(string line, char sep)
        {
            /// <summary>
            /// the incoming line should be the datastructure line from a FileStream
            /// this line converted into a list of strings which incluide the variable names
            /// </summary>
            /// <remarks></remarks>
            VariableIdentifierRows.Add(rowToList(line, sep));

            /// <summary>
            /// Convert a list of variable names to
            /// VariableIdentifiers
            /// </summary>
            /// <remarks>SubmitedVariableIdentifiers is the list</remarks>
            convertAndAddToSubmitedVariableIdentifier();

            /// <summary>
            /// Validate datastructure with the SubmitedVariableIdentifiers from FileStream
            /// </summary>
            /// <remarks></remarks>
            List<Error> ecList = ValidateComparisonWithDatatsructure(SubmitedVariableIdentifiers);

            if (ecList != null)
            {
                if (ecList.Count > 0)
                {
                    this.ErrorMessages = this.ErrorMessages.Concat(ecList).ToList();
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
        private void convertAndAddToSubmitedVariableIdentifier()
        {
            if (VariableIdentifierRows != null)
            {
                foreach (List<string> l in VariableIdentifierRows)
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

        #endregion validate

        #region basic reader functions without AsciiFileReaderInfo

        public static long Count(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName), "fileName not exist");

            if (File.Exists(fileName))
            {
                long count = 0;

                using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(file))
                    {
                        return countLinesMaybe(file);
                    }
                }

                return count;

            }
            else
            {
                throw new FileNotFoundException("file not found");
            }

            return 0;
        }

        /// <summary>
        /// return number of skipped rows
        /// to find the first data, skip al rows that are empty
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static int Skipped(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName), "fileName not exist");


            int count = 0;
            if (File.Exists(fileName))
            {
                using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(file))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) count++;
                            else break;
                        }
                    }
                }

            }
            else
            {
                throw new FileNotFoundException("file not found");
            }

            return count;
        }

        private const char CR = '\r';
        private const char LF = '\n';
        private const char NULL = (char)0;

        private static long countLinesMaybe(Stream stream)
        {
            var lineCount = 0L;

            var byteBuffer = new byte[1024 * 1024];
            const int BytesAtTheTime = 4;
            var detectedEOL = NULL;
            var currentChar = NULL;

            int bytesRead;
            while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
            {
                var i = 0;
                for (; i <= bytesRead - BytesAtTheTime; i += BytesAtTheTime)
                {
                    currentChar = (char)byteBuffer[i];

                    if (detectedEOL != NULL)
                    {
                        if (currentChar == detectedEOL) { lineCount++; }

                        currentChar = (char)byteBuffer[i + 1];
                        if (currentChar == detectedEOL) { lineCount++; }

                        currentChar = (char)byteBuffer[i + 2];
                        if (currentChar == detectedEOL) { lineCount++; }

                        currentChar = (char)byteBuffer[i + 3];
                        if (currentChar == detectedEOL) { lineCount++; }
                    }
                    else
                    {
                        if (currentChar == LF || currentChar == CR)
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                        i -= BytesAtTheTime - 1;
                    }
                }

                for (; i < bytesRead; i++)
                {
                    currentChar = (char)byteBuffer[i];

                    if (detectedEOL != NULL)
                    {
                        if (currentChar == detectedEOL) { lineCount++; }
                    }
                    else
                    {
                        if (currentChar == LF || currentChar == CR)
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                    }
                }
            }

            if (currentChar != LF && currentChar != CR && currentChar != NULL)
            {
                lineCount++;
            }
            return lineCount;
        }


        public static List<string> GetRows(string fileName, int number=0)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName), "fileName not exist");
     
            List<string> selectedRows = new List<string>();
            if (File.Exists(fileName))
            {
                using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {

                    using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
                    {
                        string line = "";
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                selectedRows.Add(line);
                                if (selectedRows.Count == number) break;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException("file not found");
            }

            return selectedRows;
        }

        /// <summary>
        /// get rows by index starting from first detected row- skipped are not counted
        /// get a subset of the row with a list of active cells 
        /// cells list must same lenght as row afer split with text seperator
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="indexList"></param>
        /// <param name="activeCells"></param>
        /// <param name="delimeter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static List<string> GetRows(string fileName, List<int> indexList, List<bool> activeCells=null, TextSeperator delimeter = TextSeperator.tab)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName), "fileName not exist");
            if (indexList == null || !indexList.Any()) throw new ArgumentNullException(nameof(fileName), "row index list is empty");

            List<string> selectedRows = new List<string>();

            if (File.Exists(fileName))
            {
                using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {

                    using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
                    {
                        // skipp all empty rows
                        string line;
                        int index = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                

                                // check if index is in indexList
                                if (indexList.Contains(index))
                                {
                                    selectedRows.Add(getSubsetOfLine(line, activeCells, delimeter));
                                }

                                // if selectedRows count == indexList, all rows are found, break while loop
                                if(indexList.Count== selectedRows.Count) break;

                                // count only rows they have data
                                index++;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException("file not found");
            }

            return selectedRows;
        }

        private static string getSubsetOfLine(string line, List<bool> activeCells, TextSeperator delimeter)
        {
            // if cell list exist and entry are false, means 
            // from the row we want a subset of cells
            if (activeCells != null && activeCells.Any())
            {
                #region subset of row

                List<string> subset = new List<string>();
                int cellCount = activeCells.Where(v=>v==true).ToList().Count;

                char d = AsciiFileReaderInfo.GetSeperator(delimeter);
                string[] cells = line.Split(d);
                int rowSplit = cells.Length;

                // compare cell count with length with row split by text seperator
                if (cellCount < rowSplit) // number is differ, create a subset
                {
                    for (int i = 0; i < activeCells.Count; i++)
                    {
                        if (activeCells[i]) subset.Add(cells[i]);
                    }

                    return String.Join("" + d, subset.ToArray());
                }
                else // both have the same lenght
                {
                    return line;
                }

                
                #endregion

            }
            else // full row is wanted
            {
                return line;
            }
        }

        public static List<string> GetRandowRows(string fileName, long total, long selection, long dataStart=0)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName), "fileName not exist");
            if (total==0) throw new Exception("total can not be 0");
            if (selection == 0) throw new Exception("selection can not be 0");
            if (total< selection) throw new Exception("total must be greater then selection");

            List<long> selectedRowsIndex = new List<long>();
            List<string> selectedRows = new List<string>();
            Random rand = new Random();

            // select row index randomly
            for (int i = 0; i < selection; i++)
            {
                long c = 0;
                do // run random index till int not exist in the selectRowsIndex
                {
                    c = Convert.ToInt64(rand.Next(Convert.ToInt32(dataStart), Convert.ToInt32(total)+1));
                }
                while (selectedRowsIndex.Contains(c));

                selectedRowsIndex.Add(c);
            }

            if (File.Exists(fileName))
            {
                using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    var lineCount = 1;

                    using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
                    {
                        string line = "";
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line) && selectedRowsIndex.Contains(lineCount))
                            {
                                selectedRows.Add(line);
                            }
                            lineCount++;
                        }
                    }
                }
            }
            else
            { 
                throw new FileNotFoundException("file not found");
            }

            return selectedRows;
        }

        


        #endregion

        #region helper methods

        List<string> tempRow = new List<string>();
        /// <summary>
        /// Convert a row as a string to a list of strings
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="line">Row as a string</param>
        /// <param name="seperator">Character used as TextSeparator</param>
        /// <returns>List of values</returns>
        public List<string> rowToList(string line, char seperator)
        {
            if (this.Info != null)
            {
                if (fileReaderInfo == null) fileReaderInfo = (AsciiFileReaderInfo)this.Info;

                if (fileReaderInfo != null)
                {
                    tempRow = new List<string>();
                    tempRow = TextMarkerHandling(line, seperator, AsciiFileReaderInfo.GetTextMarker(fileReaderInfo.TextMarker));

                    //if a offset is marked in the filereaader informations the offset needs to skip from the complete string array
                    return tempRow.Skip(fileReaderInfo.Offset).ToList();
                }

            }
            return line.Split(seperator).ToList();
        }


        List<string> values;
        List<string> temp;
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
        public List<string> TextMarkerHandling(string row, char separator, char textmarker)
        {
            values = row.Split(separator).ToList();

            /// <summary>
            /// check if the row contains a textmarker
            /// </summary>
            /// <remarks></remarks>
            if (row.Contains(textmarker))
            {
                string tempValue = "";
                bool startText = false;

                temp = new List<string>();

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
                        //if the qoutest are in one value - first and last character
                        if (v.ToCharArray().First().Equals(textmarker) && v.ToCharArray().Last().Equals(textmarker))
                        {
                            temp.Add(v.Trim(textmarker));
                        }
                        else
                        {
                            if (v.ToCharArray().First().Equals(textmarker))
                            {
                                tempValue = v;
                                startText = true;
                            }

                            if (v.ToCharArray().Last().Equals(textmarker))
                            {
                                tempValue += separator + v;
                                temp.Add(tempValue.Trim(textmarker));
                                startText = false;
                            }
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

        private bool isEmpty(string line, char seperator)
        {
            string tmp = line.Replace(seperator, ' ');

            if (string.IsNullOrWhiteSpace(tmp))
            {
                NumberOSkippedfRows++;
                Debug.WriteLine(tmp);
                Debug.WriteLine("NumberOSkippedfRows" + NumberOSkippedfRows);
                return true;
            }

            return false;
        }

  
        #endregion helper methods

        #region encoding

        private void setEncoding(string path)
        {
            using (var reader = new StreamReader(path, Encoding.Default, true))
            {
                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                encoding = reader.CurrentEncoding;
                reader.Close();
            }

        }

        #endregion
    }
}