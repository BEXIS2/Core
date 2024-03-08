using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            dt.Columns.Add("Description");
            dt.Columns.Add("Unit");
            //dt.Columns.Add("Parameters");      
            dt.Columns.Add("DataType");
            dt.Columns.Add("Category");
            dt.Columns.Add("MissingValues");
            dt.Columns.Add("Meanings");
            dt.Columns.Add("Optional");
            dt.Columns.Add("VariableId");

            using (var uow = this.GetUnitOfWork())
            {
                StructuredDataStructure datastructure = uow.GetReadOnlyRepository<StructuredDataStructure>().Get(sds.Id);
                if (datastructure != null)
                {
                    List<VariableInstance> variables = SortVariablesOnDatastructure(datastructure.Variables.ToList(), datastructure);

                    foreach (Variable var in variables)
                    {
                        VariableInstance sdvu = uow.GetReadOnlyRepository<VariableInstance>().Get(var.Id);

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

                        if (sdvu.VariableTemplate != null)
                            dr["Category"] = sdvu.VariableTemplate.Label;
                        else
                            dr["Category"] = "n/a";

                        //if (sdvu.Parameters.Count > 0) dr["Parameters"] = "current not shown";
                        //else dr["Parameters"] = "n/a";

                        if (sdvu.Description != null || sdvu.Description != "")
                            dr["Description"] = sdvu.Description;
                        else
                            dr["Description"] = "n/a";

                        if (sdvu.Unit != null)
                            dr["Unit"] = sdvu.Unit.Abbreviation;
                        else
                            dr["Unit"] = "n/a";

                        if (sdvu.DataType != null)
                            dr["DataType"] = sdvu.DataType.Name;
                        else
                            dr["DataType"] = "n/a";

                        if (sdvu.MissingValues != null)
                            dr["MissingValues"] = String.Join(", ", sdvu.MissingValues.Select(m => m.DisplayName).ToArray());
                        else
                            dr["MissingValues"] = "n/a";

                        if (sdvu.Meanings != null)
                            dr["Meanings"] = String.Join(", ", sdvu.Meanings.Select(m => m.Name).ToArray());
                        else
                            dr["Meanings"] = "n/a";

                        dt.Rows.Add(dr);
                    }
                }
                return dt;
            }
        }

        private List<VariableInstance> SortVariablesOnDatastructure(List<VariableInstance> variables, DataStructure datastructure)
        {
            return variables.OrderBy(v => v.OrderNo).ToList();
        }
    }
}