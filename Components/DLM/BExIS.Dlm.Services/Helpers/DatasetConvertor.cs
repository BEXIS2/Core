using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace BExIS.Dlm.Services.Helpers
{
    public class DatasetConvertor
    {
        public DataTable ConvertDatasetVersion(IEnumerable<AbstractTuple> tupleIterator, StructuredDataStructure dataStructure, string tableName = "")
        {
            return ConvertPrimaryDataToDatatable(tupleIterator, dataStructure, tableName);
        }

        public DataTable ConvertDatasetVersion(DatasetManager datasetManager, DatasetVersion datasetVersion, string tableName = "")
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(tableName))
                dt.TableName = "Primary data table";
            else
                dt.TableName = tableName;

            using (DataStructureManager dsm = new DataStructureManager())
            {
                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
                var tupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);

                if (tupleIds != null && tupleIds.Count > 0 && sds != null)
                {
                    buildTheHeader(sds, dt);
                    buildTheBody(datasetManager, tupleIds, dt, sds);
                }
            }

            return dt;
        }

        public DataTable ConvertDatasetVersion(List<AbstractTuple> tuples, DatasetVersion datasetVersion, StructuredDataStructure sds, string tableName = "")
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(tableName))
                dt.TableName = "Primary data table";
            else
                dt.TableName = tableName;
            if (tuples != null && tuples.Count > 0 && sds != null)
            {
                buildTheHeader(sds, dt);
                buildTheBody(tuples, dt, sds);
            }

            return dt;
        }

        private DataTable ConvertPrimaryDataToDatatable(IEnumerable<AbstractTuple> tupleIterator, StructuredDataStructure dataStructure, string tableName = "")
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(tableName))
                dt.TableName = "Primary data table";
            else
                dt.TableName = tableName;

            if (dataStructure != null)
            {
                buildTheHeader(dataStructure, dt);
                buildTheBody(tupleIterator, dt, dataStructure);
            }

            return dt;
        }

        private void buildTheHeader(StructuredDataStructure sds, DataTable dt)
        {
            foreach (var vu in sds.Variables)
            {
                DataColumn col = null;

                string columnName = "var" + vu.Id.ToString();

                col = dt.Columns.Add(columnName); // or DisplayName also

                col.Caption = string.IsNullOrEmpty(vu.Label) ? columnName : vu.Label;

                switch (vu.DataType.SystemType)
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
            }
        }

        private void buildTheBody(IEnumerable<AbstractTuple> tupleIterator, DataTable dt, StructuredDataStructure sds)
        {
            foreach (var tuple in tupleIterator)
            {
                dt.Rows.Add(ConvertTupleIntoDataRow(dt, tuple, sds));
            }
        }

        private void buildTheBody(DatasetManager datasetManager, List<long> tupleIds, DataTable dt, StructuredDataStructure sds)
        {
            DataTupleIterator tupleIterator = new DataTupleIterator(tupleIds, datasetManager);
            foreach (var tuple in tupleIterator)
            {
                dt.Rows.Add(ConvertTupleIntoDataRow(dt, tuple, sds));
            }
        }

        private DataRow ConvertTupleIntoDataRow(DataTable dt, AbstractTuple t, StructuredDataStructure sts)
        {
            DataRow dr = dt.NewRow();
            string columnName = "";
            foreach (var vv in t.VariableValues)
            {
                columnName = "var" + vv.VariableId.ToString();

                if (vv.VariableId > 0)
                {
                    string valueAsString = "";
                    if (vv.Value == "") // check for NULL values
                    {
                        dr[columnName] = DBNull.Value;
                    }
                    else
                    {
                        valueAsString = vv.Value.ToString();

                        Dlm.Entities.DataStructure.Variable varr = sts.Variables.Where(p => p.Id == vv.VariableId).SingleOrDefault();
                        switch (varr.DataType.SystemType)
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
                                        dr[columnName] = value;
                                    else
                                        dr[columnName] = double.MaxValue;
                                    break;
                                }

                            case "Int16":
                                {
                                    Int16 value;
                                    if (Int16.TryParse(valueAsString, out value))
                                        dr[columnName] = value;
                                    else
                                        dr[columnName] = Int16.MaxValue;
                                    break;
                                }

                            case "Int32":
                                {
                                    Int32 value;
                                    if (Int32.TryParse(valueAsString, out value))
                                        dr[columnName] = value;
                                    else
                                        dr[columnName] = Int32.MaxValue;
                                    break;
                                }

                            case "Int64":
                                {
                                    Int64 value;
                                    if (Int64.TryParse(valueAsString, out value))
                                        dr[columnName] = value;
                                    else
                                        dr[columnName] = Int64.MaxValue;
                                    break;
                                }

                            case "Decimal":
                                {
                                    decimal value;
                                    if (decimal.TryParse(valueAsString, out value))
                                        dr[columnName] = value;
                                    else
                                        dr[columnName] = decimal.MaxValue;
                                    break;
                                }

                            case "Float":
                                {
                                    decimal value;
                                    if (decimal.TryParse(valueAsString, out value))
                                        dr[columnName] = value;
                                    else
                                        dr[columnName] = decimal.MaxValue;
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
                }
            }

            return dr;
        }
    }
}