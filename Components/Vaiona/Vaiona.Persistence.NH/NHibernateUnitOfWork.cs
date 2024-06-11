using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using Vaiona.Persistence.Api;

namespace Vaiona.Persistence.NH
{
    public class NHibernateUnitOfWork : IUnitOfWork
    {
        internal ISession Session = null;
        private bool autoCommit = false;
        private bool throwExceptionOnError = true;
        private bool allowMultipleCommit = true;

        public IPersistenceManager PersistenceManager { get; internal set; }

        internal NHibernateUnitOfWork(NHibernatePersistenceManager persistenceManager, ISession session, bool autoCommit = false, bool throwExceptionOnError = true, bool allowMultipleCommit = false)
        {
            this.PersistenceManager = persistenceManager;
            this.autoCommit = autoCommit;
            this.throwExceptionOnError = throwExceptionOnError;
            this.allowMultipleCommit = allowMultipleCommit;
            this.Session = session;
            this.Session.BeginTransaction();
        }

        public IReadOnlyRepository<TEntity> GetReadOnlyRepository<TEntity>() where TEntity : class
        {
            IReadOnlyRepository<TEntity> repo = new NHibernateReadonlyRepository<TEntity>(this);
            return (repo);
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            IRepository<TEntity> repo = new NHibernateRepository<TEntity>(this);
            return (repo);
        }

        public void Commit()
        {
            try
            {
                if (BeforeCommit != null)
                    BeforeCommit(this, EventArgs.Empty);
                Session.Transaction.Commit();
                if (AfterCommit != null)
                    AfterCommit(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                //session.Transaction.Rollback(); //??
                if (throwExceptionOnError)
                    throw;
            }
            if (allowMultipleCommit && !this.Session.Transaction.IsActive)
            {
                this.Session.Transaction.Begin();
            }
            //else
            //{
            //    Session.Close();
            //}
        }

        //public void CommitAndContinue()
        //{
        //    Commit();
        //    this.session.Transaction.Begin();
        //}

        public void Ignore()
        {
            try
            {
                if (Session.Transaction.IsActive)
                {
                    if (BeforeIgnore != null)
                        BeforeIgnore(this, EventArgs.Empty);
                    Session.Transaction.Rollback();
                    if (AfterIgnore != null)
                        AfterIgnore(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                if (throwExceptionOnError)
                    throw;
            }
            if (allowMultipleCommit && !this.Session.Transaction.IsActive)
            {
                this.Session.Transaction.Begin();
            }
        }

        //public void IgnoreAndContinue()
        //{
        //    Ignore();
        //    this.session.Transaction.Begin();
        //}

        private bool isDisposed = false;

        ~NHibernateUnitOfWork()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these
            // operations, as well as in your methods that use the resource.
            if (!isDisposed)
            {
                if (disposing)
                {
                    disposeResources();
                    isDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void disposeResources()
        {
            if (autoCommit)
                this.Commit();
            else
                this.Ignore();
            //CurrentSessionContext.Unbind(this.Session.SessionFactory);
            //if (Session.IsOpen)
            //    Session.Close();
            //Session.Dispose();
            //Session = null;
            //HttpContext.Current.Session.Remove("CurrentNHSession");
            // http://www.amazedsaint.com/2010/02/top-5-common-programming-mistakes-net.html case 3: unhooking event handlers appropriately after wiring them
            BeforeCommit = null;
            AfterCommit = null;
            BeforeIgnore = null;
            AfterIgnore = null;
        }

        public IReadOnlyRepository<TEntity> GetReadOnlyRepository<TEntity>(Api.CacheMode cacheMode = Api.CacheMode.Ignore) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public void ClearCache(bool applyChanages = true)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(string queryName, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public List<T> ExecuteList<T>(string queryName, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public T ExecuteDynamic<T>(string queryString, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string queryString, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string queryString, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public DataTable ExecuteQuery(string queryString, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public event EventHandler BeforeCommit;

        public event EventHandler AfterCommit;

        public event EventHandler BeforeIgnore;

        public event EventHandler AfterIgnore;
    }
}