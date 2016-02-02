using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using PartyX = BExIS.Dlm.Entities.Party.Party;

namespace BExIS.Dlm.Services.Party
{
    public sealed class PartyManager
    {
        #region Attributes
        
        public IReadOnlyRepository<PartyX> Repo { get; private set; }

        #endregion

        #region Ctors
        
        public PartyManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<PartyX>();
        }

        #endregion
        
        #region Methods
        public PartyX Create(PartyType partyType, string name, string alias, string description, DateTime startDate, DateTime endDate, PartyStatusType statusType, List<PartyCustomAttributeValue> customValues)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(partyType != null);
            Contract.Requires(statusType != null);
            Contract.Requires(partyType.StatusTypes.Contains(statusType));
            Contract.Ensures(Contract.Result<PartyX>() != null && Contract.Result<PartyX>().Id >= 0);

            PartyStatus initialStatus = new PartyStatus();
            initialStatus.Timestamp = DateTime.UtcNow;
            initialStatus.Description = "Created";
            initialStatus.StatusType = statusType;

            PartyX entity = new PartyX()
            {
                PartyType = partyType,
                Name = name,
                Alias = alias,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                CustomAttributeValues = customValues,
                CurrentStatus = initialStatus,
            };
            entity.History.Add(initialStatus);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                repo.Put(entity); // must store the status objects too
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(PartyX entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyStatus> repoCM = uow.GetRepository<PartyStatus>();

                entity = repo.Reload(entity);

                // delete the history
                repoCM.Delete(entity.History);
                repoCM.Delete(entity.CurrentStatus);

                // remove all associations between the entity and its history items
                entity.History.ToList().ForEach(a => a.Party = null);
                entity.History.Clear();

                //delete the entity
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<PartyX> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyX e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyX e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyStatus> repoCM = uow.GetRepository<PartyStatus>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    // delete the ststaus history
                    repoCM.Delete(latest.History);

                    // remove all associations between the party and its history items
                    latest.History.ToList().ForEach(a => a.Party = null);
                    latest.History.Clear();
                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public PartyX Update(PartyX entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PartyX>() != null && Contract.Result<PartyX>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }


        #endregion

        #region PartyType

        public PartyType Create(string title, string description, List<PartyStatusType> statusTypes)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(statusTypes != null && statusTypes.Count() > 0); // there should be at least one status defined for each party type
            Contract.Ensures(Contract.Result<PartyType>() != null && Contract.Result<PartyType>().Id >= 0);

            PartyType entity = new PartyType()
            {
                Title = title,
                Description = description,
                StatusTypes = statusTypes,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repo = uow.GetRepository<PartyType>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(PartyType entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repo = uow.GetRepository<PartyType>();
                IRepository<PartyStatusType> repoPST = uow.GetRepository<PartyStatusType>();
                IRepository<PartyCustomAttribute> repoCA = uow.GetRepository<PartyCustomAttribute>();

                // check whether the party type is used i party, status types are used in some statuses, and custom attributes too. if yes, the party type can ot bedeleted

                if (
                    entity.Parties.Count() > 0
                    || entity.StatusTypes.Any(p => p.Statuses.Count() > 0)
                    || entity.CustomAttributes.Any(p => p.CustomAttributeValues.Count() > 0)
                    )
                {
                    return false;
                }

                entity = repo.Reload(entity);
                // delete the history
                repoPST.Delete(entity.StatusTypes);
                repoCA.Delete(entity.CustomAttributes);

                // remove all associations between the entity and its history items
                // the status types must also be deleted, not just the association between them and the party type.
                entity.StatusTypes.ToList().ForEach(a => a.PartyType = null);
                entity.StatusTypes.Clear();
                // the custmom attributes must be also deleted.
                entity.CustomAttributes.ToList().ForEach(a => a.PartyType = null);
                entity.CustomAttributes.Clear();

                //delete the entity
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public PartyStatusType AddStatusType(PartyType partyType, string name, string description, string displayOrder)
        {
            // reorder the other status types that confclict with the displayOrder passed here
            return null;
        }

        public void RemoveStatusType(PartyType partyType, PartyStatusType statusType)
        {
            // there should remain at least 1 status type attached to the party type after removal of the passed one
            return;
        }
        #endregion
        
        #region Associations
        #endregion
    }
}
