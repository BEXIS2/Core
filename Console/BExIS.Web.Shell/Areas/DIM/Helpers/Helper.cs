using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Helper
{
    public class UIHelper
    {

        public DataTable ConvertStructuredDataStructureToDataTable(StructuredDataStructure sds)
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

            using (var uow = this.GetUnitOfWork())
            {
                StructuredDataStructure datastructure = uow.GetReadOnlyRepository<StructuredDataStructure>().Get(sds.Id);
                if (datastructure != null)
                {
                    List<Variable> variables = SortVariablesOnDatastructure(datastructure.Variables.ToList(), datastructure);

                    foreach (Variable var in variables)
                    {
                        Variable sdvu = uow.GetReadOnlyRepository<Variable>().Get(var.Id);

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
        }

        private List<Variable> SortVariablesOnDatastructure(List<Variable> variables, DataStructure datastructure)
        {
            return variables.OrderBy(v => v.OrderNo).ToList();
        }
    }
}