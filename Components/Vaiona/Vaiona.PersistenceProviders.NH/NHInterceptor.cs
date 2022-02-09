using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using System.Diagnostics;
using NHibernate.SqlCommand;

namespace Vaiona.PersistenceProviders.NH
{
    class NHInterceptor : EmptyInterceptor, IInterceptor
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