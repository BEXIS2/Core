using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class DataContainerManager : IDisposable
    {
        private ConstraintHelper helper = new ConstraintHelper();

        private IUnitOfWork guow = null;

        public DataContainerManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.ExtendedPropertyRepo = guow.GetReadOnlyRepository<ExtendedProperty>();
            this.UsageRepo = guow.GetReadOnlyRepository<Parameter>();
        }

        private bool isDisposed = false;

        ~DataContainerManager()
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
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<ExtendedProperty> ExtendedPropertyRepo { get; private set; }

        public IReadOnlyRepository<Parameter> UsageRepo { get; private set; }

        #endregion Data Readers

        #region Extended Property

        public ExtendedProperty CreateExtendedProperty(string name, string description, DataContainer container, ICollection<Constraint> constraints)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(container != null && container.Id >= 0);

            Contract.Ensures(Contract.Result<ExtendedProperty>() != null && Contract.Result<ExtendedProperty>().Id >= 0);

            ExtendedProperty e = new ExtendedProperty()
            {
                Name = name,
                Description = description,
                DataContainer = container,
            };
            //if (constraints != null)
            //    e.Constraints = new List<Constraint>(constraints);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteExtendedProperty(ExtendedProperty entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteExtendedProperty(IEnumerable<ExtendedProperty> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ExtendedProperty e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ExtendedProperty e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public ExtendedProperty UpdateExtendedProperty(ExtendedProperty entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<ExtendedProperty>() != null && Contract.Result<ExtendedProperty>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion Extended Property

        #region Associations

        public void AddConstraint(DomainConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void AddConstraint(PatternConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void AddConstraint(RangeConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void AddConstraint(ComparisonConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void RemoveConstraint(DomainConstraint constraint)
        {
            constraint.DataContainer = null;
            helper.Delete(constraint);
        }

        public void RemoveConstraint(PatternConstraint constraint)
        {
            constraint.DataContainer = null;
            helper.Delete(constraint);
        }

        public void RemoveConstraint(RangeConstraint constraint)
        {
            constraint.DataContainer = null;
            helper.Delete(constraint);
        }

        public void RemoveConstraint(ComparisonConstraint constraint)
        {
            constraint.DataContainer = null;
            helper.Delete(constraint);
        }

        #endregion Associations
    }
}