using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using BExIS.Core.Persistence.Api;

namespace BExIS.Core.PersistenceProviders.NH
{
    public class NHibernateUnitOfWork: IUnitOfWork
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
            else
            {
                Session.Close();
            }
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

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (autoCommit)
                    this.Commit();
                else
                    this.Ignore();
                if (Session.IsOpen)
                    Session.Close();
                Session = null;
                // http://www.amazedsaint.com/2010/02/top-5-common-programming-mistakes-net.html case 3: unhooking event handlers appropriately after wiring them
                BeforeCommit = null;
                AfterCommit = null;
                BeforeIgnore = null;
                AfterIgnore = null;
            }
            // Code to dispose the un-managed resources of the class
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler BeforeCommit;

        public event EventHandler AfterCommit;

        public event EventHandler BeforeIgnore;

        public event EventHandler AfterIgnore;
    }
}
