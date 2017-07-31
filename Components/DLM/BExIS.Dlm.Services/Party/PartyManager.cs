using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using BExIS.Ext.Model;
using Vaiona.Persistence.Api;
using PartyX = BExIS.Dlm.Entities.Party.Party;

namespace BExIS.Dlm.Services.Party
{
    public sealed class PartyManager
    {
        // Managing Party , PartyCustomAttributeValue,  PartyStatus, PartyRelationship
        #region Attributes

        public IReadOnlyRepository<PartyX> Repo { get; private set; }
        public IReadOnlyRepository<PartyCustomAttributeValue> RepoCustomAttrValues { get; private set; }
        public IReadOnlyRepository<PartyRelationship> RepoPartyRelationships { get; private set; }
        #endregion

        #region Ctors

        public PartyManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<PartyX>();
            RepoCustomAttrValues = uow.GetReadOnlyRepository<PartyCustomAttributeValue>();
            RepoPartyRelationships = uow.GetReadOnlyRepository<PartyRelationship>();
        }

        #endregion

        #region Methods

        //Currently there is no need to use name due to the conversation in a project meeting on December</param>
        public PartyX Create(PartyType partyType, string alias, string description, DateTime? startDate, DateTime? endDate, PartyStatusType initialStatusType)
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(partyType != null);
            Contract.Requires(initialStatusType != null);
            Contract.Requires(partyType.StatusTypes.Contains(initialStatusType));
            Contract.Ensures(Contract.Result<PartyX>() != null && Contract.Result<PartyX>().Id >= 0);
            if (startDate == null)
                startDate = DateTime.MinValue;
            if (endDate == null || endDate == DateTime.MinValue)
                endDate = DateTime.MaxValue;
            if (endDate <= startDate)
                BexisException.Throw(null, "End date should be equal or greater than start date.");

            //Create a create status
            PartyStatus initialStatus = new PartyStatus();
            initialStatus.Timestamp = DateTime.UtcNow;
            initialStatus.Description = "Created";
            initialStatus.StatusType = initialStatusType;

            PartyX entity = new PartyX()
            {
                PartyType = partyType,
                Alias = alias,
                Description = description,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                CurrentStatus = initialStatus
            };
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
                IRepository<PartyRelationship> repoRel = uow.GetRepository<PartyRelationship>();

                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyStatus> repoCM = uow.GetRepository<PartyStatus>();
                IRepository<PartyCustomAttributeValue> repoCustomeAttrVal = uow.GetRepository<PartyCustomAttributeValue>();

