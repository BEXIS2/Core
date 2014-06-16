using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Io.Transform.Validation.Exceptions;

namespace BExIS.Io.Transform.Output
{
    public class AsciiWriter:DataWriter
    {
        public TextSeperator Delimeter { get; set; }


        public AsciiWriter()
        {
            Delimeter = TextSeperator.comma;
        }

        public AsciiWriter(TextSeperator delimeter)
        {
            Delimeter = delimeter;
        }

        public string CreateFile(long datasetId, long datasetVersionOrderNr, long dataStructureId, string title, string extention)
        {
            string dataPath = GetStorePath(datasetId, datasetVersionOrderNr, title, extention);

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
        /// Add Datatuples and Datastructure to a Ascii file
        /// </summary>
        /// <param name="dataTuples"> Datatuples to add</param>
        /// <param name="filePath">Path of the excel template file</param>
        /// <param name="dataStructureId">Id of datastructure</param>
        /// <returns>List of Errors or null</returns>
        public List<Error> AddDataTuples(List<long> dataTuplesIds, string filePath, long dataStructureId)
        {
            if (File.Exists(filePath))
            {
                StringBuilder data = new StringBuilder();

                data.AppendLine(DataStructureToRow(dataStructureId));

                foreach (long id in dataTuplesIds)
                {
                    data.AppendLine(DatatupleToRow(id));
                }


                File.WriteAllText(filePath, data.ToString());
            }

            return errorMessages;
        }

        public List<Error> AddDataTuples(List<AbstractTuple> dataTuples, string filePath, long dataStructureId)
        {
            if (File.Exists(filePath))
            {
                StringBuilder data = new StringBuilder();

                data.AppendLine(DataStructureToRow(dataStructureId));

                foreach (AbstractTuple dataTuple in dataTuples)
                {
                    data.AppendLine(DatatupleToRow(dataTuple));
                }


                File.WriteAllText(filePath, data.ToString());
            }

            return errorMessages;
        }

        /// <summary>
        /// Convert Datatuple to  String line
        /// </summary>
        /// <param name="id">Id of the Datatuple</param>
        /// <returns></returns>
        private string DatatupleToRow(long id)
        {
            //DatatupleManager
            DatasetManager datasetManager = new DatasetManager();
            
            // I do not know where this function is called, but there is a chance that the id is referring to a tuple in a previous version, in that case, the tuple is not in the data tuples anymore. Javad
            DataTuple dataTuple = datasetManager.DataTupleRepo.Get(id);
            dataTuple.Materialize();

            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (VariableValue vv in dataTuple.VariableValues)
            {
                string value ="";
                if(vv.Value!=null)
                    value =  vv.Value.ToString();
                // Add separator if this isn't the first value
                if (!first)
                    builder.Append(AsciiHelper.GetSeperator(Delimeter));
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                first = false;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Convert Datatuple to  String line
        /// </summary>
        /// <param name="dataTuple"></param>
        /// <returns></returns>
        private string DatatupleToRow(AbstractTuple dataTuple)
        {
            StringBuilder builder = new StringBuilder();
            bool first = true;

            List<VariableValue> variableValues = dataTuple.VariableValues.ToList();

            if (visibleColumns != null)
            {
                variableValues = GetSubsetOfVariableValues(variableValues, visibleColumns);
            }

            foreach (VariableValue vv in variableValues)
            {
                string value = "";
                if (vv.Value != null)
                    value = vv.Value.ToString();
                // Add separator if this isn't the first value
                if (!first)
                    builder.Append(AsciiHelper.GetSeperator(Delimeter));
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                first = false;
            }

            return builder.ToString();
        }
        
        /// <summary>
        /// Convert Datastructure to a String line
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string DataStructureToRow(long id)
        {
            StructuredDataStructure ds = GetDataStructure(id);
            StringBuilder builder = new StringBuilder();
            bool first = true;

            List<Variable> variables = ds.Variables.ToList();

            if (visibleColumns != null)
            {
                variables = GetSubsetOfVariables(ds.Variables.ToList(), visibleColumns);
            }

            foreach (Variable v in variables)
            {
                string value = v.Label.ToString();
                // Add separator if this isn't the first value
                if (!first)
                    builder.Append(AsciiHelper.GetSeperator(Delimeter));
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                first = false;
            }

            return builder.ToString();
        }

        
    }


}
