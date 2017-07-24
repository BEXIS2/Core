using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Dlm.Orm.NH.Utils
{
    public class MaterializedViewHelper
    {
        // use this to access proper templates. All the templates are in one XML file under nativeObjects
        // they can be in default, or specific dialect folder
        string dbDialect = AppConfiguration.DatabaseDialect;
        List<string> columnLabels = new List<string>();

        public MaterializedViewHelper()
        {
        }

        //public MaterializedViewHelper(string dbDialect)
        //{
        //    this.dbDialect = dbDialect;
        //}

        public DataTable Retrieve(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT * FROM {0};", this.BuildName(datasetId).ToLower()));
            // execute the statement
            return retrieve(mvBuilder.ToString(), datasetId);
        }

        public DataTable Retrieve(long datasetId, int pageNumber, int pageSize)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT * FROM {0} Order by OrderNo OFFSET {1} LIMIT {2};", this.BuildName(datasetId).ToLower(), pageNumber*pageSize, pageSize));
            // execute the statement
            return retrieve(mvBuilder.ToString(), datasetId);
        }

        private DataTable retrieve(string queryStr, long datasetId)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    DataTable table = uow.ExecuteQuery(queryStr);
                    table = applyColumnLabels(table, datasetId);
                    return table;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private DataTable applyColumnLabels(DataTable table, long datasetId)
        {
            DataTable metadata = null;
            StringBuilder metadataQueryBuilder = new StringBuilder();
            metadataQueryBuilder.AppendLine(string.Format("SELECT a.attrelid, a.attnum, a.attname AS columnname, d.description, c.relname"));
            metadataQueryBuilder.AppendLine(string.Format("FROM pg_class c  JOIN pg_attribute a on c.oid = a.attrelid  JOIN pg_description d on c.oid = d.objoid"));
            metadataQueryBuilder.AppendLine(string.Format("WHERE a.attnum > 0 AND NOT a.attisdropped AND c.relname = '{0}' AND a.attnum = d.objsubid", this.BuildName(datasetId).ToLower()));
            metadataQueryBuilder.AppendLine(string.Format("ORDER BY a.attnum"));
            metadataQueryBuilder.Append(string.Format(";"));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                metadata = uow.ExecuteQuery(metadataQueryBuilder.ToString());
            }

            if (metadata == null || metadata.Rows == null || metadata.Rows.Count <= 0)
                return table;
            // setting default caption for all columns
            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].Caption = table.Columns[i].ColumnName;
            }
            // setting captions for variables, coming from the MV metadata (created and saved at MV cretaion time)
            foreach (DataRow row in metadata.Rows)
            {
                var columnName = row["columnname"].ToString();
                var columnLabel = row["description"].ToString();
                table.Columns[columnName].Caption = columnLabel;
            }
            return table;
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
            mvBuilder.AppendLine(string.Format("CREATE MATERIALIZED VIEW {0} AS", this.BuildName(datasetId)));
            // build MV's SELECT statement
            StringBuilder selectBuilder = new StringBuilder("SELECT").AppendLine(); // all the strings come form the tamplates
            selectBuilder
                .AppendLine(string.Format("{0},", "t.id AS Id"))
                .AppendLine(string.Format("{0},", "t.orderno AS OrderNo"))
                .AppendLine(string.Format("{0},", "t.timestamp AS Timestamp"))
                .AppendLine(string.Format("{0},", "t.datasetversionref AS VersionId"))
                ;
            int counter = 0;
            foreach (var columnDefinition in columnDefinitionList.OrderBy(p=>p.Item3)) // item3 is the variable order in its data structure
            {
                counter++;
                string columnStr = buildViewField(columnDefinition.Item1, columnDefinition.Item2, columnDefinition.Item3, columnDefinition.Item4);
                if (counter < columnDefinitionList.Count)
                    selectBuilder.AppendLine(string.Format("{0},", columnStr));
                else
                    selectBuilder.AppendLine(string.Format("{0}", columnStr)); // no comma for the last column

                columnLabels.Add(string.Format("COMMENT ON COLUMN {0}.{1} IS '{2}';",
                                    this.BuildName(datasetId).ToLower(),
                                    this.BuildColumnName(columnDefinition.Item4).ToLower(),
                                    columnDefinition.Item1));
            }
            selectBuilder
                .AppendLine("FROM datasetversions v INNER JOIN datatuples t ON t.datasetversionref = v.id")
                .AppendLine(string.Format("WHERE v.datasetref = {0}", datasetId))
                .Append("WITH DATA") //marks the view as queryable even if there is no data at creation time.
                ;

            // build the satetment
            mvBuilder
                //.Append(" ")
                .Append(selectBuilder)
                .AppendLine(";")
                ;
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    int result = uow.ExecuteNonQuery(mvBuilder.ToString());
                }
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    columnLabels.ForEach(p => uow.ExecuteNonQuery(p));
                }
            }
            catch (Exception ex)
            {
                throw ex; // ex is cought here for inspection purposes!
            }
        }

        private string BuildColumnName(long variableId)
        {
            return "var" + variableId;
        }

        public bool ExistsForDataset(long datasetId)
        {            
            StringBuilder mvBuilder = new StringBuilder();
            // the functions should obtain a reference to the DB dialect and based on that 1) load the templates from the native objects folder and 2) decide to use lowercase,uppercase, or natural case for object names.
            mvBuilder.AppendLine(string.Format("SELECT EXISTS (SELECT 1 FROM pg_catalog.pg_class c WHERE  c.relname = '{0}' AND c.relkind = 'm');", this.BuildName(datasetId).ToLower()));
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    object objRes = uow.ExecuteScalar(mvBuilder.ToString());
                    bool result = Convert.ToBoolean(objRes);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Refresh(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("REFRESH MATERIALIZED VIEW {0};", this.BuildName(datasetId).ToLower()));
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    uow.ExecuteNonQuery(mvBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                // what to do?
            }
        }

        public void Drop(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("DROP MATERIALIZED VIEW {0};", this.BuildName(datasetId).ToLower()));
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    uow.ExecuteNonQuery(mvBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                // what to do?
            }
        }


        public string BuildName(long datasetId)
        {
            return "mvDataset" + datasetId; // the strings must come from the mappings, nativeObjects/templates.xml. considering dialects and hierarchy
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

            string def = string.Format(template, order, dbDataType(dataType), this.BuildColumnName(Id).ToLower());// variableName.Replace(" ", "_"));
            return def;
        }

        private static Dictionary<string, string> typeTable = new Dictionary<string, string>
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

        /// <summary>
        /// Each supported runtime type has a corrsponsing type in the nativeObjects/lookups.xml under datatypes node <datatype key="" dbType="" hasSize="false"></datatype>
        /// </summary>
        /// <param name="variableDataType"></param>
        /// <returns></returns>
        private string dbDataType(string variableDataType, int size = 0)
        {
            if (typeTable.ContainsKey(variableDataType.ToLower()))
                return typeTable[variableDataType.ToLower()]; // change this
            else
                return typeTable["string"];
        }
    }
}
