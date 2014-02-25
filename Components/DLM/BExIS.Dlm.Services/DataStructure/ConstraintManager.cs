using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public sealed class ConstraintManager
    {
        //public ConstraintManager() 
        //{
        //    IUnitOfWork uow = this.GetUnitOfWork();
        //    this.DefaultValueRepo = uow.GetReadOnlyRepository<DefaultValueConstraint>();
        //    this.DomainValueRepo = uow.GetReadOnlyRepository<DomainValueConstraint>();
        //    this.ValidatorRepo = uow.GetReadOnlyRepository<ValidatorConstraint>();
        //}

        #region Data Readers

        //public IReadOnlyRepository<DefaultValueConstraint> DefaultValueRepo { get; private set; }
        //public IReadOnlyRepository<DomainValueConstraint> DomainValueRepo { get; private set; }
        //public IReadOnlyRepository<ValidatorConstraint> ValidatorRepo { get; private set; }

        #endregion

        #region DomainConstraint

        public DomainConstraint CreateDomainConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, List<DomainItem> items, DataContainer container)
        {
            Contract.Requires(items != null);
            Contract.Requires(items.Count > 0);


            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0);

            DomainConstraint u = new DomainConstraint()
            {
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
                CultureId = cultureId,
                Description = description,
                Negated = negated,
                Context = context!= null? context: "Default",
                MessageTemplate = messageTemplate,
                NegatedMessageTemplate = negatedMessageTemplate,
                Items = items,
                DataContainer = container,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
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
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<DomainConstraint> entities)
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

        public DomainConstraint Update(DomainConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<DomainConstraint>() != null && Contract.Result<DomainConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DomainConstraint> repo = uow.GetRepository<DomainConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region PatternConstraint

        public PatternConstraint CreatePatternConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, string matchingPhrase, bool caseSensitive, DataContainer container)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(matchingPhrase));

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0);

            PatternConstraint u = new PatternConstraint()
            {
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
                CultureId = cultureId,
                Description = description,
                Negated = negated,
                Context = context != null ? context : "Default",
                MessageTemplate = messageTemplate,
                NegatedMessageTemplate = negatedMessageTemplate,
                MatchingPhrase = matchingPhrase,
                CaseSensitive = caseSensitive,
                DataContainer = container,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
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

        public bool Delete(IEnumerable<PatternConstraint> entities)
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

        public PatternConstraint Update(PatternConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PatternConstraint>() != null && Contract.Result<PatternConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PatternConstraint> repo = uow.GetRepository<PatternConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region RangeConstraint

        public RangeConstraint CreateRangeConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, double lowerbound, bool lowerboundIncluded
            , double upperbound, bool upperboundIncluded, DataContainer container)
        {
            Contract.Requires(lowerbound <= upperbound);

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0);

            RangeConstraint u = new RangeConstraint()
            {
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
                CultureId = cultureId,
                Description = description,
                Negated = negated,
                Context = context != null ? context : "Default",
                MessageTemplate = messageTemplate,
                NegatedMessageTemplate = negatedMessageTemplate,
                Lowerbound = lowerbound,
                LowerboundIncluded = lowerboundIncluded,
                Upperbound = upperbound,
                UpperboundIncluded = upperboundIncluded,
                DataContainer = container,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
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

        public bool Delete(IEnumerable<RangeConstraint> entities)
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

        public RangeConstraint Update(RangeConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<RangeConstraint>() != null && Contract.Result<RangeConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RangeConstraint> repo = uow.GetRepository<RangeConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region CompareConstraint

        public CompareConstraint CreateCompareConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, ComparisonOperator comparisonOperator, ComparisonTargetType targetType
            , string target, ComparisonOffsetType offsetType, double offset, DataContainer container)
        {
            //Contract.Requires();

            Contract.Ensures(Contract.Result<CompareConstraint>() != null && Contract.Result<CompareConstraint>().Id >= 0);

            CompareConstraint u = new CompareConstraint()
            {
                Provider = provider,
                ConstraintSelectionPredicate = constraintSelectionPredicate,
                CultureId = cultureId,
                Description = description,
                Negated = negated,
                Context = context != null ? context : "Default",
                MessageTemplate = messageTemplate,
                NegatedMessageTemplate = negatedMessageTemplate,
                Operator = comparisonOperator,
                TargetType = targetType,
                Target = target,
                OffsetType = offsetType,
                OffsetValue = offset,
                DataContainer = container,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<CompareConstraint> repo = uow.GetRepository<CompareConstraint>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        public bool Delete(CompareConstraint entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<CompareConstraint> repo = uow.GetRepository<CompareConstraint>();

                entity = repo.Reload(entity);
                //delete the unit
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<CompareConstraint> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (CompareConstraint e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (CompareConstraint e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<CompareConstraint> repo = uow.GetRepository<CompareConstraint>();

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

        public CompareConstraint Update(CompareConstraint entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<CompareConstraint>() != null && Contract.Result<CompareConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<CompareConstraint> repo = uow.GetRepository<CompareConstraint>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion
    
    }
}
