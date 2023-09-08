using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class ConstraintManager : IDisposable
    {
        private IUnitOfWork uow = null;

        public ConstraintManager()
        {
            uow = this.GetIsolatedUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<Constraint>();
        }

        private bool isDisposed = false;

        ~ConstraintManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (uow != null)
                        uow.Dispose();
                    isDisposed = true;
                }
            }
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<Constraint> Repo { get; private set; }
        public IReadOnlyRepository<DomainConstraint> DomainConstraintRepo { get; private set; }
        public IReadOnlyRepository<PatternConstraint> PatternConstraintRepo { get; private set; }
        public IReadOnlyRepository<RangeConstraint> RangeConstraintRepo { get; private set; }

        #endregion Data Readers

        #region domain 
        public DomainConstraint Create(DomainConstraint entity)
        {
            Contract.Requires(entity.Items != null);
            Contract.Requires(entity.Items.Count > 0);

            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0);
            entity.Dematerialize();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        internal DomainConstraint Update(DomainConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(DomainConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();

                entity = repo.Reload(entity);
                //delete the unit
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
 
            return (true);
        }

        #endregion

        #region pattern

        public PatternConstraint Create(PatternConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public PatternConstraint Update(PatternConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(PatternConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();

                entity = repo.Reload(entity);
                //delete the unit
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion

        #region range

        public RangeConstraint Create(RangeConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            //Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();

                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public RangeConstraint Update(RangeConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(RangeConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();

                entity = repo.Reload(entity);
                //delete the unit
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion
    }
}