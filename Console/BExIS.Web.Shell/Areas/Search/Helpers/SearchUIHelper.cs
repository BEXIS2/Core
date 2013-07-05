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


        public static DataTable ConvertPrimaryDataToDatatable(DatasetVersion dsv)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Primary data table";
            StructuredDataStructure sds = (StructuredDataStructure)(dsv.Dataset.DataStructure.Self);


            if (dsv.EffectiveTuples != null && sds != null)
            {
                foreach (var vu in sds.VariableUsages)
                {
                    // use vu.Label or vu.DataAttribute.Name
                    DataColumn col = dt.Columns.Add(vu.Label.Replace(" ","")); // or DisplayName also
                    col.Caption = vu.Label;
                    

                    if(vu.ParameterUsages.Count>0)
                    {
                        foreach (var pu in vu.ParameterUsages)
                        {
                            DataColumn col2 = dt.Columns.Add(pu.Label.Replace(" ", "")); // or DisplayName also
                            col2.Caption = pu.Label;
                            
                        }
                    }
                }

                foreach (var tuple in dsv.EffectiveTuples)
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
                if (vv.Usage != null)
                {
                    dr[vv.Usage.Label.Replace(" ", "")] = vv.Value;

                    if (vv.ParameterValues.Count > 0)
                    {
                        foreach (var pu in vv.ParameterValues)
                        {
                            dr[pu.Usage.Label.Replace(" ", "")] = pu.Value;
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

            foreach (VariableUsage sdvu in sds.VariableUsages)
            {
                DataRow dr = dt.NewRow();
                if (sdvu.Label != null) dr["VariableName"] = sdvu.Label;
                else dr["VariableName"] = "n/a";

                if (sdvu.ParameterUsages.Count > 0) dr["Parameters"] = "current not shown";
                else dr["Parameters"] = "n/a";

                if (sdvu.DataAttribute.Unit != null) dr["Unit"] = sdvu.DataAttribute.Unit.Name;
                else dr["Unit"] = "n/a";

                if (sdvu.DataAttribute.Description != null || sdvu.DataAttribute.Description != "")
                {

                    dr["Description"] = sdvu.DataAttribute.Description;
                }
                else dr["Description"] = "n/a";

                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string GetParameterNamesAsString(ICollection<ParameterUsage> vpuList)
        {
            string parameters = "";
            foreach (ParameterUsage vpu in vpuList)
            {
                if (vpu.Equals(vpuList.First()))
                    parameters = vpu.Label;
                else
                    parameters += ", " + vpu.Label;
            }

            return "";
        }
    }
}