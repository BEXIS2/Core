using System.Web;
using Vaiona.PersistenceProviders.NH;


namespace Vaiona.Web.HttpModules
{
    public class UoWLoggingModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                NHibernateUnitOfWork.ResetCounter();
            };

            context.EndRequest += (sender, e) =>
            {
                int count = NHibernateUnitOfWork.CurrentRequestUoWCount;
                System.Diagnostics.Debug.WriteLine($"[UoW] Total UoWs used this request: {count}");
            };
        }

        public void Dispose() { }
    }
}
