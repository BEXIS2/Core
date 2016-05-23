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
        public PartyX Create(PartyType partyType, string name, string alias, string description, DateTime? startDate, DateTime? endDate, PartyStatusType statusType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(partyType != null);
            Contract.Requires(statusType != null);
            Contract.Requires(partyType.StatusTypes.Contains(statusType));
            Contract.Ensures(Contract.Result<PartyX>() != null && Contract.Result<PartyX>().Id >= 0);
            if (startDate == null)
                startDate = DateTime.MinValue;
            if (endDate == null)
                endDate = DateTime.MaxValue;
            //Create a create status
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
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                CurrentStatus = initialStatus
            };
            //if we set cascade to all in party mapping fileto save history at the same time of creating party
            initialStatus.Party = entity;
            entity.History = new List<PartyStatus>();
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
                var latest = repo.Reload(entity);
                // remove all associations between the entity and its history items
                repoCM.Delete(latest.History);
                if (entity.History.Any())
                {
                    entity.History.ToList().ForEach(a => a.Party = null);
                    entity.History.Clear();
                }
                //delete the entity
                repo.Delete(latest);
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
                    // remove all associations between the entity and its history items
                    repoCM.Delete(latest.History);
                    entity.History.ToList().ForEach(a => a.Party = null);
                    entity.History.Clear();
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




        #region PartyRelationship
        public PartyRelationship AddPartyRelationship(PartyX firstParty, PartyX secondParty, PartyRelationshipType partyRelationshipType,
                                   DateTime? startDate, DateTime? endDate, string title, string description, string scope)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(firstParty != null);
            Contract.Requires(firstParty.Id >= 0, "provided first entity must have a permanent ID");
            Contract.Requires(secondParty != null);
            Contract.Requires(secondParty.Id >= 0, "provided first entity must have a permanent ID");
            Contract.Requires(partyRelationshipType != null && partyRelationshipType.Id > 0);
            Contract.Ensures(Contract.Result<PartyRelationship>() != null && Contract.Result<PartyRelationship>().Id >= 0);
            if (startDate == null)
                startDate = DateTime.MinValue;
            if (endDate == null)
                endDate = DateTime.MaxValue;
            var entity = new PartyRelationship()
            {
                Description = description,
                EndDate = endDate.Value,
                FirstParty = firstParty,
                PartyRelationshipType = partyRelationshipType,
                Scope = scope,
                SecondParty = secondParty,
                StartDate = startDate.Value,
                Title = title
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                //Check if there is another relationship
                if (repoPR.Get(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                        && (item.FirstParty != null && item.FirstParty.Id == firstParty.Id)
                                         && (item.SecondParty != null && item.SecondParty.Id == secondParty.Id)).Count > 0)
                    throw new Exception("This relationship is already exist in database.");
                repoPR.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        public bool RemovePartyRelationship(PartyRelationship partyRelationship)
        {
            Contract.Requires(partyRelationship != null);
            Contract.Requires(partyRelationship.Id >= 0, "provided entity must have a permanent ID"); 
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                var entity = repoPR.Reload(partyRelationship);
                repoPR.Delete(entity);
                uow.Commit();
            }
            return (true);
        }
        public bool RemovePartyRelationship(IEnumerable<PartyRelationship> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyRelationship e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyRelationship e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                foreach (var entity in entities)
                {
                    var latest = repoPR.Reload(entity);
                    repoPR.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }
        #endregion

        #region Associations

        public PartyCustomAttributeValue AddPartyCustomAttriuteValue(PartyX party, PartyCustomAttribute partyCustomAttribute, string value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(partyCustomAttribute != null);
            Contract.Requires(partyCustomAttribute.Id >= 0, "provided Custom Attribute entity must have a permanent ID");
            Contract.Requires(party != null);
            Contract.Requires(party.Id >= 0, "provided party entity must have a permanent ID");
            var entity = new PartyCustomAttributeValue()
            {

                CustomAttribute = partyCustomAttribute,
                Value = value
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyCustomAttributeValue> repoCAV = uow.GetRepository<PartyCustomAttributeValue>();
                var partyEntity = repo.Reload(party);
                //there is no attribute value when it created
                if (partyEntity.CustomAttributeValues == null)
                    partyEntity.CustomAttributeValues = new List<PartyCustomAttributeValue>();
                partyEntity.CustomAttributeValues.Add(entity);
                repoCAV.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public bool RemovePartyCustomAttriuteValue(PartyCustomAttributeValue partyCustomAttributeValue)
        {
            Contract.Requires(partyCustomAttributeValue != null);
            Contract.Requires(partyCustomAttributeValue.Id >= 0, "provided entity must have a permanent ID");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                var entity = repo.Reload(partyCustomAttributeValue);
                repo.Delete(entity);
                uow.Commit();
            }
            return (true);
        }
        public bool RemovePartyCustomAttriuteValue(IEnumerable<PartyCustomAttributeValue> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyCustomAttributeValue e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyCustomAttributeValue e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(entity);
                }
                uow.Commit();
            }
            return (true);
        }
        #endregion
    }
}
