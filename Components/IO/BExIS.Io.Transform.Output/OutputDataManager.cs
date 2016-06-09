using System;
using System.Collections.Generic;
using System.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using System.IO;
using System.Linq.Expressions;
using Vaiona.Utils.Cfg;
using System.Data;
using System.Globalization;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.IO.Transform.Output
{
    public class OutputDataManager
    {
        #region export prepare files

        public string GenerateAsciiFile(long id, string title, string mimeType)
        {
            string contentDescriptorTitle = "";
            string ext = "";

            switch (mimeType)
            {
                case "text/csv":
                {
                        contentDescriptorTitle = "generatedCSV";
                        ext = ".csv";
                        break;
                }
                default:
                {
                        contentDescriptorTitle = "generatedTXT";
                        ext = ".txt";
                        break;
                }
            }


            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
            AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
            
            string path = "";

            List<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
            long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

            //ascii allready exist
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals("c")) > 0)
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

                    path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                    storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);

                    writer.AddDataTuples(datatupleIds, path, datastuctureId);

                    return path;
                }

                #endregion

            }
            // not exist needs to generated
            else
            {
                #region FileStream not exist

                path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);

                writer.AddDataTuples(datatupleIds, path, datastuctureId);

                return path;

                #endregion
            }
        }

        public string GenerateAsciiFile(long id, string title, string mimeType, string[] visibleColumns)
        {
            string ext = "";
            string path = "";

            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
            AsciiWriter writer = new AsciiWriter(TextSeperator.comma);

            List<AbstractTuple> datatuples = new List<AbstractTuple>(); //GetFilteredDataTuples(datasetVersion);

            long datastuctureId = datasetVersion.Dataset.DataStructure.Id;

            path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

            if (visibleColumns != null)
                writer.VisibleColumns = visibleColumns;

            writer.AddDataTuples(datatuples, path, datastuctureId);

            return path;
        }

        public string GenerateExcelFile(long id, string title)
        {
            string mimeType = "application / xlsm";
            string contentDescriptorTitle = "generated";
            string ext = ".xlsm";


            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
            ExcelWriter writer = new ExcelWriter();

            string path = "";

            //excel allready exist
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals("generated")) > 0)
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

                    storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);
                    writer.AddDataTuplesToTemplate(datatupleIds, path, datastuctureId);

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
                path = generateDownloadFile(id, datasetVersion.Id, datastuctureId, title, ext, writer);

                storeGeneratedFilePathToContentDiscriptor(id, datasetVersion, title, ext, writer);
                writer.AddDataTuplesToTemplate(datatupleIds, path, datastuctureId);

                return path;

                #endregion
            }

            return "";
        }

        private string generateDownloadFile(long id, long datasetVersionOrderNo, long dataStructureId, string title, string ext, DataWriter writer)
        {
            if (ext.Equals(".csv") || ext.Equals(".txt"))
            {
                AsciiWriter asciiwriter = (AsciiWriter)writer;
                return asciiwriter.CreateFile(id, datasetVersionOrderNo, dataStructureId, title, ext);
            }
            else
            if (ext.Equals(".xlsm"))
            {
                ExcelWriter excelwriter = (ExcelWriter)writer;
                return excelwriter.CreateFile(id, datasetVersionOrderNo, dataStructureId, title, ext);
            }

            return "";
        }

        private void storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext, DataWriter writer)
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
            string dynamicPath = writer.GetDynamicStorePath(datasetId, datasetVersion.Id, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            DatasetManager dm = new DatasetManager();
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

        #endregion

        #region datatable

        public static DataTable ConvertPrimaryDataToDatatable(DatasetVersion dsv, IEnumerable<long> dsVersionTupleIds,string tableName="", bool useLabelsAsColumnNames=false)
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(tableName))
                dt.TableName = "Primary data table";
            else
                dt.TableName = tableName;
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

            if (dsVersionTupleIds != null && sds != null)
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

                DatasetManager datasetManager = new DatasetManager();

                foreach (var id in dsVersionTupleIds)
                {
                    DataTuple dataTuple = datasetManager.DataTupleRepo.Query(d => d.Id.Equals(id)).FirstOrDefault();
                    dataTuple.Materialize();
                    dt.Rows.Add(ConvertTupleIntoDataRow(dt, dataTuple, sds, true));
                }
            }

            return dt;
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
                columnName = useLabelsAsColumnName == true? vv.Variable.Label :  "ID" + vv.VariableId.ToString();

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
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataColumn dc = dt.Columns[i];
                if (!visibleColumns.Contains(dc.Caption))
                {
                    dt.Columns.Remove(dc);
                }
            }

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

        #endregion
    }
}
