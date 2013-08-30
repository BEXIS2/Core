using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public sealed class ConstraintManager
    {
        public ConstraintManager() 
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.DefaultValueRepo = uow.GetReadOnlyRepository<DefaultValueConstraint>();
            this.DomainValueRepo = uow.GetReadOnlyRepository<DomainValueConstraint>();
            this.ValidatorRepo = uow.GetReadOnlyRepository<ValidatorConstraint>();
        }

        #region Data Readers

        public IReadOnlyRepository<DefaultValueConstraint> DefaultValueRepo { get; private set; }
        public IReadOnlyRepository<DomainValueConstraint> DomainValueRepo { get; private set; }
        public IReadOnlyRepository<ValidatorConstraint> ValidatorRepo { get; private set; }

        #endregion

        #region DefaultValueConstraint

        public DefaultValueConstraint CreateDefaultValueConstraint(string defaultValue, string missingValue, ConstraintProviderSource provider, string constraintSelectionPredicate)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(defaultValue));
            Contract.Requires(!string.IsNullOrWhiteSpace(missingValue));
            Contract.Requires(provider == ConstraintProviderSource.External ? (!string.IsNullOrWhiteSpace(constraintSelectionPredicate)) : true);
            Contract.Ensures(Contract.Result<DefaultValueConstraint>() != null && Contract.Result<DefaultValueConstraint>().Id >= 0);

            DefaultValueConstraint u = new DefaultValueConstraint()
            {
                DefaultValue = defaultValue,
                MissingValue = missingValue,
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DefaultValueConstraint> repo = uow.GetRepository<DefaultValueConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);            
        }

        public bool DeleteDefaultValueConstraint(DefaultValueConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DefaultValueConstraint> repo = uow.GetRepository<DefaultValueConstraint>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteDefaultValueConstraint(IEnumerable<DefaultValueConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DefaultValueConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DefaultValueConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DefaultValueConstraint> repo = uow.GetRepository<DefaultValueConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public DefaultValueConstraint UpdateDefaultValueConstraint(DefaultValueConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DefaultValueConstraint>() != null && Contract.Result<DefaultValueConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<DefaultValueConstraint> repo = uow.GetRepository<DefaultValueConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion

        #region DomainValueConstraint

        public DomainValueConstraint CreateDomainValueConstraint(string domainValue, string description, ConstraintProviderSource provider, string constraintSelectionPredicate)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(domainValue));
            Contract.Requires(provider == ConstraintProviderSource.External ? (!string.IsNullOrWhiteSpace(constraintSelectionPredicate)) : true);
            Contract.Ensures(Contract.Result<DomainValueConstraint>() != null && Contract.Result<DomainValueConstraint>().Id >= 0);

            DomainValueConstraint u = new DomainValueConstraint()
            {
                DomainValue = domainValue,
                Description = description,
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainValueConstraint> repo = uow.GetRepository<DomainValueConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        public bool DeleteDomainValueConstraint(DomainValueConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainValueConstraint> repo = uow.GetRepository<DomainValueConstraint>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteDomainValueConstraint(IEnumerable<DomainValueConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DomainValueConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DomainValueConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainValueConstraint> repo = uow.GetRepository<DomainValueConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public DomainValueConstraint UpdateDomainValueConstraint(DomainValueConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DomainValueConstraint>() != null && Contract.Result<DomainValueConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<DomainValueConstraint> repo = uow.GetRepository<DomainValueConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region ValidatorConstraint

        public ValidatorConstraint CreateValidatorConstraint(string body, ConstraintProviderSource provider, string constraintSelectionPredicate)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(body));
            Contract.Requires(provider == ConstraintProviderSource.External ? (!string.IsNullOrWhiteSpace(constraintSelectionPredicate)) : true);
            Contract.Ensures(Contract.Result<ValidatorConstraint>() != null && Contract.Result<ValidatorConstraint>().Id >= 0);

            ValidatorConstraint u = new ValidatorConstraint()
            {
                Body = body,                
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ValidatorConstraint> repo = uow.GetRepository<ValidatorConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        public bool DeleteValidatorConstraint(ValidatorConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ValidatorConstraint> repo = uow.GetRepository<ValidatorConstraint>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteValidatorConstraint(IEnumerable<ValidatorConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ValidatorConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ValidatorConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ValidatorConstraint> repo = uow.GetRepository<ValidatorConstraint>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public ValidatorConstraint UpdateValidatorConstraint(ValidatorConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<ValidatorConstraint>() != null && Contract.Result<ValidatorConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<ValidatorConstraint> repo = uow.GetRepository<ValidatorConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion
    }
}
