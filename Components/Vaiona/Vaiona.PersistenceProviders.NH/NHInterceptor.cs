using NHibernate;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Vaiona.PersistenceProviders.NH
{
    internal class NHInterceptor : EmptyInterceptor, IInterceptor
    {
        SqlString IInterceptor.OnPrepareStatement(SqlString sql)
        {
            Trace.WriteLine("SQL output at:" + DateTime.Now.ToString() + "--> " + sql.ToString());
            //NHSQL.NHibernateSQL.Add(sql.ToString());
            return sql;
        }
    }

    public static class NHSQL
    {
        public static List<string> NHibernateSQL { get; set; }
    }
}