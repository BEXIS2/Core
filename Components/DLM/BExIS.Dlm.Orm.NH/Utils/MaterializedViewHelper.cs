using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Orm.NH.Utils
{
    public class MaterializedViewHelper
    {
        IUnitOfWork uow;
        public MaterializedViewHelper(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        /// <summary>
        /// Creates a materialized view for the latest version of the dataset with <paramref name="datasetId"/> and installs it on the DB via the connection bound to uow
        /// </summary>
        /// <param name="datasetId">The id of the dataset. It is used in naming the view.</param>
        /// <param name="columnDefinitionList">A list of column definitions coming from the data structure of the dataset. Each definition conatins: variable's name, data type, order, and Id</param>
        public void Create(long datasetId, List<Tuple<string, string, int, long>> columnDefinitionList)
        {
            StringBuilder mvBuilder = new StringBuilder();
            // build MV's name
            mvBuilder
                .Append("CREATE MATERIALIZED VIEW")
                .Append(" ")
                .Append(this.BuildName(datasetId))
                .Append(" ")
                .AppendLine("AS")
                ;
            // build MV's SELECT statement
            StringBuilder selectBuilder = new StringBuilder("SELECT"); // all the strings come form the tamplates
            selectBuilder
                .Append(" ").Append("t.id").Append(", ")
                .Append(" ").Append("t.versionno").Append(", ")
                .Append(" ").Append("t.orderno").Append(", ")
                .Append(" ").Append("t.timestamp").Append(", ")
                .Append(" ").Append("t.datasetversionref").Append(", ")
                ;
            foreach (var columnDefinition in columnDefinitionList.OrderBy(p=>p.Item3)) // item3 is the variable order in its data structure
            {
                string columnStr = buildViewField(columnDefinition.Item1, columnDefinition.Item2, columnDefinition.Item3, columnDefinition.Item4);
                selectBuilder
                    .Append(" ")
                    .Append(columnStr)
                    .Append(", ");
            }
            //remove the latest comma
            selectBuilder
                .Remove(selectBuilder.Length - 2, 1)
                .AppendLine();
            selectBuilder
                .AppendLine("FROM datatuples t")
                .Append("WHERE  t.datasetversionref = ")
                .Append(datasetId.ToString())
                ;

            // build the satetment

            mvBuilder.Append(" ")
                .Append(selectBuilder)
                .AppendLine(";")
                ;
            // execute the statement
            using (uow)
            {
                uow.ExecuteDynamic<int>(mvBuilder.ToString());
            }

        }

        public string BuildName(long datasetId)
        {
            return "mvDataset" + datasetId; // the strings must come from the mappings, nativeObjects/templates.xml. considering dialects and hierarchy
        }

        public bool ExistsForDataset(long datasetId)
        {
            string viewName = BuildName(datasetId);
            string sql = string.Format("... {0} ...", viewName);
            using (IUnitOfWork uow = this.GetIsolatedUnitOfWork())
            {
                int result = uow.ExecuteDynamic<int>(sql);
                if (result > 0) // must be double checked
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Using a column definition, creates a projection phrase to be used in the view's select.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="dataType">The system type of the variable</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        private string buildViewField(string variableName, string dataType, int order, long Id)
        {
            // @"unnest(xpath('/Content/Item[{0}]/Property[@Name=""Value""]/@value', t.xmlvariablevalues)\\:\\:varchar[])\\:\\:{1} as {2}"
            string template = @"cast(unnest(cast(xpath('/Content/Item[{0}]/Property[@Name=""Value""]/@value', t.xmlvariablevalues) AS varchar[])) AS {1}) AS {2}";

            string def = string.Format(template, order, dbDataType(dataType), variableName.Replace(" ", "_"));
            return def;
        }

        /// <summary>
        /// Each supported runtime type has a corrsponsing type in the nativeObjects/lookups.xml under datatypes node <datatype key="" dbType="" hasSize="false"></datatype>
        /// </summary>
        /// <param name="variableDataType"></param>
        /// <returns></returns>
        private string dbDataType(string variableDataType, int size = 0)
        {
            Dictionary<string, string> typeTable = new Dictionary<string, string>
            {
                { "bool", "bool" },
                { "boolean", "bool" },
                { "date", "date" },
                { "datetime", "date" },
                { "time", "time" },
                { "decimal", "numeric" },
                { "double", "float8" },
                { "int", "int4" },
                { "int32", "int4" },
                { "integer", "int4" },
                { "text", "text" },
                { "string", "character varying(255)" }
            };
            if (typeTable.ContainsKey(variableDataType.ToLower()))
                return typeTable[variableDataType]; // change this
            else
                return "character varying(255)";
        }
    }
}
