using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
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
                IRepository<PartyRelationship> repoRel = uow.GetRepository<PartyRelationship>();
                var latest = repo.Reload(entity);
                // remove all associations between the entity and its history items
                repoCM.Delete(latest.History);
                if (entity.History.Any())
                {
                    entity.History.ToList().ForEach(a => a.Party = null);
                    entity.History.Clear();
                }
                //remove all relations
                var relations = repoRel.Get(item => item.FirstParty.Id == entity.Id || item.SecondParty.Id == entity.Id);
                foreach (var relation in relations)
                {
                    repoRel.Delete(relation);
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
            Contract.Requires(entity != null, "Provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "Provided entity must have a permanent ID");
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
                                    string title, string description, DateTime? startDate = null, DateTime? endDate = null, string scope = "")
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(firstParty != null);
            Contract.Requires(firstParty.Id >= 0, "Provided first entity must have a permanent ID");
            Contract.Requires(secondParty != null);
            Contract.Requires(secondParty.Id >= 0, "Provided first entity must have a permanent ID");
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
                IRepository<PartyRelationshipType> repoRelType = uow.GetRepository<PartyRelationshipType>();
                partyRelationshipType = repoRelType.Reload(partyRelationshipType);
                //Check if there is another relationship
                var cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                        && (item.FirstParty != null && item.FirstParty.Id == firstParty.Id)
                                         && (item.SecondParty != null && item.SecondParty.Id == secondParty.Id)).Count();
                if (cnt > 0)
                    PartyManager.ThrowException(entity, "This relationship is already exist in database.", ExceptionType.Add);
                //Check maximun cardinality
                if (partyRelationshipType.MaxCardinality <= partyRelationshipType.PartyRelationships.Count())
                    PartyManager.ThrowException(entity, string.Format("Maximum relations for this type of relation is {0}.", partyRelationshipType.MaxCardinality), PartyManager.ExceptionType.Add);

                //Check if there is a relevant party type pair
                var alowedSource = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.AlowedSource == firstParty.PartyType || item.AlowedSource == secondParty.PartyType);
                var alowedTarget = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.AlowedTarget == firstParty.PartyType || item.AlowedTarget == secondParty.PartyType);
                if (alowedSource == null || alowedTarget == null)
                    PartyManager.ThrowException(entity, string.Format("There is not relevant 'PartyTypePair' for these types of parties.", partyRelationshipType.MaxCardinality), PartyManager.ExceptionType.Add);

                
                partyRelationshipType.PartyRelationships.Add(entity);
                repoPR.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public bool RemovePartyRelationship(PartyRelationship partyRelationship)
        {
            Contract.Requires(partyRelationship != null);
            Contract.Requires(partyRelationship.Id >= 0, "Provided entity must have a permanent ID");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                partyRelationship = repoPR.Reload(partyRelationship);
                if (partyRelationship.PartyRelationshipType.MinCardinality > (partyRelationship.PartyRelationshipType.PartyRelationships.Count() - 1))
                    ThrowException(partyRelationship, String.Format("Atleast {0} party relation is required.", partyRelationship.PartyRelationshipType.MinCardinality), ExceptionType.Delete);
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
                    if (entity.PartyRelationshipType.MinCardinality > (entity.PartyRelationshipType.PartyRelationships.Count() - 1))
                        ThrowException(entity, String.Format("Atleast {0} party relation is required.", entity.PartyRelationshipType.MinCardinality), ExceptionType.Delete, true);
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
            Contract.Requires(partyCustomAttribute.Id >= 0, "Provided Custom Attribute entity must have a permanent identifier.");
            Contract.Requires(party != null);
            Contract.Requires(party.Id >= 0, "Provided party entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PartyCustomAttributeValue>() != null && Contract.Result<PartyCustomAttributeValue>().Id >= 0);

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
                //if there is no attribute value when it created
                if (partyEntity.CustomAttributeValues == null)
                    partyEntity.CustomAttributeValues = new List<PartyCustomAttributeValue>();
                //check if there is the same custom attribute for this party update it
                var similarPartyCustomAttr = partyEntity.CustomAttributeValues.FirstOrDefault(item => item.CustomAttribute.Id == partyCustomAttribute.Id);
                if (similarPartyCustomAttr != null)
                {
                    similarPartyCustomAttr.Value = value;
                    entity = similarPartyCustomAttr;
                }
                else
                {
                    partyEntity.CustomAttributeValues.Add(entity);
                    repoCAV.Put(entity);
                }
                uow.Commit();
            }
            return (entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="party"></param>
        /// <param name="partyCustomAttributeValues"></param>
        /// <returns></returns>
        public IEnumerable<PartyCustomAttributeValue> AddPartyCustomAttriuteValue(PartyX party, Dictionary<PartyCustomAttribute, string> partyCustomAttributeValues)
        {
            Contract.Requires(partyCustomAttributeValues != null);
            Contract.Requires(party != null);
            Contract.Requires(party.Id >= 0, "Provided party entity must have a permanent ID");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyCustomAttributeValue> repoCAV = uow.GetRepository<PartyCustomAttributeValue>();
                party = repo.Reload(party);

                foreach (var partyCustomAttributeValue in partyCustomAttributeValues)
                {
                    //check if there is the same custom attribute for this party update it
                    var similarPartyCustomAttr = party.CustomAttributeValues.FirstOrDefault(item => item.CustomAttribute.Id == partyCustomAttributeValue.Key.Id);
                    if (similarPartyCustomAttr != null)
                        similarPartyCustomAttr.Value = partyCustomAttributeValue.Value;
                    else
                    {
                        var entity = new PartyCustomAttributeValue()
                        {
                            CustomAttribute = partyCustomAttributeValue.Key,
                            Value = partyCustomAttributeValue.Value
                        };
                        party.CustomAttributeValues.Add(entity);
                        repoCAV.Put(entity);
                    }
                }
                uow.Commit();
            }
            return party.CustomAttributeValues;
        }

        public bool RemovePartyCustomAttriuteValue(PartyCustomAttributeValue partyCustomAttributeValue)
        {
            Contract.Requires(partyCustomAttributeValue != null);
            Contract.Requires(partyCustomAttributeValue.Id >= 0, "Provided entity must have a permanent ID");
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
        public PartyStatus AddPartyStatus(PartyX party, PartyStatusType partyStatusType, string description)
        {
            Contract.Requires(party != null);
            Contract.Requires(partyStatusType != null);
            Contract.Ensures(Contract.Result<PartyStatus>() != null && Contract.Result<PartyStatus>().Id >= 0);
            var entity = new PartyStatus()
            {
                Description = description,
                Party = party,
                StatusType = partyStatusType,
                Timestamp = DateTime.Now
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatus> repoStatus = uow.GetRepository<PartyStatus>();

                repoStatus.Put(entity);
                // The current status must get updated, too. dependes on the current status's update logic.
                uow.Commit();
            }
            return (entity);
        }

        /// <summary>
        /// There is no need to delete party status
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool RemovePartyStatus(PartyStatus entity)
        {
            Contract.Requires(entity != null && entity.Id >= 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatus> repo = uow.GetRepository<PartyStatus>();
                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        /// <summary>
        /// There is no need to delete party status
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool RemovePartyStatus(IEnumerable<PartyStatus> entities)
        {
            Contract.Requires(entities != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatus> repo = uow.GetRepository<PartyStatus>();
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }
        #endregion

        public static void ThrowException(object entityObj, string reason, ExceptionType exceptionType, bool GroupFailed = false)
        {
            var entity = (BaseEntity)entityObj;
            throw new Exception(string.Format("{0} {1} (ID: {2}) failed. Reason: {3} . {4}", exceptionType.ToString(), entityObj.GetType().Name, entity.Id, reason, GroupFailed ? " All operations canceled" : ""));

        }

        public enum ExceptionType
        {
            Add = 0,
            Edit = 1,
            Delete = 2
        }
    }
}
