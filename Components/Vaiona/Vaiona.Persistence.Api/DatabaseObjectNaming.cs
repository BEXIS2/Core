using System;
using System.Linq;
using Vaiona.Utils.Cfg;

namespace Vaiona.Persistence.Api
{
    public class DatabaseObjectNaming
    {
        public static string CreateTableName(Type entity, string baseName = "")
        {
            // take care of polarization
            string name = string.Empty;
            name = (!string.IsNullOrWhiteSpace(baseName) ? baseName : entity.Name.Pluralize().CapitalizeShort());
            return (name);
        }

        // subject to more tests
        public static string CreateSchemaTableName(Type entity, string baseName = "")
        {
            // take care of polarization
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