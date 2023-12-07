using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
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
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public ConstraintManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            DomainConstraintRepository = _guow.GetReadOnlyRepository<DomainConstraint>();
            PatternConstraintRepository = _guow.GetReadOnlyRepository<PatternConstraint>();
            RangeConstraintRepository = _guow.GetReadOnlyRepository<RangeConstraint>();
            ConstraintRepository = _guow.GetReadOnlyRepository<Constraint>();
        }

        ~ConstraintManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Constraint> ConstraintRepository { get; }
        public IQueryable<Constraint> Constraints => ConstraintRepository.Query();
        public IReadOnlyRepository<DomainConstraint> DomainConstraintRepository { get; }
        public IQueryable<DomainConstraint> DomainConstraints => DomainConstraintRepository.Query();
        public IReadOnlyRepository<PatternConstraint> PatternConstraintRepository { get; }
        public IQueryable<PatternConstraint> PatternConstraints => PatternConstraintRepository.Query();
        public IReadOnlyRepository<RangeConstraint> RangeConstraintRepository { get; }
        public IQueryable<RangeConstraint> RangeConstraints => RangeConstraintRepository.Query();

        public bool DeleteById(long constraintId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var constraintRepository = uow.GetRepository<Constraint>();
                return constraintRepository.Delete(constraintId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Constraint FindById(long constraintId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var constraintRepository = uow.GetReadOnlyRepository<Constraint>();
                return constraintRepository.Get(constraintId);
            }
        }

        public T FindById<T>(long constraintId) where T : Constraint
        {
            using (var uow = this.GetUnitOfWork())
            {
                var constraintRepository = uow.GetReadOnlyRepository<T>();
                return constraintRepository.Get(constraintId);
            }
        }

        public Constraint FindByName(string constraintName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var constraintRepository = uow.GetReadOnlyRepository<Constraint>();
                return constraintRepository.Query(m => m.Name.ToLowerInvariant() == constraintName.ToLowerInvariant()).FirstOrDefault();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        #region domain

        public DomainConstraint Create(DomainConstraint entity)
        {
            Contract.Requires(entity.Items != null);
            Contract.Requires(entity.Items.Count > 0);

            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0);
            
            entity.CreationDate = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            entity.LastModified = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            entity.Dematerialize();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();
                repo.Put(entity);
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

        public DomainConstraint Update(DomainConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0, "No entity is persisted!");
            
            entity.LastModified = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            entity.Dematerialize();
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

        #endregion domain

        #region pattern

        public PatternConstraint Create(PatternConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0, "No entity is persisted!");

            entity.CreationDate = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            entity.LastModified = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();
                repo.Put(entity);
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

        public PatternConstraint Update(PatternConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0, "No entity is persisted!");

            entity.LastModified = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
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

        #endregion pattern

        #region range

        public RangeConstraint Create(RangeConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            //Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0, "No entity is persisted!");

            entity.CreationDate = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            entity.LastModified = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();

                repo.Put(entity);
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

        public RangeConstraint Update(RangeConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0, "No entity is persisted!");

            entity.LastModified = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture);
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

        #endregion range
    }
}