                var latest = repo.Reload(entity);
                // remove all associations between the entity and its history items
                //remove all relations
                var relations = repoRel.Get(item => item.FirstParty.Id == entity.Id || item.SecondParty.Id == entity.Id);
                repoRel.Delete(relations);
                repoCM.Delete(latest.History);
                if (latest.History.Count() > 0)
                {
                    latest.History.ToList().ForEach(a => a.Party = null);
                    latest.History.Clear();
                }
                //remove all 'CustomAttributeValues'
                repoCustomeAttrVal.Delete(latest.CustomAttributeValues);
                latest.CustomAttributeValues.Clear();




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
                entity = repo.Reload(entity);
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
            if (endDate <= startDate)
                BexisException.Throw(firstParty, "End date should be greater than start date.");
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
                var cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                        && (item.FirstParty != null && item.FirstParty.Id == firstParty.Id)
                                         && (item.SecondParty != null && item.SecondParty.Id == secondParty.Id)).Count();
                //Check maximun cardinality
                if (partyRelationshipType.MaxCardinality != -1 && partyRelationshipType.MaxCardinality <= cnt)
                    BexisException.Throw(entity, string.Format("Maximum relations for this type of relation is {0}.", partyRelationshipType.MaxCardinality), BexisException.ExceptionType.Add);

                //Check if there is a relevant party type pair
                var alowedSource = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.AllowedSource == firstParty.PartyType || item.AllowedSource == secondParty.PartyType);
                var alowedTarget = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.AllowedTarget == firstParty.PartyType || item.AllowedTarget == secondParty.PartyType);
                if (alowedSource == null || alowedTarget == null)
                    BexisException.Throw(entity, "There is not relevant 'PartyTypePair' for these types of parties.", BexisException.ExceptionType.Add);
                partyRelationshipType.PartyRelationships.Add(entity);
                repoPR.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public Boolean UpdatePartyRelationship(long id, string title, string description, DateTime startDate, DateTime endDate, string scope)
        {
            Contract.Requires(!string.IsNullOrEmpty(title), "Title can not be empty");
            Contract.Requires(id >= 0, "a permanent ID is required.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repo = uow.GetRepository<PartyRelationship>();
                var entity = repo.Get(id);
                entity.Title = title;
                entity.Description = description;
                entity.StartDate = startDate;
                entity.EndDate = endDate;
                entity.Scope = scope;
                repo.Put(entity);
                uow.Commit();
                entity = repo.Reload(entity);
            }
            return true;
        }

        public bool RemovePartyRelationship(PartyRelationship partyRelationship)
        {
            Contract.Requires(partyRelationship != null);
            Contract.Requires(partyRelationship.Id >= 0, "Provided entity must have a permanent ID");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                partyRelationship = repoPR.Reload(partyRelationship);
                var cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationship.PartyRelationshipType.Id)
                                      && (item.FirstParty != null && item.FirstParty.Id == partyRelationship.FirstParty.Id)
                                       && (item.SecondParty != null && item.SecondParty.Id == partyRelationship.SecondParty.Id)).Count();
                if (partyRelationship.PartyRelationshipType.MinCardinality >= cnt)
                    BexisException.Throw(partyRelationship, String.Format("Atleast {0} party relation is required.", partyRelationship.PartyRelationshipType.MinCardinality), BexisException.ExceptionType.Delete);
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
                        BexisException.Throw(entity, String.Format("Atleast {0} party relation is required.", entity.PartyRelationshipType.MinCardinality), BexisException.ExceptionType.Delete, true);
                    var latest = repoPR.Reload(entity);
                    repoPR.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }


        #endregion

        #region Associations
        //It's not checking uniqeness when it is not for a single custom attribute because it couldn't predict other values--> it should have all values to make a hash
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
                IRepository<PartyCustomAttribute> repoCA = uow.GetRepository<PartyCustomAttribute>();
                var partyEntity = repo.Reload(party);
                //check uniqeness if there is just one uniqe custome attribute
                if (partyCustomAttribute.IsUnique)
                {
                    var customAttributes = repoCA.Get(item => item.PartyType == partyEntity.PartyType);
                    if (customAttributes.Where(item => item.IsUnique).Count() == 1)
                        if (!CheckUniqueness(repo, party.PartyType, value, party))
                            BexisException.Throw(party, String.Format("Due the party uniqueness policy for this party type this value couldn't save"), BexisException.ExceptionType.Add, true);
                }
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
        public IEnumerable<PartyCustomAttributeValue> AddPartyCustomAttriuteValues(PartyX party, Dictionary<PartyCustomAttribute, string> partyCustomAttributeValues)
        {
            Contract.Requires(partyCustomAttributeValues != null);
            Contract.Requires(party != null);
            Contract.Requires(party.Id >= 0, "Provided party entity must have a permanent ID");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyCustomAttributeValue> repoCAV = uow.GetRepository<PartyCustomAttributeValue>();
                IRepository<PartyCustomAttribute> repoCA = uow.GetRepository<PartyCustomAttribute>();
                party = repo.Reload(party);
                if (!CheckUniqueness(repo, party.PartyType, partyCustomAttributeValues, party))
                    BexisException.Throw(party, String.Format("Due the party uniqueness policy for this party type this value couldn't save"), BexisException.ExceptionType.Add, true);
                foreach (var partyCustomAttributeValue in partyCustomAttributeValues)
                {
                    //check if there is the same custom attribute for this party update it
                    var entity = party.CustomAttributeValues.FirstOrDefault(item => item.CustomAttribute.Id == partyCustomAttributeValue.Key.Id);
                    if (entity != null)
                        entity.Value = partyCustomAttributeValue.Value;
                    else
                    {
                        entity = new PartyCustomAttributeValue()
                        {
                            CustomAttribute = partyCustomAttributeValue.Key,
                            Party = party,
                            Value = partyCustomAttributeValue.Value
                        };
                    }
                    repoCAV.Put(entity);
                }
                uow.Commit();
            }
            return party.CustomAttributeValues;
        }

        public PartyX GetParty(long id)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var partyRepo = uow.GetRepository<PartyX>();
                return partyRepo.Get(id);
            }
        }

        public PartyCustomAttributeValue UpdatePartyCustomAttriuteValue(PartyCustomAttributeValue partyCustomAttributeValue, string newValue)
        {
            Contract.Requires(partyCustomAttributeValue != null && partyCustomAttributeValue.Id != 0, "Provided entities can not be null");
            Contract.Ensures(Contract.Result<PartyCustomAttributeValue>() != null && Contract.Result<PartyCustomAttributeValue>().Id >= 0, "No entity is persisted!");
            // Check uniqeness policy is not possible in single updating 
            var entity = new PartyCustomAttributeValue();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                entity = repo.Get(item => item.Id == partyCustomAttributeValue.Id).FirstOrDefault();
                entity.Value = newValue;
                var partyCustomAttrVals = entity.Party.CustomAttributeValues.ToList();
                partyCustomAttrVals.First(item => item.Id == entity.Id).Value = newValue;
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        public PartyCustomAttributeValue UpdatePartyCustomAttriuteValues(List<PartyCustomAttributeValue> partyCustomAttributeValues)
        {
            Contract.Requires(partyCustomAttributeValues != null && partyCustomAttributeValues.Any(), "Provided entities can not be null");
            Contract.Ensures(Contract.Result<PartyCustomAttributeValue>() != null && Contract.Result<PartyCustomAttributeValue>().Id >= 0, "No entity is persisted!");
            if (!CheckUniqueness(this.Repo, partyCustomAttributeValues, partyCustomAttributeValues.First().Party))
                BexisException.Throw(partyCustomAttributeValues.First(), String.Format("Due the party uniqueness policy for this party type this value couldn't save"), BexisException.ExceptionType.Edit, true);
            var entity = new PartyCustomAttributeValue();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                foreach (var partyCustomAttrVal in partyCustomAttributeValues)
                {
                    entity = repo.Get(partyCustomAttrVal.Id);
                    entity.Value = partyCustomAttrVal.Value;
                    repo.Put(entity); // Merge is required here!!!!                   
                }
                uow.Commit();

            }
            return (entity);
        }
        //public PartyCustomAttributeValue UpdatePartyCustomAttriuteValue(PartyCustomAttribute partyCustomAttribute, PartyX party, string value)
        //{
        //    Contract.Requires(partyCustomAttribute != null && party != null, "Provided entities can not be null");
        //    Contract.Requires(partyCustomAttribute.Id >= 0 && party.Id >= 0, "Provided entitities must have a permanent ID");
        //    Contract.Ensures(Contract.Result<PartyCustomAttributeValue>() != null && Contract.Result<PartyCustomAttributeValue>().Id >= 0, "No entity is persisted!");
        //    var entity = new PartyCustomAttributeValue();

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
        //        entity = repo.Get(item => item.Party.Id == party.Id && item.CustomAttribute.Id == partyCustomAttribute.Id).FirstOrDefault();
        //        entity.Value = value;
        //        repo.Put(entity); // Merge is required here!!!!
        //        uow.Commit();
        //    }
        //    var partyCustomAttributeValues = Repo.Get(party.Id).CustomAttributeValues.ToList();
        //        if (!CheckUniqueness(party.PartyType, partyCustomAttributeValues, party))
        //            BexisException.Throw(party, String.Format("Due the party uniqueness policy for this party type this value couldn't save"), BexisException.ExceptionType.Edit, true);

        //    return (entity);

        //}


        public bool RemovePartyCustomAttriuteValue(PartyCustomAttributeValue partyCustomAttributeValue)
        {
            Contract.Requires(partyCustomAttributeValue != null);
            Contract.Requires(partyCustomAttributeValue.Id >= 0, "Provided entity must have a permanent ID");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                var entity = repo.Reload(partyCustomAttributeValue);
                //Uniqeness policy
                repo.Delete(entity);
                uow.Commit();

            }
            return (true);
        }

        public bool RemovePartyCustomAttriuteValues(IEnumerable<PartyCustomAttributeValue> entities)
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

        #region Account
        public void AddPartyUser(PartyX party, long userId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var partyUser = new PartyUser();
                partyUser.UserId = userId;
                partyUser.PartyId = party.Id;
                partyUser.Party = party;
                IRepository<PartyUser> repo = uow.GetRepository<PartyUser>();
                repo.Put(partyUser);
                uow.Commit();
            }
        }

        //public PartyX GetPartyByUser(int userId)
        //{
        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<PartyUser> repo = uow.GetRepository<PartyUser>();
        //        return repo.Get(c => c.UserId == userId).Select(c=>c.Party).FirstOrDefault();
        //    }
        //}

        public long GetUserIdByParty(int partyId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyUser> repo = uow.GetRepository<PartyUser>();
                return repo.Get(c => c.PartyId == partyId).Select(c => c.UserId).FirstOrDefault();
            }
        }
        #endregion 

        /// <summary>
        /// make a hash from isUniqe custom attributes and check it with all of the other parties hash 
        /// </summary>
        /// <param name="partyType"></param>
        /// <param name="partyCustomAttrVals"></param>
        /// <returns></returns>
        public bool CheckUniqueness(IReadOnlyRepository<PartyX> repo, PartyType partyType, string hash, PartyX currentParty = null)
        {

            if (!string.IsNullOrEmpty(hash))
            {
                var parties = repo.Get(item => item.PartyType.Id == partyType.Id);
                if (currentParty != null && currentParty.Id != 0)
                    parties = parties.Where(item => item.Id != currentParty.Id).ToList();
                foreach (var party in parties)
                    if (hash == GetHash(party.CustomAttributeValues.ToList()))
                        return false;
            }
            return true;
        }

        /// <summary>
        /// make a hash from isUniqe custom attributes and check it with all of the other parties hash 
        /// </summary>
        /// <param name="partyType"></param>
        /// <param name="partyCustomAttrVals"></param>
        /// <returns></returns>
        public bool CheckUniqueness(IReadOnlyRepository<PartyX> repo, List<PartyCustomAttributeValue> partyCustomAttrVals, PartyX currentParty = null)
        {
            Contract.Requires(partyCustomAttrVals.Any(), "Provided list must have one entity at least.");
            var partyTypeId = partyCustomAttrVals.First().CustomAttribute.PartyType.Id;
            string hash = GetHash(partyCustomAttrVals);
            if (!string.IsNullOrEmpty(hash))
            {
                var parties = repo.Get(item => item.PartyType.Id == partyTypeId);
                if (currentParty != null && currentParty.Id != 0)
                    parties = parties.Where(item => item.Id != currentParty.Id).ToList();
                foreach (var party in parties)
                    if (hash == GetHash(party.CustomAttributeValues.ToList()))
                        return false;
            }
            return true;
        }

        /// <summary>
        /// make a hash from isUniqe custom attributes and check it with all of the other parties hash 
        /// </summary>
        /// <param name="partyType"></param>
        /// <param name="partyCustomAttrVals"></param>
        /// <returns></returns>
        public bool CheckUniqueness(IReadOnlyRepository<PartyX> repo, PartyType partyType, Dictionary<PartyCustomAttribute, string> partyCustomAttrVals, PartyX currentParty = null)
        {
            string hash = GetHash(partyCustomAttrVals);
            if (!string.IsNullOrEmpty(hash))
            {
                var parties = repo.Get(item => item.PartyType.Id == partyType.Id);
                if (currentParty != null && currentParty.Id != 0)
                    parties = parties.Where(item => item.Id != currentParty.Id).ToList();
                foreach (var party in parties)
                    if (hash == GetHash(party.CustomAttributeValues.ToList()))
                        return false;
            }
            return true;
        }


        #region privateMethod
        private string GetHash(Dictionary<PartyCustomAttribute, string> partyCustomAttrVals)
        {
            string hash = "";
            foreach (var partyCustomAttr in partyCustomAttrVals.Where(item => item.Key.IsUnique))
                hash += partyCustomAttr.Value;
            return hash;
        }

        private string GetHash(List<PartyCustomAttributeValue> partyCustomAttrVals)
        {
            string hash = "";
            foreach (var partyCustomAttr in partyCustomAttrVals.Where(item => item.CustomAttribute.IsUnique))
                hash += partyCustomAttr.Value;
            return hash;
        }
        #endregion privateMethod
    }
}
