using NHibernate;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace BExIS.Utils.NH.Querying
{
    public static class QueryExtention
    {
        public static String ToSql(this IQueryable queryable)
        {
            var sessionProperty = typeof(DefaultQueryProvider).GetProperty("Session", BindingFlags.NonPublic | BindingFlags.Instance);
            var session = sessionProperty.GetValue(queryable.Provider, null) as ISession;
            var sessionImpl = session.GetSessionImplementation();
            var factory = sessionImpl.Factory;
            var nhLinqExpression = new NhLinqExpression(queryable.Expression, factory);
            var translatorFactory = new ASTQueryTranslatorFactory();

            //in case you want the parameters as well
            var parameters = nhLinqExpression.ParameterValuesByName.ToDictionary(x => x.Key, x => x.Value.Item1);

            var translator = translatorFactory.CreateQueryTranslators(nhLinqExpression, null, false, sessionImpl.EnabledFilters, factory).First();

            return translator.SQLString;
        }
    }
}