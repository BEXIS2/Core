using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Validation.DSValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Vaiona.Utils.Cfg;

/// <summary>
///
/// </summary>        
namespace BExIS.IO.Transform.Output
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class AsciiWriter : DataWriter
    {
        #region constructor

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public TextSeperator Delimeter { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public AsciiWriter()
        {
            Delimeter = TextSeperator.comma;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="delimeter"></param>
        public AsciiWriter(TextSeperator delimeter) : base()
        {
            Delimeter = delimeter;
        }

        #endregion

        #region constants
        private static char[] specialChars = new char[] { '"', ',' };
        #endregion

        #region instance variables
        // filepath to the output file
        private string filepath;
        // file content
        private StringBuilder data;
        // separator
        string separator;
        #endregion

        #region setup / close actions
        protected override void Init(string file, long dataStructureId)
        {
            // store pointer to dataStructure
            dataStructure = GetDataStructure(dataStructureId);

            // create the file
            CreateFile(file);

            // save the path
            this.filepath = file;

            // reset instance variables
            this.data = new StringBuilder();
            this.separator = Char.ToString(AsciiHelper.GetSeperator(Delimeter));
        }

        protected override void Close()
        {
            // write content to file
            File.WriteAllText(this.filepath, data.ToString());

            // reset
            this.filepath = "";
            this.data = null;
            this.separator = null;
        }
        #endregion

        #region addHeader
        protected override bool AddHeader(StructuredDataStructure ds)
        {
            if (ds.Variables != null && ds.Variables.Any())
            {
                List<Variable> variables = ds.Variables.ToList();

                if (VisibleColumns != null)
                {
                    variables = GetSubsetOfVariables(ds.Variables.ToList(), VisibleColumns);
                }

                variables = variables.OrderBy(v => v.OrderNo).ToList();

                // copy header titles to array
                string[] line = new string[variables.Count];

                //foreach (Variable v in variables)
                for (int i = 0; i < variables.Count; i++)
                {
                    // get value
                    string value = variables[i].Label.ToString();

                    // add value
                    line[i] = escapeValue(value);

                    // add to variable identifiers
                    this.VariableIdentifiers.Add
                        (
                            new VariableIdentifier
                            {
                                id = variables[i].Id,
                                name = variables[i].Label,
                                systemType = variables[i].DataAttribute.DataType.SystemType
                            }
                        );
                }

                // add to data collection
                data.AppendLine(String.Join(this.separator, line));

                return true;
            }

            return false;
        }

        protected override bool AddHeader(DataColumnCollection columns)
        {
            // number of columns
            int colCount = columns.Count;
            // content of one line
            string[] line = new string[colCount];

            // append header
            for (int i = 0; i < colCount; i++)
            {
                line[i] = escapeValue(columns[i].Caption);
            }
            data.AppendLine(String.Join(this.separator, line));

            return true;
        }
        #endregion

        #region addRow
        protected override bool AddRow(AbstractTuple tuple, long rowIndex)
        {
            // number of columns
            int colCount = this.VariableIdentifiers.Count;
            // content of one line
            string[] line = new string[colCount];

            string value;
            for (int i = 0; i < this.VariableIdentifiers.Count; i++)
            {
                // shortcut
                var vi = this.VariableIdentifiers[i];

                Variable variable = dataStructure.Variables.Where(p => p.Id == vi.id).SingleOrDefault();

                if (variable != null)
                {
                    Dlm.Entities.DataStructure.DataType dataType = variable.DataAttribute.DataType;

                    VariableValue vv = tuple.VariableValues.Where(v => v.VariableId.Equals(vi.id)).FirstOrDefault();

                    if (vv != null && vv.Value != null)
                    {
                        string format = GetStringFormat(dataType);
                        if (!string.IsNullOrEmpty(format))
                        {
                            value = GetFormatedValue(vv.Value, dataType, format);
                        }
                        else value = vv.Value.ToString();

                        // Add value to row
                        line[i] = escapeValue(value);

                    }

                }
            }

            // add line to result
            data.AppendLine(String.Join(this.separator, line));

            return true;
        }

        protected override bool AddRow(DataRow row, long rowIndex)
        {
            // number of columns
            int colCount = row.Table.Columns.Count;
            // content of one line
            string[] line = new string[colCount];

            // append contents
            for (int i = 0; i < colCount; i++)
            {
                // get value as string
                string value = row[i].ToString();

                // add value to row
                line[i] = escapeValue(value);

            }

            // Add to result
            data.AppendLine(String.Join(this.separator, line));

            return true;
        }
        #endregion

        #region generic

        public static bool AllTextToFile(string filepath, string text)
        {
            try
            {
                File.WriteAllText(filepath, text);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region helper
        private string escapeValue(string value)
        {
            // modify if special characters are present
            if (value.IndexOfAny(specialChars) != -1)
            {
                value = "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
        #endregion

        #region bexis internal usage

        /// <summary>
        /// create a new empty file at the given path
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static new string CreateFile(string filepath)
        {
            // method is needed as C# does not support static method inheritance

            string dataPath = Path.Combine(AppConfiguration.DataPath, filepath);

            try
            {
                if (!File.Exists(dataPath))
                {
                    File.Create(dataPath).Close();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message.ToString();
            }

            return dataPath;
        }

        /// <summary>
        /// return the filepath
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="dataStructureId"></param>
        /// <param name="title"></param>
        /// <param name="extention"></param>
        public string CreateFile(long datasetId, long datasetVersionOrderNr, long dataStructureId, string title, string extention)
        {
            string dataPath = GetFullStorePath(datasetId, datasetVersionOrderNr, title, extention);

            try
            {
                if (!File.Exists(dataPath))
                {
                    File.Create(dataPath).Close();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message.ToString();
            }

            return dataPath;
        }


        /// <summary>
        /// create a new file in the given namespace and return the full path
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="v"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        internal string CreateFile(string ns, string title, string extension)
        {
            string dataPath = GetFullStorePath(ns, title, extension);

            try
            {
                if (!File.Exists(dataPath))
                {
                    File.Create(dataPath).Close();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message.ToString();
            }

            return dataPath;
        }

        #endregion

    }

}