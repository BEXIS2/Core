using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Dlm.Orm.NH.Utils
{
    public class MaterializedViewHelper
    {
        // use this to access proper templates. All the templates are in one XML file under nativeObjects
        // they can be in default, or specific dialect folder
        private string dbDialect = AppConfiguration.DatabaseDialect;

        private List<string> columnLabels = new List<string>();

        public MaterializedViewHelper()
        {
        }

        public DataTable Retrieve(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT * FROM {0} Order by Id;", this.BuildName(datasetId).ToLower()));
            // execute the statement
            return retrieve(mvBuilder.ToString(), datasetId);
        }

        public DataTable Retrieve(long datasetId, int pageNumber, int pageSize)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT * FROM {0} Order by Id OFFSET {1} LIMIT {2};", this.BuildName(datasetId).ToLower(), pageNumber * pageSize, pageSize));
            // execute the statement
            return retrieve(mvBuilder.ToString(), datasetId);
        }

        public DataTable Retrieve(long datasetId, FilterExpression filter, OrderByExpression orderBy, ProjectionExpression projection, int pageNumber = 0, int pageSize = 0)
        {
            // Would be better to additionally have a ToHQL() method.
            var projectionClause = projection?.ToSQL();
            var orderbyClause = orderBy?.ToSQL();
            var whereClause = filter?.ToSQL();

            return Retrieve(datasetId, whereClause, orderbyClause, projectionClause, pageNumber, pageSize);
        }

        // can be public, but after the other overloads got matured enough.
        protected DataTable Retrieve(long datasetId, string whereClause, string orderbyClause, string projectionClause, int pageNumber = 0, int pageSize = 0)
        {
            // the following query must be converted to HQL for DB portability purpose. Also, all other dynamically created queries.
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder
                .Append("SELECT ")
                .Append(string.IsNullOrWhiteSpace(projectionClause) ? "*" : projectionClause).Append(" ") // projection
                .Append("FROM ").Append(this.BuildName(datasetId).ToLower()).Append(" ") // source mat. view
                .Append(string.IsNullOrWhiteSpace(whereClause) ? "" : "WHERE (" + whereClause + ")").Append(" ") // where
                .Append(string.IsNullOrWhiteSpace(orderbyClause) ? "Order by Id" : "Order By " + orderbyClause).Append(" ") //order by
                .Append(pageNumber <= 0 ? "" : "OFFSET " + pageNumber * pageSize).Append(" ") //offset
                .Append(pageSize <= 0 ? "LIMIT 10" : "LIMIT " + pageSize) // limit, default page size is 10
                .AppendLine()
                ;
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
                throw new Exception(string.Format("Could not retrieve data from dataset {0}. Check whether the corresponding view exists and is populated with data.", datasetId), ex);
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
                if (table.Columns.Contains(columnName))
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
            StringBuilder indexBuilder = new StringBuilder();
            // build MV's name
            mvBuilder.AppendLine(string.Format("CREATE MATERIALIZED VIEW {0} AS", this.BuildName(datasetId)));
            // build MV's SELECT statement
            StringBuilder selectBuilder = new StringBuilder("SELECT").AppendLine(); // all the strings come form the tamplates
            selectBuilder
                .AppendLine(string.Format("{0},", "t.id AS Id"))
                //.AppendLine(string.Format("{0},", "t.orderno AS OrderNo"))
                .AppendLine(string.Format("{0},", "t.timestamp AS Timestamp"))
                .AppendLine(string.Format("{0},", "t.datasetversionref AS VersionId"))
                ;
            int counter = 0;
            foreach (var columnDefinition in columnDefinitionList.OrderBy(p => p.Item3)) // item3 is the variable order in its data structure
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
                .AppendLine(string.Format("WHERE v.datasetref = {0} AND v.status in (0,2)", datasetId))
                .Append("WITH NO DATA") //avoids refreshing the MV at the creation time, the view will not be queryable until explicitly refreshed.
                                        //.Append("WITH DATA") //marks the view as queryable even if there is no data at creation time.
                ;

            // create index on id
            string indexName = this.BuildName(datasetId) + "Index";
            indexBuilder.AppendLine(string.Format("CREATE UNIQUE INDEX {1} ON {0}(Id)", this.BuildName(datasetId), indexName).ToString());

            // build the satetment
            mvBuilder
                //.Append(" ")
                .Append(selectBuilder)
                .AppendLine(";")
                .Append(indexBuilder)
                .AppendLine(";")

                ;
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    int result = uow.ExecuteNonQuery(mvBuilder.ToString());
                    foreach (var columnLabel in columnLabels)
                    {
                        uow.ExecuteNonQuery(columnLabel);
                    }
                    //columnLabels.ForEach(columnLabel => uow.ExecuteNonQuery(columnLabel));
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
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    uow.ExecuteNonQuery(mvBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                // what to do?
                throw ex;
            }
        }

        public long Count(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT COUNT(id) AS cnt FROM {0};", this.BuildName(datasetId).ToLower()));
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    var result = uow.ExecuteScalar(mvBuilder.ToString());
                    return (long)result;
                }
            }
            catch
            {
                return -1;
            }
        }

        public long Count(long datasetId, FilterExpression filter)
        {
            var whereClause = filter?.ToSQL();
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder
                .Append("SELECT ")
                .Append("COUNT(id) AS cnt").Append(" ")
                .Append("FROM ").Append(this.BuildName(datasetId).ToLower()).Append(" ") // source mat. view
                .Append(string.IsNullOrWhiteSpace(whereClause) ? "" : "WHERE (" + whereClause + ")").Append(" ") // where
                .AppendLine()
                ;
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    var result = uow.ExecuteScalar(mvBuilder.ToString());
                    return (long)result;
                }
            }
            catch
            {
                return -1;
            }
        }

        public bool Any(long datasetId)
        {
            using (IUnitOfWork uow = this.GetBulkUnitOfWork())
            {
                return Any(datasetId, uow);
            }
        }

        public bool Any(long datasetId, IUnitOfWork uow)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT id AS cnt FROM {0} LIMIT 1;", this.BuildName(datasetId).ToLower()));
            // execute the statement
            try
            {

                {
                    long result = (long)uow.ExecuteScalar(mvBuilder.ToString());
                    if (result > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public long Any(long datasetId, FilterExpression filter)
        {
            var whereClause = filter?.ToSQL();
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder
                .Append("SELECT ")
                .Append("COUNT(id) AS cnt").Append(" ")
                .Append("FROM ").Append(this.BuildName(datasetId).ToLower()).Append(" ") // source mat. view
                .Append(string.IsNullOrWhiteSpace(whereClause) ? "" : "WHERE (" + whereClause + ")").Append(" ") // where
                .Append("LIMIT 1")
                .AppendLine()
                ;
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    var result = uow.ExecuteScalar(mvBuilder.ToString());
                    return (long)result;
                }
            }
            catch
            {
                return -1;
            }
        }


        public void Drop(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("DROP MATERIALIZED VIEW IF EXISTS {0};", this.BuildName(datasetId).ToLower()));
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
            // string template = @"unnest(xpath('/Content/Item[{0}]/Property[@Name=""Value""]/@value', t.xmlvariablevalues)\\:\\:varchar[])\\:\\:{1} as {2}";
            // string template =   @"cast(unnest(cast(xpath('/Content/Item[Property[@Name=""VariableId"" and @value=""{0}""]][1]/Property[@Name=""Value""]/@value', t.xmlvariablevalues) AS varchar[])) AS {1}) AS {2}";
            // string template = @"unnest(xpath('/Content/Item[Property[@Name=""VariableId"" and @value=""{0}""]][1]/Property[@Name=""Value""]/@value', t.xmlvariablevalues)::character varying[]){1} AS {2}";

            string fieldType = dbDataType(dataType);
            fieldType = !string.IsNullOrEmpty(fieldType) ? " AS " + fieldType : "";

            //string accessPathTemplate = @"xpath('/Content/Item[Property[@Name=""VariableId"" and @value=""{0}""]][1]/Property[@Name=""Value""]/@value', t.xmlvariablevalues)";
            //string accessPath = string.Format(accessPathTemplate, Id);

            //            string fieldDef = $"CASE WHEN ({accessPath}::text = '{{\"\"}}'::text) THEN NULL WHEN ({accessPath}::text = '{{_null_null}}'::text) THEN NULL ELSE cast(({accessPath}::character varying[])[1] {fieldType}) END AS {this.BuildColumnName(Id).ToLower()}";
            string fieldDef = $"cast((t.values::character varying[])[{order}]  {fieldType}) AS {this.BuildColumnName(Id).ToLower()}";

            //string fieldDef = string.Format(fieldTemplate, accessPath, fieldType, this.BuildColumnName(Id).ToLower());
            // guard the column mapping for NULL protection
            return fieldDef;
        }

        private static Dictionary<string, string> typeTable = new Dictionary<string, string>
            {
                { "bool", "bool" },
                { "boolean", "bool" },
                { "date", "date" },
                { "datetime", "timestamp without time zone" },
                { "time", "time" },
                { "decimal", "numeric" },
                { "double", "float8" },
                { "int", "integer" },
                { "integer", "integer" },
                { "int16", "integer" },
                { "int32", "integer" },
                { "long", "bigint" },
                { "int64", "bigint" },
                { "uint16", "bigint" },
                { "uint32", "bigint" },
                { "uint64", "bigint" },
                { "text", "" }, // not needed -> character varying()
                { "string", "character varying" } //changed from 255 to unlimited to avoid data does not fit e.g. Sequence data
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
