using BExIS.Dlm.Entities.DataStructure;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Helpers
{
    /// <summary>
    /// Design decision: should it be a static class?!
    /// </summary>
    /// <remarks>Should not be used directly or by any function outside of the service layer.</remarks>
    internal sealed class ConstraintHelper
    {
        #region Data Readers

        //public IReadOnlyRepository<DefaultValueConstraint> DefaultValueRepo { get; private set; }
        //public IReadOnlyRepository<DomainValueConstraint> DomainValueRepo { get; private set; }
        //public IReadOnlyRepository<ValidatorConstraint> ValidatorRepo { get; private set; }

        #endregion Data Readers

        #region DomainConstraint

        internal DomainConstraint SaveConstraint(DomainConstraint constraint, DataContainer container)
        {
            Contract.Requires(constraint.Items != null);
            Contract.Requires(constraint.Items.Count > 0);
            Contract.Requires(container != null);

            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0);
            constraint.Dematerialize();
            constraint.DataContainer = container;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return (constraint);
        }

        internal bool Delete(DomainConstraint entity)
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
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        internal bool Delete(IEnumerable<DomainConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DomainConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DomainConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
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

        #endregion DomainConstraint

        #region PatternConstraint

        internal PatternConstraint SaveConstraint(PatternConstraint constraint, DataContainer container)
        {
            Contract.Requires(constraint != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(constraint.MatchingPhrase));
            Contract.Requires(container != null);

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0);

            constraint.DataContainer = container;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return (constraint);
        }

        internal bool Delete(PatternConstraint entity)
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

        internal bool Delete(IEnumerable<PatternConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PatternConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PatternConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        internal PatternConstraint Update(PatternConstraint entity)
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

        #endregion PatternConstraint

        #region RangeConstraint

        internal RangeConstraint SaveConstraint(RangeConstraint constraint, DataContainer container)
        {
            Contract.Requires(constraint.Lowerbound <= constraint.Upperbound);
            Contract.Requires(container != null);

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0);

            constraint.DataContainer = container;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return (constraint);
        }

        internal bool Delete(RangeConstraint entity)
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

        internal bool Delete(IEnumerable<RangeConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (RangeConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (RangeConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        internal RangeConstraint Update(RangeConstraint entity)
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

        #endregion RangeConstraint

        #region ComparisonConstraint

        internal ComparisonConstraint SaveConstraint(ComparisonConstraint constraint, DataContainer container)
        {
            //Contract.Requires();
            Contract.Requires(container != null);

            Contract.Ensures(Contract.Result<ComparisonConstraint>() != null && Contract.Result<ComparisonConstraint>().Id >= 0);

            constraint.DataContainer = container;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ComparisonConstraint> repo = uow.GetRepository<ComparisonConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return (constraint);
        }

        internal bool Delete(ComparisonConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ComparisonConstraint> repo = uow.GetRepository<ComparisonConstraint>();

                entity = repo.Reload(entity);
                //delete the unit
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        internal bool Delete(IEnumerable<ComparisonConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ComparisonConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ComparisonConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ComparisonConstraint> repo = uow.GetRepository<ComparisonConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        internal ComparisonConstraint Update(ComparisonConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<ComparisonConstraint>() != null && Contract.Result<ComparisonConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ComparisonConstraint> repo = uow.GetRepository<ComparisonConstraint>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion ComparisonConstraint
    }
}