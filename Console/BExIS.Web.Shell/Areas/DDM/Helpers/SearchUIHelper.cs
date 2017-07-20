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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class SearchUIHelper
    {
        private static DataStructureManager dsm = new DataStructureManager();

        public static string ConvertXmlToHtml(string m, string xslPath = "")
        {
            string url = "";

            url = AppConfiguration.GetModuleWorkspacePath("ddm") + xslPath;

            if (m != null)
            {

                StringReader stringReader = new StringReader(m);
                XmlReader xmlReader = XmlReader.Create(stringReader);

                XslCompiledTransform xslt = new XslCompiledTransform(true);
                XsltSettings xsltSettings = new XsltSettings(true, false);
                xslt.Load(url, xsltSettings, new XmlUrlResolver());

                XsltArgumentList xsltArgumentList = new XsltArgumentList();

                StringWriter stringWriter = new StringWriter();
                xslt.Transform(xmlReader, xsltArgumentList, stringWriter);
                return stringWriter.ToString().Replace("bgc:", "");


            }

            return "";

        }

        public static DataTable ConvertPrimaryDataToDatatable(DatasetVersion dsv, IEnumerable<long> dsVersionTupleIds)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Primary data table";
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

            if (dsVersionTupleIds != null && sds != null)
            {
                foreach (var vu in sds.Variables)
                {
                    // use vu.Label or vu.DataAttribute.Name
                    DataColumn col = dt.Columns.Add("ID" + vu.Id.ToString()); // or DisplayName also
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
                    dt.Rows.Add(ConvertTupleIntoDataRow(dt, dataTuple, sds));
                }
            }

            return dt;
        }

        public static DataTable ConvertPrimaryDataToDatatable(DatasetVersion dsv, IEnumerable<AbstractTuple> dsVersionTuples)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Primary data table";
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

            XmlDocument doc = new XmlDocument();
            doc = (XmlDocument)sds.Extra;



            if (dsVersionTuples != null && sds != null && doc != null)
            {
                IEnumerable<XElement> orderList = XmlUtility.GetXElementByNodeName("variable", XmlUtility.ToXDocument(doc));

                foreach (XElement element in orderList)
                {
                    var vu = sds.Variables.Where(v => v.Id.Equals(Convert.ToInt64(element.Value))).FirstOrDefault();

                    // use vu.Label or vu.DataAttribute.Name
                    DataColumn col = dt.Columns.Add("ID" + vu.Id.ToString()); // or DisplayName also
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

                foreach (var tuple in dsVersionTuples)
                {
                    dt.Rows.Add(ConvertTupleIntoDataRow(dt, tuple, sds));
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
        private static DataRow ConvertTupleIntoDataRow(DataTable dt, AbstractTuple t, StructuredDataStructure sts)
        {

            DataRow dr = dt.NewRow();

            foreach (var vv in t.VariableValues)
            {
                if (vv.VariableId > 0)
                {
                    string valueAsString = "";
                    if (vv.Value == null)
                    {
                        dr["ID" + vv.VariableId.ToString()] = DBNull.Value;
                    }
                    else
                    {
                        valueAsString = vv.Value.ToString();

                        Variable varr = sts.Variables.Where(p => p.Id == vv.VariableId).SingleOrDefault();
                        switch (varr.DataAttribute.DataType.SystemType)
                        {
                            case "String":
                                {
                                    dr["ID" + vv.VariableId.ToString()] = valueAsString;
                                    break;
                                }

                            case "Double":
                                {
                                    double value;
                                    if (double.TryParse(valueAsString, out value))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToDouble(valueAsString);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = -99999;//double.MaxValue;
                                    break;
                                }

                            case "Int16":
                                {
                                    Int16 value;
                                    if (Int16.TryParse(valueAsString, out value))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToInt16(valueAsString);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = Int16.MaxValue;
                                    break;
                                }

                            case "Int32":
                                {
                                    Int32 value;
                                    if (Int32.TryParse(valueAsString, out value))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToInt32(valueAsString);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = Int32.MaxValue;
                                    break;
                                }

                            case "Int64":
                                {
                                    Int64 value;
                                    if (Int64.TryParse(valueAsString, out value))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToInt64(valueAsString);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = Int64.MaxValue;
                                    break;
                                }

                            case "Decimal":
                                {
                                    decimal value;
                                    if (decimal.TryParse(valueAsString, out value))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToDecimal(valueAsString);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = -99999;//decimal.MaxValue;
                                    break;
                                }

                            case "Float":
                                {
                                    decimal value;
                                    if (decimal.TryParse(valueAsString, out value))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToDecimal(valueAsString);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = -99999;
                                    break;
                                }

                            case "DateTime":
                                {
                                    if (!String.IsNullOrEmpty(valueAsString))
                                        dr["ID" + vv.VariableId.ToString()] = Convert.ToDateTime(valueAsString, CultureInfo.InvariantCulture);
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = DateTime.MaxValue;

                                    break;
                                }

                            default:
                                {
                                    if (!String.IsNullOrEmpty(vv.Value.ToString()))
                                        dr["ID" + vv.VariableId.ToString()] = valueAsString;
                                    else
                                        dr["ID" + vv.VariableId.ToString()] = DBNull.Value;

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

        public static DataTable ConvertStructuredDataStructureToDataTable(StructuredDataStructure sds)
        {
            DataTable dt = new DataTable();

            dt.TableName = "DataStruture";
            dt.Columns.Add("VariableName");
            dt.Columns.Add("Optional");
            dt.Columns.Add("VariableId");
            dt.Columns.Add("ShortName");
            //dt.Columns.Add("Parameters");
            dt.Columns.Add("Description");
            dt.Columns.Add("Unit");
            dt.Columns.Add("DataType");

            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure datastructure = dsm.StructuredDataStructureRepo.Get(sds.Id);
            if (datastructure != null)
            {
                List<Variable> variables = SortVariablesOnDatastructure(datastructure.Variables.ToList(), datastructure);

                foreach (Variable var in variables)
                {
                    Variable sdvu = dsm.VariableRepo.Get(var.Id);

                    DataRow dr = dt.NewRow();
                    if (sdvu.Label != null)
                        dr["VariableName"] = sdvu.Label;
                    else
                        dr["VariableName"] = "n/a";

                    dr["Optional"] = sdvu.IsValueOptional.ToString();

                    if (sdvu.Label != null)
                        dr["VariableId"] = sdvu.Id;
                    else
                        dr["VariableId"] = "n/a";

                    if (sdvu.DataAttribute.DataType != null)
                        dr["ShortName"] = sdvu.DataAttribute.ShortName;
                    else
                        dr["ShortName"] = "n/a";

                    //if (sdvu.Parameters.Count > 0) dr["Parameters"] = "current not shown";
                    //else dr["Parameters"] = "n/a";

                    if (sdvu.Description != null || sdvu.Description != "")
                        dr["Description"] = sdvu.Description;
                    else
                        dr["Description"] = "n/a";

                    if (sdvu.Unit != null)
                        dr["Unit"] = sdvu.Unit.Name;
                    else
                        dr["Unit"] = "n/a";

                    if (sdvu.DataAttribute.DataType != null)
                        dr["DataType"] = sdvu.DataAttribute.DataType.Name;
                    else
                        dr["DataType"] = "n/a";

                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        private string GetParameterNamesAsString(ICollection<Parameter> vpuList)
        {
            string parameters = "";
            foreach (Parameter vpu in vpuList)
            {
                if (vpu.Equals(vpuList.First()))
                    parameters = vpu.Label;
                else
                    parameters += ", " + vpu.Label;
            }

            return "";
        }

        public static List<ContentDescriptor> GetContantDescriptorFromKey(DatasetVersion datasetVersion, string key)
        {
            List<ContentDescriptor> fileList = new List<ContentDescriptor>();

            foreach (ContentDescriptor contenDescriptor in datasetVersion.ContentDescriptors)
            {
                if (contenDescriptor.Name.Equals(key)) fileList.Add(contenDescriptor);
            }

            return fileList;
        }


        private static List<Variable> SortVariablesOnDatastructure(List<Variable> variables, DataStructure datastructure)
        {
            List<Variable> sortedVariables = new List<Variable>();

            XmlDocument extraXml = datastructure.Extra as XmlDocument;

            if (datastructure.Extra != null && (datastructure.Extra as XmlDocument).GetElementsByTagName("order").Count != 0)
            {
                XmlNode orderNode = extraXml.GetElementsByTagName("order")[0];
                XmlDocument order = new XmlDocument();
                order.LoadXml(orderNode.OuterXml);

                IEnumerable<XElement> elements = XmlUtility.GetXElementByNodeName("variable", XmlUtility.ToXDocument(order));

                foreach (XElement element in elements)
                {
                    long id = Convert.ToInt64(element.Value);
                    Variable var = variables.Where(v => v.Id.Equals(id)).FirstOrDefault();
                    if (var != null)
                        sortedVariables.Add(var);
                }
                return sortedVariables;
            }
            return variables;
        }

    }
}