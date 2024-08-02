using BExIS.Dlm.Entities.Party;
using BExIS.Ext.Model;
using NCalc;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using Vaiona.Persistence.Api;
using PartyX = BExIS.Dlm.Entities.Party.Party;

namespace BExIS.Dlm.Services.Party
{
    public sealed class PartyManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;
        // Managing Party , PartyCustomAttributeValue,  PartyStatus, PartyRelationship

        #region Ctors

        public PartyManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            PartyRepository = _guow.GetReadOnlyRepository<PartyX>();
            PartyCustomAttributeValueRepository = _guow.GetReadOnlyRepository<PartyCustomAttributeValue>();
            PartyRelationshipRepository = _guow.GetReadOnlyRepository<PartyRelationship>();
            PartyCustomGridColumnsRepository = _guow.GetReadOnlyRepository<PartyCustomGridColumns>();
        }

        ~PartyManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<PartyRelationship> PartyRelationshipRepository { get; }
        public IReadOnlyRepository<PartyCustomAttributeValue> PartyCustomAttributeValueRepository { get; }
        public IReadOnlyRepository<PartyX> PartyRepository { get; }
        public IReadOnlyRepository<PartyCustomGridColumns> PartyCustomGridColumnsRepository { get; }
        public IQueryable<PartyX> Parties => PartyRepository.Query();
        public IQueryable<PartyRelationship> PartyRelationships => PartyRelationshipRepository.Query();

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
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

        #endregion Ctors

        #region Methods

        public PartyX Find(long partyId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var repo = uow.GetReadOnlyRepository<PartyX>();
                return repo.Query(item => item.Id == partyId).FirstOrDefault();
            }
        }

        //Currently there is no need to use name due to the conversation in a project meeting on December</param>
        /// <summary>
        /// Create a party
        /// </summary>
        /// <param name="partyType"></param>
        /// <param name="alias"></param>
        /// <param name="description"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="initialStatusType"></param>
        /// <param name="isTemp"></param>
        /// <returns></returns>
        public PartyX Create(PartyType partyType, string alias, string description, DateTime? startDate, DateTime? endDate, PartyStatusType initialStatusType, bool isTemp = true)
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(partyType != null);
            Contract.Requires(initialStatusType != null);
            Contract.Requires(partyType.StatusTypes.Any(cc => cc.Id == initialStatusType.Id));
            Contract.Ensures(Contract.Result<PartyX>() != null && Contract.Result<PartyX>().Id >= 0);
            if (startDate == null)
                startDate = DateTime.MinValue;
            if (endDate == null || endDate == DateTime.MinValue)
                endDate = DateTime.MaxValue;
            if (startDate > endDate)
                BexisException.Throw(null, "End date should be greater than start date.");

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
                CurrentStatus = initialStatus,
                IsTemp = isTemp
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
            //bool addSystemRelationship = true;
            //if (addSystemRelationship)
            //    addSystemRelationships(entity);

            return (entity);
        }

        /// <summary>
        /// create a party with custom attribute values
        /// </summary>
        /// <param name="partyType">Party type</param>
        /// <param name="description">Description</param>
        /// <param name="startDate">Party start date</param>
        /// <param name="endDate">Party end date</param>
        /// <param name="partyCustomAttributeValues">Custom attribute values</param>
        /// <returns>Party</returns>
        public PartyX Create(PartyType partyType, string description, DateTime? startDate, DateTime? endDate, Dictionary<string, string> partyCustomAttributeValues)
        {
            using (var partyTypeManager = new PartyTypeManager())
            {
                var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                var party = Create(partyType, "", description, startDate, endDate, partyStatusType, false);
                AddPartyCustomAttributeValues(party, ConvertDictionaryToPartyCustomeAttrValuesDictionary(partyCustomAttributeValues, partyType));
                return PartyRepository.Reload(party);
            }
        }

        /// <summary>
        /// create a party with custom attribute values
        /// </summary>
        /// <param name="partyType">Party type</param>
        /// <param name="description">Description</param>
        /// <param name="startDate">Party start date</param>
        /// <param name="endDate">Party end date</param>
        /// <param name="partyCustomAttributeValues">Custom attribute values</param>
        /// <returns>Party</returns>
        public PartyX Create(PartyType partyType, string description, DateTime? startDate, DateTime? endDate, Dictionary<long, string> partyCustomAttributeValues)
        {
            using (var partyTypeManager = new PartyTypeManager())
            {
                var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                var party = Create(partyType, "", description, startDate, endDate, partyStatusType, false);
                AddPartyCustomAttributeValues(party, ConvertDictionaryToPartyCustomeAttrValuesDictionary(partyCustomAttributeValues));
                return PartyRepository.Reload(party);
            }
        }

        public bool Delete(PartyX entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoRel = uow.GetRepository<PartyRelationship>();
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                IRepository<PartyUser> repoPartyUser = uow.GetRepository<PartyUser>();
                IRepository<PartyStatus> repoCM = uow.GetRepository<PartyStatus>();
                IRepository<PartyCustomAttributeValue> repoCustomeAttrVal = uow.GetRepository<PartyCustomAttributeValue>();

                var latest = repo.Reload(entity);
                // remove all associations between the entity and its history items
                //remove all relations
                var relations = repoRel.Get(item => item.SourceParty.Id == entity.Id || item.TargetParty.Id == entity.Id);
                repoRel.Delete(relations);
                //Remove from user if there is
                repoPartyUser.Delete(repoPartyUser.Get(cc => cc.PartyId == entity.Id));
                //Remove all the histories
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

        public PartyX Update(PartyX newParty)
        {
            Contract.Requires(newParty != null, "Provided entity can not be null");
            Contract.Requires(newParty.Id >= 0, "Provided entity must have a permanent ID");
            Contract.Ensures(Contract.Result<PartyX>() != null && Contract.Result<PartyX>().Id >= 0, "No entity is persisted!");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var party = uow.GetReadOnlyRepository<PartyX>().Get(newParty.Id);

                if (party.StartDate == null)
                    party.StartDate = DateTime.MinValue;
                if (party.EndDate == null || party.EndDate == DateTime.MinValue)
                    party.EndDate = DateTime.MaxValue;
                if (party.StartDate > party.EndDate)
                    BexisException.Throw(null, "End date should be greater than start date.");
                if ((ValidateRelationships(party.Id)).Any())
                    party.IsTemp = true;
                else
                    party.IsTemp = false;

                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                repo.Merge(party);
                var merged = repo.Get(party.Id);
                repo.Put(merged);
                uow.Commit();

                return (party);
            }
        }

        public bool TempPartyToPermanent(long partyId)
        {
            Contract.Requires(partyId >= 0, "Provided entity must have a permanent ID");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                var party = repo.Get(partyId);
                party.IsTemp = false;
                repo.Put(party); // Merge is required here!!!!
                uow.Commit();
            }
            return true;
        }

        #endregion Methods

        #region PartyRelationship

        public PartyRelationship AddPartyRelationship(long firstPartyId, long secondPartyId,
                                    string title, string description, long partyTypePairId, DateTime? startDate = null, DateTime? endDate = null, string scope = "", int? permission = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(firstPartyId >= 0, "Provided first entity must have a permanent ID");
            Contract.Requires(secondPartyId >= 0, "Provided first entity must have a permanent ID");
            Contract.Requires(partyTypePairId > 0);
            Contract.Ensures(Contract.Result<PartyRelationship>() != null && Contract.Result<PartyRelationship>().Id >= 0);

            if (startDate == null)
                startDate = DateTime.MinValue;
            if (endDate == null)
                endDate = DateTime.MaxValue;
            if (startDate > endDate)
                BexisException.Throw(null, "End date should be greater than start date.");

            PartyRelationship entity = null;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repoParty = uow.GetRepository<PartyX>();
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                IRepository<PartyRelationshipType> repoRelType = uow.GetRepository<PartyRelationshipType>();

                PartyTypePair partyTypePair = uow.GetReadOnlyRepository<PartyTypePair>().Get(partyTypePairId);
                PartyX firstParty = repoParty.Get(firstPartyId);
                PartyX secondParty = repoParty.Get(secondPartyId);

                entity = new PartyRelationship()
                {
                    Description = description,
                    EndDate = endDate.Value,
                    SourceParty = firstParty,
                    //PartyRelationshipType = partyTypePair.PartyRelationshipType,
                    Scope = scope,
                    TargetParty = secondParty,
                    StartDate = startDate.Value,
                    Title = title,
                    Permission = permission.HasValue ? permission.Value : partyTypePair.PermissionTemplate
                };

                if (partyTypePair != null)
                {
                    entity.PartyTypePair = partyTypePair;
                    entity.PartyRelationshipType = partyTypePair.PartyRelationshipType;
                }

                //if(!repoRelType.IsLoaded(partyRelationshipType))
                //    partyRelationshipType = repoRelType.Reload(partyRelationshipType);
                var cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyTypePair.PartyRelationshipType.Id)
                                        && (item.SourceParty != null && item.SourceParty.Id == firstParty.Id)
                                         && (item.TargetParty != null && item.TargetParty.Id == secondParty.Id)).Where(item => item.EndDate > startDate).Count();
                //Check maximun cardinality
                if (partyTypePair.PartyRelationshipType.MaxCardinality != -1 && partyTypePair.PartyRelationshipType.MaxCardinality <= cnt)
                    BexisException.Throw(entity, string.Format("Maximum relations for this type of relation is {0}.", partyTypePair.PartyRelationshipType.MaxCardinality), BexisException.ExceptionType.Add);

                //Check if there is a relevant party type pair
                var alowedSource = partyTypePair.PartyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.SourcePartyType.Id == firstParty.PartyType.Id);
                var alowedTarget = partyTypePair.PartyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.TargetPartyType.Id == secondParty.PartyType.Id);
                if (alowedSource == null || alowedTarget == null)
                    BexisException.Throw(entity, "There is not relevant 'PartyTypePair' for these types of parties.", BexisException.ExceptionType.Add);
                partyTypePair.PartyRelationshipType.PartyRelationships.Add(entity);
                repoPR.Put(entity);
                uow.Commit();
            }

            if (entity != null) Update(entity.SourceParty);
            return (entity);

            //update the source party to check if relationship rules are satisfied and changed the istemp field
        }

        public PartyRelationship AddPartyRelationship(PartyX sourceParty, PartyX targetParty,
                                    string title, string description, PartyTypePair partyTypePair, DateTime? startDate = null, DateTime? endDate = null, string scope = "", int? permission = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(sourceParty != null);
            Contract.Requires(sourceParty.Id >= 0, "Provided first entity must have a permanent ID");
            Contract.Requires(targetParty != null);
            Contract.Requires(targetParty.Id >= 0, "Provided first entity must have a permanent ID");
            Contract.Requires(partyTypePair != null);
            Contract.Requires(partyTypePair.Id > 0);
            Contract.Ensures(Contract.Result<PartyRelationship>() != null && Contract.Result<PartyRelationship>().Id >= 0);

            if (startDate == null)
                startDate = DateTime.MinValue;
            if (endDate == null)
                endDate = DateTime.MaxValue;
            if (startDate > endDate)
                BexisException.Throw(sourceParty, "End date should be greater than start date.");

            PartyRelationship entity = null; ;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repoParty = uow.GetRepository<PartyX>();
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                IRepository<PartyRelationshipType> repoRelType = uow.GetRepository<PartyRelationshipType>();

                entity = new PartyRelationship()
                {
                    Description = description,
                    EndDate = endDate.Value,
                    SourceParty = sourceParty,
                    //PartyRelationshipType = partyTypePair.PartyRelationshipType,
                    Scope = scope,
                    TargetParty = targetParty,
                    StartDate = startDate.Value,
                    Title = title,
                    Permission = permission.HasValue ? permission.Value : partyTypePair.PermissionTemplate
                };

                if (partyTypePair != null)
                {
                    entity.PartyTypePair = partyTypePair;
                    entity.PartyRelationshipType = partyTypePair.PartyRelationshipType;
                }

                //if(!repoRelType.IsLoaded(partyRelationshipType))
                //    partyRelationshipType = repoRelType.Reload(partyRelationshipType);
                var cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyTypePair.PartyRelationshipType.Id)
                                    && (item.SourceParty != null && item.SourceParty.Id == sourceParty.Id)
                                     && (item.TargetParty != null && item.TargetParty.Id == targetParty.Id)).Where(item => item.EndDate > startDate).Count();
                //Check maximun cardinality
                if (partyTypePair.PartyRelationshipType.MaxCardinality != -1 && partyTypePair.PartyRelationshipType.MaxCardinality <= cnt)
                    BexisException.Throw(entity, string.Format("Maximum relations for this type of relation is {0}.", partyTypePair.PartyRelationshipType.MaxCardinality), BexisException.ExceptionType.Add);

                //Check if there is a relevant party type pair
                // var sourceType = partyTypePair.PartyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.SourcePartyType.Id == sourceParty.PartyType.Id || item.SourcePartyType.Id == targetParty.PartyType.Id);
                // var targetType = partyTypePair.PartyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.TargetPartyType.Id == sourceParty.PartyType.Id || item.TargetPartyType.Id == targetParty.PartyType.Id);
                var sourceType = partyTypePair.PartyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.SourcePartyType.Id == sourceParty.PartyType.Id);
                var targetType = partyTypePair.PartyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.TargetPartyType.Id == targetParty.PartyType.Id);
                if (sourceType == null || targetType == null)
                    BexisException.Throw(entity, "There is not relevant 'PartyTypePair' for these types of parties.", BexisException.ExceptionType.Add);
                partyTypePair.PartyRelationshipType.PartyRelationships.Add(entity);
                repoPR.Put(entity);
                uow.Commit();
            }

            //update the source party to check if relationship rules are satisfied and changed the istemp field
            if (entity != null) Update(entity.SourceParty);

            return (entity);
        }

        public Boolean UpdatePartyRelationship(long id, string title = null, string description = null, DateTime? startDate = null, DateTime? endDate = null, string scope = null, int? permission = null)
        {
            Contract.Requires(id >= 0, "a permanent ID is required.");
            if (startDate > endDate)
                BexisException.Throw(new PartyRelationship() { Id = id }, "End date should be greater than start date.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repo = uow.GetRepository<PartyRelationship>();
                var entity = repo.Get(id);
                if (title != null)
                    entity.Title = title;
                if (description != null)
                    entity.Description = description;
                if (startDate != null)
                    entity.StartDate = startDate.Value;
                if (endDate != null)
                    entity.EndDate = endDate.Value;
                if (scope != null)
                    entity.Scope = scope;
                if (permission != null)
                    entity.Permission = permission.Value;
                repo.Put(entity);
                uow.Commit();

                entity = repo.Reload(entity);
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="partyRelationship"></param>
        /// <param name="direction">0 = Source, 1 = target</param>
        /// <returns></returns>
        public bool RemovePartyRelationship(PartyRelationship partyRelationship, int direction = 0)
        {
            Contract.Requires(partyRelationship != null);
            Contract.Requires(partyRelationship.Id >= 0, "Provided entity must have a permanent ID");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationship> repoPR = uow.GetRepository<PartyRelationship>();
                partyRelationship = repoPR.Reload(partyRelationship);

                var cnt = 0;

                if (direction == 0)
                    cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationship.PartyRelationshipType.Id)
                                      && (item.SourceParty != null && item.SourceParty.Id == partyRelationship.SourceParty.Id)).Count();
                else if (direction == 1)
                    cnt = repoPR.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationship.PartyRelationshipType.Id)
                                      && (item.TargetParty != null && item.TargetParty.Id == partyRelationship.TargetParty.Id)).Count();

                if (partyRelationship.PartyRelationshipType.MinCardinality >= cnt)
                    BexisException.Throw(partyRelationship, String.Format("At least {0} party relation is required.", partyRelationship.PartyRelationshipType.MinCardinality), BexisException.ExceptionType.Delete);
                var entity = repoPR.Reload(partyRelationship);
                repoPR.Delete(entity);
                uow.Commit();
                //Here we don't need to change temp status of party because minimum cardinality will be presereved in any way.
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
                        BexisException.Throw(entity, String.Format("At least {0} party relation is required.", entity.PartyRelationshipType.MinCardinality), BexisException.ExceptionType.Delete, true);
                    var latest = repoPR.Reload(entity);
                    repoPR.Delete(latest);
                }
                uow.Commit();
                //Here we don't need to change temp status of party because minimum cardinality will be presereved in any way.
            }
            return (true);
        }

        #endregion PartyRelationship

        #region Associations

        /// <summary>
        /// add a single custom attribute value to a party object
        ///
        /// It's not checking uniqeness when it is not for a single custom attribute because it couldn't predict other values--> it should have all values to make a hash
        /// </summary>
        /// <param name="party"></param>
        /// <param name="partyCustomAttribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PartyCustomAttributeValue AddPartyCustomAttributeValue(PartyX party, PartyCustomAttribute partyCustomAttribute, string value)
        {
            // create a dictionary to pass along
            var dic = new Dictionary<PartyCustomAttribute, string>
            {
                { partyCustomAttribute, value }
            };

            // pass along
            var result = AddPartyCustomAttributeValues(party, dic);

            // find the corresponding attribute in the result
            return result.Where((item) => (item.CustomAttribute == partyCustomAttribute) && (item.Value == value)).FirstOrDefault();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="party"></param>
        /// <param name="partyCustomAttributeValues"></param>
        /// <returns></returns>
        public IEnumerable<PartyCustomAttributeValue> AddPartyCustomAttributeValues(PartyX party, Dictionary<PartyCustomAttribute, string> partyCustomAttributeValues)
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
                        party.CustomAttributeValues.Add(entity);
                    }
                    repoCAV.Put(entity);
                }
                uow.Commit();
            }
            party = UpdatePartyName(party);
            return party.CustomAttributeValues;
        }

        public IEnumerable<PartyCustomAttributeValue> AddPartyCustomAttributeValues(PartyX party, Dictionary<long, string> partyCustomAttributeValues)
        {
            return AddPartyCustomAttributeValues(party, ConvertDictionaryToPartyCustomeAttrValuesDictionary(partyCustomAttributeValues));
        }

        public PartyX GetParty(long id)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var partyRepo = uow.GetRepository<PartyX>();
                return partyRepo.Get(id);
            }
        }

        public PartyCustomAttributeValue UpdatePartyCustomAttributeValue(PartyCustomAttributeValue partyCustomAttributeValue, string newValue)
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
            entity.Party = UpdatePartyName(entity.Party);
            return (entity);
        }

        public PartyCustomAttributeValue UpdatePartyCustomAttributeValues(List<PartyCustomAttributeValue> partyCustomAttributeValues)
        {
            Contract.Requires(partyCustomAttributeValues != null && partyCustomAttributeValues.Any(), "Provided entities can not be null");
            Contract.Ensures(Contract.Result<PartyCustomAttributeValue>() != null && Contract.Result<PartyCustomAttributeValue>().Id >= 0, "No entity is persisted!");
            if (!CheckUniqueness(this.PartyRepository, partyCustomAttributeValues, partyCustomAttributeValues.First().Party))
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
            entity.Party = UpdatePartyName(entity.Party);
            return (entity);
        }

        public bool RemovePartyCustomAttributeValue(PartyCustomAttributeValue partyCustomAttributeValue)
        {
            Contract.Requires(partyCustomAttributeValue != null);
            Contract.Requires(partyCustomAttributeValue.Id >= 0, "Provided entity must have a permanent ID");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                if (partyCustomAttributeValue != null)
                {
                    var latest = repo.Get(partyCustomAttributeValue.Id);

                    if (latest != null)
                    {
                        latest.CustomAttribute = null;
                        latest.Party = null;
                        repo.Put(latest);
                        repo.Delete(latest);
                    }

                    uow.Commit();
                }
            }
            //partyCustomAttributeValue.Party = UpdatePartyName(partyCustomAttributeValue.Party);
            return (true);
        }

        public bool RemovePartyCustomAttributeValues(IEnumerable<PartyCustomAttributeValue> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyCustomAttributeValue e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyCustomAttributeValue e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttributeValue> repo = uow.GetRepository<PartyCustomAttributeValue>();
                foreach (var entity in entities)
                {
                    if (entity == null) continue;
                    var latest = repo.Get(entity.Id);

                    if (latest != null)
                    {
                        latest.CustomAttribute = null;
                        latest.Party = null;
                        repo.Put(latest);
                        repo.Delete(latest);
                    }
                }
                uow.Commit();
            }
            //if (entities.Any())
            //    UpdatePartyName(entities.First().Party);
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
        /// Filter parties by their custom attribute values and a related party type.
        /// </summary>
        /// <param name="partyType">Party type</param>
        /// <param name="customAttributeAndValues">key is CustomAttribute Name and the value is CustomAttribute Value</param>
        /// <returns></returns>
        public IEnumerable<PartyX> GetPartyByCustomAttributeValues(PartyType partyType, Dictionary<string, string> customAttributeAndValues)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repoParty = uow.GetRepository<PartyX>();
                IRepository<PartyCustomAttributeValue> repoPartyCustomAttributeValue = uow.GetRepository<PartyCustomAttributeValue>();
                var parties = repoParty.Get(cc => cc.PartyType.Id == partyType.Id);
                foreach (var customAttributeAndValue in customAttributeAndValues)
                {
                    parties = parties.Where(item => item.CustomAttributeValues.Where(cc => cc.Value == customAttributeAndValue.Value && cc.CustomAttribute.Name == customAttributeAndValue.Key).Any()).ToList();
                }
                return parties;
            }
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

        #endregion Associations

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

        public void RemovePartyUser(PartyX party, long userId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyUser> repo = uow.GetRepository<PartyUser>();
                var partyUser = repo.Get(cc => cc.PartyId == party.Id && cc.UserId == userId);
                if (partyUser != null)
                    repo.Delete(partyUser);
                uow.Commit();
            }
        }

        public PartyX GetPartyByUser(long userId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<PartyUser> repo = uow.GetReadOnlyRepository<PartyUser>();
                return repo.Query(c => c.UserId == userId)
                           .Select(c => c.Party)
                           .FirstOrDefault();
            }
        }

        public long GetUserIdByParty(long partyId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<PartyUser> repo = uow.GetReadOnlyRepository<PartyUser>();
                return repo.Query(c => c.PartyId == partyId)
                           .Select(c => c.UserId)
                           .FirstOrDefault();
            }
        }

        public Boolean UpdatePartyGridCustomColumns(IEnumerable<PartyCustomGridColumns> partyCustomGridColumns)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomGridColumns> repo = uow.GetRepository<PartyCustomGridColumns>();
                foreach (var partyCustomGridColumn in partyCustomGridColumns)
                {
                    var partyCustomGridColumnRep = repo.Get(partyCustomGridColumn.Id);
                    //TODO: Get current userId and fill the next line
                    //partyCustomGridColumnRep.UserId = partyCustomGridColumn.UserId;
                    partyCustomGridColumnRep.Enable = partyCustomGridColumn.Enable;
                    repo.Put(partyCustomGridColumnRep);
                }
                uow.Commit();
            }
            return true;
        }

        public Boolean UpdateOrAddPartyGridCustomColumn(PartyType partyType, PartyCustomAttribute partyCustomAttribute, PartyTypePair partyTypePair, bool enable = true, long? userId = null)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomGridColumns> repo = uow.GetRepository<PartyCustomGridColumns>();

                var partyCustomGridColumn = new PartyCustomGridColumns();

                //partyCustomGridColumn = repo.Query().Where(p =>
                //(p.CustomAttribute != null && partyCustomAttribute != null && p.CustomAttribute.Id.Equals(partyCustomAttribute.Id)) ||
                //(p.CustomAttribute == null && partyCustomAttribute == null)) &&
                //((p.TypePair != null && partyTypePair != null && p.TypePair.Id.Equals(partyTypePair.Id)) ||
                //(p.TypePair == null && partyTypePair == null))).FirstOrDefault();

                if (partyCustomAttribute != null || partyTypePair == null)
                {
                    partyCustomGridColumn = repo.Query().Where(p =>
                    p.TypePair == null &&
                    p.CustomAttribute != null &&
                    p.CustomAttribute.Id.Equals(partyCustomAttribute.Id)).FirstOrDefault();
                }
                else if (partyCustomAttribute == null || partyTypePair != null)
                {
                    partyCustomGridColumn = repo.Query().Where(p =>
                    p.CustomAttribute == null &&
                    p.TypePair != null &&
                    p.TypePair.Id.Equals(partyTypePair.Id)).FirstOrDefault();
                }

                if (partyCustomGridColumn == null) partyCustomGridColumn = new PartyCustomGridColumns();

                partyCustomGridColumn.UserId = userId;
                partyCustomGridColumn.Enable = enable;
                partyCustomGridColumn.CustomAttribute = partyCustomAttribute;
                partyCustomGridColumn.TypePair = partyTypePair;
                repo.Put(partyCustomGridColumn);
                uow.Commit();
            }
            return true;
        }

        public Boolean RemovePartyGridCustomColumn(long id)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomGridColumns> repo = uow.GetRepository<PartyCustomGridColumns>();
                var pcgc = repo.Get(id);

                if (pcgc != null)
                {
                    repo.Delete(pcgc);
                    uow.Commit();
                    return true;
                }
            }

            return false;
        }

        #endregion Account

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

        public Dictionary<PartyRelationshipType, int> ValidateRelationships(long partyId)
        {
            using (var partyManager = new PartyManager())
            using (var partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                var party = partyManager.PartyRepository.Get(partyId);
                var requiredPartyRelationTypes = partyRelationshipTypeManager.GetAllPartyRelationshipTypes(party.PartyType.Id).Where(cc => cc.MinCardinality > 0);
                var partyRelations = partyManager.PartyRelationshipRepository.Get(cc => cc.SourceParty.Id == party.Id);
                var requiredPartyRelationTypeCount = new Dictionary<PartyRelationshipType, int>();
                foreach (var requiredPartyRelationType in requiredPartyRelationTypes)
                {
                    //if all the type pair have condition and current party doesn't have any of this conditions, this relation type will be skipped
                    if (requiredPartyRelationType.AssociatedPairs.Count(cc => !String.IsNullOrEmpty(cc.ConditionSource)) == requiredPartyRelationType.AssociatedPairs.Count)
                    {
                        bool passConditions = false;
                        foreach (var partyTypePair in requiredPartyRelationType.AssociatedPairs)
                            if (CheckCondition(partyTypePair.ConditionSource, partyId))
                                passConditions = true;
                        if (!passConditions)
                            continue;
                    }
                    if (partyRelations.Where(cc => cc.PartyRelationshipType.Id == requiredPartyRelationType.Id).Count() < requiredPartyRelationType.MinCardinality)
                        requiredPartyRelationTypeCount.Add(requiredPartyRelationType, requiredPartyRelationType.MinCardinality - partyRelations.Where(cc => cc.PartyRelationshipType.Id == requiredPartyRelationType.Id).Count());
                }
                return requiredPartyRelationTypeCount;
            }
        }

        public bool CheckCondition(String condition, long partyId)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            var party = PartyRepository.Get(partyId);
            if (party == null)
                return false;
            var newCondition = condition;
            //if text is sourounded with [] means the value comes from an element
            //Extract such text and replace them by the value of the related element

            MatchCollection matches = Regex.Matches(condition, @"\[(.*?)\]");
            foreach (Match match in matches)
            {
                var customAttributeName = match.Groups[1].Value;
                var customAttributeValue = party.CustomAttributeValues.FirstOrDefault(cc => cc.CustomAttribute.Name == customAttributeName);
                if (customAttributeValue == null)
                    throw new Exception(string.Format("There is not any custom attribute name which has {0} name.", customAttributeName));
                newCondition = newCondition.Replace(match.Groups[0].Value, string.Format("'{0}'", customAttributeValue.Value));
            }
            try
            {
                Expression e = new Expression(newCondition);
                return ((bool)e.Evaluate());
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        /// <summary>
        /// Conver a simple string,string dictionary to PartyCustomAttribute, string
        /// </summary>
        /// <param name="partyCustomAttributes"></param>
        /// <returns></returns>
        private Dictionary<PartyCustomAttribute, string> ConvertDictionaryToPartyCustomeAttrValuesDictionary(Dictionary<long, string> partyCustomAttributes)
        {
            using (var partyTypeManager = new PartyTypeManager())
            {
                var result = new Dictionary<PartyCustomAttribute, string>();
                foreach (var partyCustomAttribute in partyCustomAttributes)
                {
                    var customAttribute = partyTypeManager.PartyCustomAttributeRepository.Get(partyCustomAttribute.Key);
                    if (customAttribute != null && customAttribute.Id != 0)
                        result.Add(customAttribute, partyCustomAttribute.Value);
                    else
                        throw new Exception("Error in custom attribute values.");
                }
                return result;
            }
        }

        /// <summary>
        /// Conver a simple string,string dictionary to PartyCustomAttribute, string
        /// </summary>
        /// <param name="partyCustomAttributes"></param>
        /// <returns></returns>
        private Dictionary<PartyCustomAttribute, string> ConvertDictionaryToPartyCustomeAttrValuesDictionary(Dictionary<string, string> partyCustomAttributes, PartyType partyType)
        {
            using (var partyTypeManager = new PartyTypeManager())
            {
                var result = new Dictionary<PartyCustomAttribute, string>();
                foreach (var partyCustomAttribute in partyCustomAttributes)
                {
                    var customAttribiute = partyTypeManager.PartyCustomAttributeRepository.Get(cc => cc.Name == partyCustomAttribute.Key && cc.PartyType == partyType).FirstOrDefault();
                    if (customAttribiute == null)
                        BexisException.Throw(customAttribiute, "There is no custom attribute with name of " + partyCustomAttribute.Key + " for this party type!");
                    result.Add(customAttribiute, partyCustomAttribute.Value);
                }
                return result;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="partyTypeId"></param>
        /// <param name="all">regardless of enable</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<PartyCustomGridColumns> GetPartyCustomGridColumns(long partyTypeId, bool all = false, long? userId = null)
        {
            //retrieve all the records for this partyId
            var partyCustomGridColumns = PartyCustomGridColumnsRepository.Get(cc => (cc.CustomAttribute.PartyType.Id == partyTypeId || cc.TypePair.SourcePartyType.Id == partyTypeId)
            && (cc.UserId == userId));
            //&& ((userId.HasValue == true && cc.UserId.Value == userId.Value) || !userId.HasValue));

            if (!all)
                partyCustomGridColumns = partyCustomGridColumns.Where(cc => cc.Enable).ToList();
            return partyCustomGridColumns;
        }

        private PartyX UpdatePartyName(PartyX party)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyX> repo = uow.GetRepository<PartyX>();
                //party = repo.Reload(party);
                var mainValues = party.CustomAttributeValues.Where(item => item.CustomAttribute.IsMain).Select(item => item.Value).ToArray();
                if (mainValues.Length > 0)
                {
                    party.Name = string.Join(" ", mainValues); // what should happen when no custom attribute is there!?
                    repo.Merge(party);
                    var merged = repo.Get(party.Id);
                    repo.Put(merged);
                    uow.Commit();
                }
                return party;
            }
        }

        #endregion privateMethod
    }
}