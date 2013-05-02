using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Data;
using BExIS.Core.Util.Cfg;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.Search.Helpers
{
    public class SearchUIHelper
    {
        public static string ConvertXmlToHtml(string m, string xslPath="")
        {
            string url = "";
            if (xslPath != ""){
                url = AppConfiguration.GetModuleWorkspacePath("Search") + "UI\\HtmlShowMetadata.xsl";
            }
            else {
                url = AppConfiguration.GetModuleWorkspacePath("Search") + xslPath;
            }

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
                return stringWriter.ToString().Replace("bgc:","");

                
            }

            return "";

        }


        public static DataTable ConvertPrimaryDataToDatatable(Dataset ds)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Primary data table";
            StructuredDataStructure sds = (StructuredDataStructure)(ds.DataStructure.Self);


            if (ds.Tuples != null && sds != null)
            {
                foreach (var vu in sds.VariableUsages)
                {
                    DataColumn col = dt.Columns.Add(vu.Variable.Name.Replace(" ","")); // or DisplayName also
                    col.Caption = vu.Variable.Name;
                    

                    if(vu.Variable.ParameterUsages.Count>0)
                    {
                        foreach (var pu in vu.Variable.ParameterUsages)
                        {
                            DataColumn col2 = dt.Columns.Add(pu.Parameter.Name.Replace(" ", "")); // or DisplayName also
                            col2.Caption = pu.Parameter.Name;
                            
                        }
                    }
                }

                foreach (var tuple in ds.Tuples)
                {
                     dt.Rows.Add(ConvertTupleIntoDataRow(dt,tuple));
                }
            }

            return dt;
        }

        private static DataRow ConvertTupleIntoDataRow(DataTable dt, DataTuple t)
        {

            DataRow dr = dt.NewRow();

            foreach(var vv in t.VariableValues)
            {
                if (vv.Variable != null)
                {
                    dr[vv.Variable.Name.Replace(" ", "")] = vv.Value;

                    if (vv.ParameterValues.Count > 0)
                    {
                        foreach (var pu in vv.ParameterValues)
                        {
                            dr[pu.Parameter.Name.Replace(" ", "")] = pu.Value;
                        }
                    }
                }
            }

            return dr;
        }

        public static DataTable ConvertStructuredDataStructureToDataTable(StructuredDataStructure sds)
        { 
            DataTable dt = new DataTable();

            dt.TableName = "DataStruture";
            dt.Columns.Add("VariableName");
            dt.Columns.Add("Parameters");
            dt.Columns.Add("Unit");
            dt.Columns.Add("Description");

            foreach (StructuredDataVariableUsage sdvu in sds.VariableUsages)
            {
                DataRow dr = dt.NewRow();
                if (sdvu.Variable.Name != null) dr["VariableName"] = sdvu.Variable.Name;
                else dr["VariableName"] = "n/a";

                if (sdvu.Variable.ParameterUsages.Count > 0) dr["Parameters"] = "current not shown";
                else dr["Parameters"] = "n/a";

                if (sdvu.Variable.Unit != null) dr["Unit"] = sdvu.Variable.Unit.Name;
                else dr["Unit"] = "n/a";

                if (sdvu.Variable.Description != null || sdvu.Variable.Description!="")
                {

                    dr["Description"] = sdvu.Variable.Description;
                }
                else dr["Description"] = "n/a";

                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string GetParameterNamesAsString(ICollection<VariableParameterUsage> vpuList)
        {
            string parameters = "";
            foreach (VariableParameterUsage vpu in vpuList)
            {
                if (vpu.Equals(vpuList.First()))
                    parameters = vpu.Parameter.Name;
                else
                    parameters += ", " + vpu.Parameter.Name;
            }

            return "";
        }
    }
}