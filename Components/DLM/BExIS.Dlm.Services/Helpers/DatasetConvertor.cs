using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dlm.Services.Helpers
{
    public class DatasetConvertor
    {
        public DataTable ConvertDatasetVersion(IEnumerable<AbstractTuple> tupleIterator, StructuredDataStructure dataStructure, string tableName = "", bool useLabelsAsColumnNames = false)
        {
            return ConvertPrimaryDataToDatatable(tupleIterator, dataStructure, tableName, useLabelsAsColumnNames);
        }

        public DataTable ConvertDatasetVersion(DatasetManager datasetManager, DatasetVersion datasetVersion, string tableName = "", bool useLabelsAsColumnNames = false)
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(tableName))
                dt.TableName = "Primary data table";
            else
                dt.TableName = tableName;
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
            var tupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);

            if (tupleIds != null && tupleIds.Count > 0 && sds != null)
            {
                buildTheHeader(sds, useLabelsAsColumnNames, dt);
                buildTheBody(datasetManager, tupleIds, dt, sds);
            }

            return dt;
        }

        private DataTable ConvertPrimaryDataToDatatable(IEnumerable<AbstractTuple> tupleIterator, StructuredDataStructure dataStructure, string tableName = "", bool useLabelsAsColumnNames = false)
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(tableName))
                dt.TableName = "Primary data table";
            else
                dt.TableName = tableName;

            if (dataStructure != null)
            {
                buildTheHeader(dataStructure, useLabelsAsColumnNames, dt);
                buildTheBody(tupleIterator, dt, dataStructure);
            }

            return dt;
        }

        private void buildTheHeader(StructuredDataStructure sds, bool useLabelsAsColumnNames, DataTable dt)
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

        private void buildTheBody(IEnumerable<AbstractTuple> tupleIterator, DataTable dt, StructuredDataStructure sds)
        {
            foreach (var tuple in tupleIterator)
            {
                dt.Rows.Add(ConvertTupleIntoDataRow(dt, tuple, sds, true));
            }
        }

        private void buildTheBody(DatasetManager datasetManager, List<long> tupleIds, DataTable dt, StructuredDataStructure sds)
        {
            DataTupleIterator tupleIterator = new DataTupleIterator(tupleIds, datasetManager);
            foreach (var tuple in tupleIterator)
            {
                dt.Rows.Add(ConvertTupleIntoDataRow(dt, tuple, sds, true));
            }
        }

        private DataRow ConvertTupleIntoDataRow(DataTable dt, AbstractTuple t, StructuredDataStructure sts, bool useLabelsAsColumnName = false)
        {
            DataRow dr = dt.NewRow();
            string columnName = "";
            foreach (var vv in t.VariableValues)
            {
                columnName = useLabelsAsColumnName == true ? vv.Variable.Label : "ID" + vv.VariableId.ToString();

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


    }
}
