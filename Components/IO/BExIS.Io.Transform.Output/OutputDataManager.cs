using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Transform.Output
{
    public class OutputDataManager
    {
        #region export prepare files

        public string GenerateAsciiFile(long id, string title, string mimeType)
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                long datasetVersionId = datasetVersion.Id;

                return GenerateAsciiFile(id, datasetVersionId, title, mimeType);
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        public string GenerateAsciiFile(long id, long versionId, string title, string mimeType)
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(versionId);

                string contentDescriptorTitle = "";
                string ext = "";

                TextSeperator textSeperator = TextSeperator.semicolon;

                switch (mimeType)
                {
                    case "text/csv":
                        {
                            contentDescriptorTitle = "generatedCSV";
                            ext = ".csv";
                            textSeperator = TextSeperator.semicolon;
                            break;
                        }
                    default:
                        {
                            contentDescriptorTitle = "generatedTXT";
                            ext = ".txt";
                            textSeperator = TextSeperator.tab;
                            break;
                        }
                }

                AsciiWriter writer = new AsciiWriter(textSeperator);

                string path = "";

                //ascii allready exist
                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(contentDescriptorTitle) && p.URI.Contains(datasetVersion.Id.ToString())) > 0)
                {
                    #region FileStream exist

                    ContentDescriptor contentdescriptor = datasetVersion.ContentDescriptors.Where(p => p.Name.Equals(contentDescriptorTitle)).FirstOrDefault();
                    path = Path.Combine(AppConfiguration.DataPath, contentdescriptor.URI);

                    if (FileHelper.FileExist(path))
                    {
                        return path;
                    }
                    else
                    {
                        List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                        long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                        path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, "Data", ext, writer);

                        storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, ext);

                        writer.AddDataTuples(datasetManager, datatupleIds, path, datastuctureId);

                        return path;
                    }

                    #endregion

                }
                // not exist needs to generated
                else
                {
                    #region FileStream not exist

                    List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                    long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                    path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, "data", ext, writer);

                    storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, ext);

                    writer.AddDataTuples(datasetManager, datatupleIds, path, datastuctureId);

                    return path;

                    #endregion
                }
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        public string GenerateAsciiFile(string ns, DataTable table, string title, string mimeType, long dataStructureId)
        {
            string ext = "";
            TextSeperator textSeperator = TextSeperator.semicolon;

            switch (mimeType)
            {
                case "text/csv":
                    {
                        ext = ".csv";
                        textSeperator = TextSeperator.semicolon;
                        break;
                    }
                default:
                    {
                        ext = ".txt";
                        textSeperator = TextSeperator.tab;
                        break;
                    }
            }

            AsciiWriter writer = new AsciiWriter(textSeperator);

            // write to file
            // if there is already a file, replace it
            string path = generateDownloadFile(ns, dataStructureId, title, ext, writer);

            writer.AddDataTuples(table, path, dataStructureId);

            return path;

        }

        public string GenerateAsciiFile(long id, string title, string mimeType, string[] visibleColumns)
        {
            string ext = "";
            string path = "";

            DatasetManager datasetManager = new DatasetManager();

            try
            {

                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                AsciiWriter writer = new AsciiWriter(TextSeperator.comma);

                // Javad: It is better to have a list of tuple IDs and pass it to the AddDataTuples method. 
                // This method is using a special iterator to reduce the number of queries. 18.11.2016

                List<long> datatuples = new List<long>(); //GetFilteredDataTuples(datasetVersion);

                long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

                path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, "data", ext, writer);

                if (visibleColumns != null)
                    writer.VisibleColumns = visibleColumns;

                writer.AddDataTuples(datasetManager, datatuples, path, datastuctureId);

                return path;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        /// <summary>
        /// create an Excel file from the given Datatable
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public string GenerateExcelFile(string ns, DataTable table, string title, long dsId)
        {
            ExcelWriter writer = new ExcelWriter();

            string[] columnNames = table.Columns.Cast<DataColumn>()
                                 .Select(x => x.Caption)
                                 .ToArray();

            //get variable ids of the datatable columns
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dsId);

     
            string path = generateDownloadFile(ns, dsId, title, ".xlsm", writer, columnNames);
            writer.AddDataTuplesToFile(table, path, dsId);

            return path;
        }

        public string GenerateExcelFile(long id, string title)
        {
            string mimeType = "application / xlsm";
            string contentDescriptorTitle = "generated";
            string ext = ".xlsm";


            DatasetManager datasetManager = new DatasetManager();


            try
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                ExcelWriter writer = new ExcelWriter();

                string path = "";

                //excel allready exist
                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals("generated") && p.URI.Contains(datasetVersion.Id.ToString())) > 0)
                {
                    #region FileStream exist

                    ContentDescriptor contentdescriptor =
                        datasetVersion.ContentDescriptors.Where(p => p.Name.Equals("generated"))
                            .FirstOrDefault();
                    path = Path.Combine(AppConfiguration.DataPath, contentdescriptor.URI);

                    long version = datasetVersion.Id;
                    long versionNrGeneratedFile =
                        Convert.ToInt64(contentdescriptor.URI.Split('\\').Last().Split('_')[1]);

                    // check if FileStream exist
                    if (FileHelper.FileExist(path) && version == versionNrGeneratedFile)
                    {
                        return path;
                    }

                    // if not generate
                    else
                    {
                        List<long> datatupleIds =
                            datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                        long datastuctureId = datasetVersion.Dataset.DataStructure.Id;
                        path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext,
                            writer);

                        storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, ext);
                        writer.AddDataTuplesToTemplate(datasetManager, datatupleIds, path, datastuctureId);

                        return path;
                    }

                    #endregion
                }
                // not exist needs to generated
                else
                {
                    #region FileStream not exist

                    List<long> datatupleIds =
                        datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                    long datastuctureId = datasetVersion.Dataset.DataStructure.Id;
                    path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, "data", ext, writer);

                    storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, ext);
                    writer.AddDataTuplesToTemplate(datasetManager, datatupleIds, path, datastuctureId);

                    return path;

                    #endregion
                }

                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        private string generateDownloadFile(long id, long datasetVersionOrderNo, long dataStructureId, string title, string ext, DataWriter writer)
        {
            if (ext.Equals(".csv") || ext.Equals(".txt"))
            {
                AsciiWriter asciiwriter = (AsciiWriter)writer;
                return asciiwriter.CreateFile(id, datasetVersionOrderNo, dataStructureId, "data", ext);
            }
            else
            if (ext.Equals(".xlsm"))
            {
                ExcelWriter excelwriter = (ExcelWriter)writer;
                return excelwriter.CreateFile(id, datasetVersionOrderNo, dataStructureId, "data", ext);
            }

            return "";
        }


        /// <summary>
        /// create a new file of given extension
        /// ns will be used to seperate from other files created of individual datasets
        /// </summary>
        /// <param name="ns">namespace</param>
        /// <param name="title"></param>
        /// <param name="ext"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private string generateDownloadFile(string ns, long datastructureId, string title, string ext, DataWriter writer, string[] columns = null)
        {
            switch (ext)
            {
                // text based files
                case ".csv":
                case ".txt":
                    AsciiWriter asciiwriter = (AsciiWriter)writer;
                    return asciiwriter.CreateFile(ns, title, ext);

                // excel files
                case ".xlsx":
                case ".xlsm":
                    ExcelWriter excelwriter = (ExcelWriter)writer;
                    excelwriter.VisibleColumns = columns;
                    return excelwriter.CreateFile(ns, datastructureId, title, ext);

                // no valid extension given
                default:
                    return "";
            }
        }

        private void storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string ext)
        {

            DatasetManager dm = new DatasetManager();
            try
            {


                string name = "";
                string mimeType = "";

                if (ext.Contains("csv"))
                {
                    name = "generatedCSV";
                    mimeType = "text/csv";
                }

                if (ext.Contains("txt"))
                {
                    name = "generatedTXT";
                    mimeType = "text/plain";
                }

                if (ext.Contains("xlsm"))
                {
                    name = "generated";
                    mimeType = "application/xlsm";
                }

                // create the generated FileStream and determine its location
                string dynamicPath = IOHelper.GetDynamicStorePath(datasetId, datasetVersion.Id, "data", ext);
                //Register the generated data FileStream as a resource of the current dataset version
                //ContentDescriptor generatedDescriptor = new ContentDescriptor()
                //{
                //    OrderNo = 1,
                //    Name = name,
                //    MimeType = mimeType,
                //    URI = dynamicPath,
                //    DatasetVersion = datasetVersion,
                //};

                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
                {   // remove the one contentdesciptor 
                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        if (cd.Name == name)
                        {
                            cd.URI = dynamicPath;
                            dm.UpdateContentDescriptor(cd);
                        }
                    }
                }
                else
                {
                    // add current contentdesciptor to list
                    //datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                    dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
                }

                //dm.EditDatasetVersion(datasetVersion, null, null, null);
            }
            finally
            {
                dm.Dispose();
            }

        }

        #endregion

        #region datatable

        //[MeasurePerformance]
        public static DataTable ConvertPrimaryDataToDatatable(DatasetManager datasetManager, DatasetVersion datasetVersion, string tableName = "", bool useLabelsAsColumnNames = false)
        {

            DataStructureManager dsm = new DataStructureManager();

            try
            {

                DataTable dt = new DataTable();
                if (string.IsNullOrEmpty(tableName))
                    dt.TableName = "Primary data table";
                else
                    dt.TableName = tableName;
                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
                var tupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);

                if (tupleIds != null && tupleIds.Count > 0 && sds != null)
                {
                    buildTheHeader(sds, useLabelsAsColumnNames, dt);
                    buildTheBody(datasetManager, tupleIds, dt, sds, useLabelsAsColumnNames);
                }

                return dt;
            }
            finally
            {
                dsm.Dispose();
            }
        }

        private static void buildTheBody(DatasetManager datasetManager, List<long> tupleIds, DataTable dt, StructuredDataStructure sds, bool useLabelsAsColumnNames)
        {
            DataTupleIterator tupleIterator = new DataTupleIterator(tupleIds, datasetManager);
            foreach (var tuple in tupleIterator)
            {
                dt.Rows.Add(ConvertTupleIntoDataRow(dt, tuple, sds, useLabelsAsColumnNames));
            }
        }

        private static void buildTheHeader(StructuredDataStructure sds, bool useLabelsAsColumnNames, DataTable dt)
        {
            foreach (var vu in sds.Variables)
            {
                DataColumn col = null;
                if (useLabelsAsColumnNames)
                {
                    col = dt.Columns.Add(vu.Label);
                }
                else
                {
                    col = dt.Columns.Add("ID" + vu.Id.ToString()); // or DisplayName also
                }

                col.Caption = vu.Label;

                switch (vu.DataAttribute.DataType.SystemType)
                {
                    case "String":
                        {
                            col.DataType = Type.GetType("System.String");
                            break;
                        }

                    case "Double":
                        {
                            col.DataType = Type.GetType("System.Double");
                            break;
                        }

                    case "Int16":
                        {
                            col.DataType = Type.GetType("System.Int16");
                            break;
                        }

                    case "Int32":
                        {
                            col.DataType = Type.GetType("System.Int32");
                            break;
                        }

                    case "Int64":
                        {
                            col.DataType = Type.GetType("System.Int64");
                            break;
                        }

                    case "Decimal":
                        {
                            col.DataType = Type.GetType("System.Decimal");
                            break;
                        }

                    case "DateTime":
                        {
                            col.DataType = Type.GetType("System.DateTime");
                            break;
                        }

                    default:
                        {
                            col.DataType = Type.GetType("System.String");
                            break;
                        }
                }

                if (vu.Parameters.Count > 0)
                {
                    foreach (var pu in vu.Parameters)
                    {
                        DataColumn col2 = dt.Columns.Add(pu.Label.Replace(" ", "")); // or DisplayName also
                        col2.Caption = pu.Label;

                    }
                }
            }
        }

        /// <summary>
        /// This function convert a datatuple into datarow for a datatable to show on the client side
        /// the grid in the client side (in client mode) has unknow problem with value 0 and null
        /// So every empty cell get the max value of the specific Systemtype.
        /// On the client side this values replaced with ""
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static DataRow ConvertTupleIntoDataRow(DataTable dt, AbstractTuple t, StructuredDataStructure sts, bool useLabelsAsColumnName = false)
        {
            DataRow dr = dt.NewRow();
            string columnName = "";
            foreach (var vv in t.VariableValues)
            {
                Variable variable = sts.Variables.FirstOrDefault(v => v.Id.Equals(vv.VariableId));

                columnName = useLabelsAsColumnName == true ? variable.Label : "ID" + vv.VariableId.ToString();

                if (vv.VariableId > 0)
                {
                    string valueAsString = "";
                    if (vv.Value == null)
                    {
                        dr[columnName] = DBNull.Value;
                    }
                    else
                    {
                        valueAsString = vv.Value.ToString();

                        Dlm.Entities.DataStructure.Variable varr = sts.Variables.Where(p => p.Id == vv.VariableId).SingleOrDefault();
                        switch (varr.DataAttribute.DataType.SystemType)
                        {
                            case "String":
                                {
                                    dr[columnName] = valueAsString;

                                    break;
                                }

                            case "Double":
                                {
                                    double value;
                                    if (double.TryParse(valueAsString, out value))
                                        dr[columnName] = Convert.ToDouble(valueAsString);
                                    else
                                        dr[columnName] = -99999;//double.MaxValue;
                                    break;
                                }

                            case "Int16":
                                {
                                    Int16 value;
                                    if (Int16.TryParse(valueAsString, out value))
                                        dr[columnName] = Convert.ToInt16(valueAsString);
                                    else
                                        dr[columnName] = Int16.MaxValue;
                                    break;
                                }

                            case "Int32":
                                {
                                    Int32 value;
                                    if (Int32.TryParse(valueAsString, out value))
                                        dr[columnName] = Convert.ToInt32(valueAsString);
                                    else
                                        dr[columnName] = Int32.MaxValue;
                                    break;
                                }

                            case "Int64":
                                {
                                    Int64 value;
                                    if (Int64.TryParse(valueAsString, out value))
                                        dr[columnName] = Convert.ToInt64(valueAsString);
                                    else
                                        dr[columnName] = Int64.MaxValue;
                                    break;
                                }

                            case "Decimal":
                                {
                                    decimal value;
                                    if (decimal.TryParse(valueAsString, out value))
                                        dr[columnName] = Convert.ToDecimal(valueAsString);
                                    else
                                        dr[columnName] = -99999;//decimal.MaxValue;
                                    break;
                                }

                            case "Float":
                                {
                                    decimal value;
                                    if (decimal.TryParse(valueAsString, out value))
                                        dr[columnName] = Convert.ToDecimal(valueAsString);
                                    else
                                        dr[columnName] = -99999;
                                    break;
                                }

                            case "DateTime":
                                {
                                    if (!String.IsNullOrEmpty(valueAsString))
                                        dr[columnName] = Convert.ToDateTime(valueAsString, CultureInfo.InvariantCulture);
                                    else
                                        dr[columnName] = DateTime.MaxValue;

                                    break;
                                }

                            default:
                                {
                                    if (!String.IsNullOrEmpty(vv.Value.ToString()))
                                        dr[columnName] = valueAsString;
                                    else
                                        dr[columnName] = DBNull.Value;

                                    break;
                                }
                        }
                    }



                    /*if (vv.ParameterValues.Count > 0)
                    {
                        foreach (var pu in vv.ParameterValues)
                        {
                            dr[pu.Parameter.Label.Replace(" ", "")] = pu.Value;
                        }
                    }*/
                }
            }

            return dr;
        }

        public static DataTable ProjectionOnDataTable(DataTable dt, string[] visibleColumns)
        {
            List<DataColumn> columnTobeDeleted = new List<DataColumn>();
            foreach (DataColumn column in dt.Columns)
            {
                if (!visibleColumns.Contains(column.ColumnName.ToUpper()))
                {
                    columnTobeDeleted.Add(column);
                }
            }
            columnTobeDeleted.ForEach(p => dt.Columns.Remove(p));
            return dt;
        }

        public static DataTable SelectionOnDataTable(DataTable dt, string selection)
        {
            DataTable newDt = dt.Clone();
            DataRow[] rows = dt.Select(selection);
            foreach (var row in rows)
            {
                newDt.ImportRow(row);
            }


            return newDt;
        }

        public static void ClearTempDirectory()
        {
            string path = Path.Combine(AppConfiguration.DataPath,"Datasets", "Temp");
            if (Directory.Exists(path))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
        }

        #endregion
    }
}
