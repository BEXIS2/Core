using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Util.Cfg;

namespace BExIS.Core.Persistence.Api
{
    public class DatabaseObjectNaming
    {
        public static string CreateTableName(Type entity, string baseName = "")
        {
            // take care or ploralization
            string name = string.Empty;
            name = (!string.IsNullOrWhiteSpace(baseName) ? baseName : entity.Name.Pluralize().CapitalizeShort());
            return (name);
        }

        public static string CreateSchemaTableName(Type entity, string baseName = "")
        {
            // take care or ploralization
            string name = string.Empty;
            if (AppConfiguration.UseSchemaInDatabaseGeneration)
            {
                if (entity.Namespace.Contains('.'))
                {
                    name = entity.Namespace.Substring(entity.Namespace.LastIndexOf('.') + 1);
                }
                else
                {
                    name = entity.Namespace;
                }
            }
            else
            {
                name = "dbo";
            }
            return (name);
        }
    }
}
