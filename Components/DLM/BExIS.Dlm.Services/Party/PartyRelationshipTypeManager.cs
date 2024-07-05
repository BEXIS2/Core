using BExIS.Dlm.Entities.Party;
using BExIS.Ext.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Party
{
    public class PartyRelationshipTypeManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public PartyRelationshipTypeManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            PartyRelationshipTypeRepository = _guow.GetReadOnlyRepository<PartyRelationshipType>();
            PartyTypePairRepository = _guow.GetReadOnlyRepository<PartyTypePair>();
        }

        ~PartyRelationshipTypeManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<PartyTypePair> PartyTypePairRepository { get; }
        public IReadOnlyRepository<PartyRelationshipType> PartyRelationshipTypeRepository { get; }
        public IQueryable<PartyRelationshipType> PartyRelationshipTypes => PartyRelationshipTypeRepository.Query();

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

        #region PartyRelationshipType

        /// <summary>
        /// Creating PartyRelationshipType
        /// because PartyRelationshipType should have PartyTypePairs,partyTypePair created in the same time of creating PartyRelationshipType
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="indicatesHierarchy"></param>
        /// <param name="maxCardinality"></param>
        /// <param name="minCardinality"></param>
        /// <param name="partyTypePairAlowedSource"></param>
        /// <param name="partyTypePairAlowedTarget"></param>
        /// <param name="partyTypePairTitle"></param>
        /// <param name="partyTypePairDescription"></param>
        /// <returns></returns>
        public PartyRelationshipType Create(string title, string displayName, string description, bool indicatesHierarchy, int maxCardinality,
            int minCardinality, bool partyRelationShipTypeDefault, PartyType partyTypePairAlowedSource, PartyType partyTypePairAlowedTarget,
            string partyTypePairTitle, string partyTypePairDescription, string conditionSource, string conditionTarget, int permissionTemplate)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(partyTypePairTitle));
            Contract.Requires(partyTypePairAlowedSource != null && partyTypePairAlowedSource.Id > 0);
            Contract.Requires(partyTypePairAlowedTarget != null && partyTypePairAlowedTarget.Id > 0);
            Contract.Ensures((Contract.Result<PartyRelationshipType>() != null && Contract.Result<PartyRelationshipType>().Id >= 0));
            //Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);

            PartyRelationshipType entity = new PartyRelationshipType()
            {
                Description = description,
                IndicatesHierarchy = indicatesHierarchy,
                MaxCardinality = maxCardinality,
                MinCardinality = minCardinality,
                Title = title,
                DisplayName = displayName
            };
            var partyTypeEntity = new PartyTypePair()
            {
                SourcePartyType = partyTypePairAlowedSource,
                TargetPartyType = partyTypePairAlowedTarget,
                Description = partyTypePairDescription,
                PartyRelationshipType = entity,
                Title = partyTypePairTitle,
                PartyRelationShipTypeDefault = partyRelationShipTypeDefault,
                ConditionSource = conditionSource,
                PermissionTemplate = permissionTemplate,
                ConditionTarget = conditionTarget
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repo = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyTypePair> repoPTP = uow.GetRepository<PartyTypePair>();
                repo.Put(entity);
                repoPTP.Put(partyTypeEntity);
                uow.Commit();
            }
            return (entity);
        }

        public PartyRelationshipType Update(long id, string title, string displayName, string description, bool indicatesHierarchy, int maxCardinality,
           int minCardinality)
        {
            Contract.Requires(id > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Ensures((Contract.Result<PartyRelationshipType>() != null && Contract.Result<PartyRelationshipType>().Id >= 0));
            //var entity = new PartyRelationshipType();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repo = uow.GetRepository<PartyRelationshipType>();
                var entity = repo.Get(id);
                if (entity == null)
                    BexisException.Throw(null, "PartyRelationshipType not found", BexisException.ExceptionType.Edit);
                entity.Title = title;
                entity.DisplayName = displayName;
                entity.Description = description;
                entity.IndicatesHierarchy = indicatesHierarchy;
                entity.MaxCardinality = maxCardinality;
                entity.MinCardinality = minCardinality;
                repo.Put(entity);
                uow.Commit();
                return entity;
            }
        }

        public bool Delete(PartyRelationshipType partyRelationType)
        {
            Contract.Requires(partyRelationType != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repoPR = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyType> repoType = uow.GetRepository<PartyType>();
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();
                var entity = repoPR.Reload(partyRelationType);
                //If there is a relation between entity and a party we couldn't delete it
                if (entity.PartyRelationships.Count() > 0)
                    BexisException.Throw(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", BexisException.ExceptionType.Delete);

                // remove all associations between the entity and AssociatedPairs
                entity.AssociatedPairs.ToList().ForEach(item => repoPartyTypePair.Delete(item));
                entity.AssociatedPairs.Clear();

                repoPR.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool Delete(IEnumerable<PartyRelationshipType> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyRelationshipType e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyRelationshipType e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repoPR = uow.GetRepository<PartyRelationshipType>();
                repoPR.Evict();

                IRepository<PartyType> repoType = uow.GetRepository<PartyType>();
                foreach (var entity in entities)
                {
                    var latest = repoPR.Reload(entity);
                    //If there is a relation between entity and a party we couldn't delete it
                    if (latest.PartyRelationships.Count() > 0)
                        BexisException.Throw(latest, "There are some relations between this 'PartyRelationshipType' and 'Party'", BexisException.ExceptionType.Delete, true);
                    // remove all associations between the entity and AssociatedPairs
                    latest.AssociatedPairs.ToList().ForEach(item => item.PartyRelationshipType = null);
                    latest.AssociatedPairs.Clear();
                    repoPR.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        /// <summary>
        /// return all relationship type which has at least one source party type in their party types pair
        /// </summary>
        /// <param name="sourcePartyTypeId"></param>
        /// <returns></returns>
        public IEnumerable<PartyRelationshipType> GetAllPartyRelationshipTypes(long sourcePartyTypeId, Boolean targetToSource = false)
        {
            Contract.Requires(sourcePartyTypeId > 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repoPT = uow.GetRepository<PartyType>();
                IRepository<PartyRelationshipType> repoPRT = uow.GetRepository<PartyRelationshipType>();
                PartyType sourcePartyType = repoPT.Get(sourcePartyTypeId);
                var partyRelationshipTypes = repoPRT.Get(cc => cc.AssociatedPairs.Any(item => (!item.SourcePartyType.SystemType && !item.TargetPartyType.SystemType) && item.SourcePartyType.Id == sourcePartyTypeId || (targetToSource && item.TargetPartyType.Id == sourcePartyTypeId))).OrderBy(item => item.Title);
                return partyRelationshipTypes;
            }
        }

        #endregion PartyRelationshipType

        #region PartyTypePair

        public PartyTypePair AddPartyTypePair(string title, PartyType allowedSource, PartyType allowedTarget, string description, bool partyRelationShipTypeDefault,
            PartyRelationshipType partyRelationshipType, string conditionSource, string conditionTarget, int permissionsTemplate)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(allowedSource != null && allowedSource.Id > 0);
            Contract.Requires(allowedTarget != null && allowedTarget.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);

            var entity = new PartyTypePair()
            {
                SourcePartyType = allowedSource,
                TargetPartyType = allowedTarget,
                Description = description,
                PartyRelationshipType = partyRelationshipType,
                Title = title,
                PartyRelationShipTypeDefault = partyRelationShipTypeDefault,
                ConditionSource = conditionSource,
                ConditionTarget = conditionTarget,
                PermissionTemplate = permissionsTemplate
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public PartyTypePair UpdatePartyTypePair(long id, string title, PartyType allowedSource, PartyType alowedTarget, string description,
            string sourceCondition, string targetCondition, bool partyRelationShipTypeDefault,
            PartyRelationshipType partyRelationshipType, int permissionTemplate)
        {
            Contract.Requires(id > 0);
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(allowedSource != null && allowedSource.Id > 0);
            Contract.Requires(alowedTarget != null && alowedTarget.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);
            var entity = new PartyTypePair();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                entity = repo.Get(id);
                if (entity == null)
                    BexisException.Throw(null, "PartyTypePair not found", BexisException.ExceptionType.Edit);
                entity.SourcePartyType = allowedSource;
                entity.TargetPartyType = alowedTarget;
                entity.Description = description;
                entity.PartyRelationshipType = partyRelationshipType;
                entity.Title = title;
                entity.PartyRelationShipTypeDefault = partyRelationShipTypeDefault;
                entity.PermissionTemplate = permissionTemplate;
                entity.ConditionSource = sourceCondition;
                entity.ConditionTarget = targetCondition;
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public void UpdatePartyTypePair(PartyTypePair partyTypePair)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<PartyTypePair>();
                repo.Merge(partyTypePair);
                var merged = repo.Get(partyTypePair.Id);
                repo.Put(merged);
                uow.Commit();
            }
        }

        public bool RemovePartyTypePair(PartyTypePair partyTypePair)
        {
            Contract.Requires(partyTypePair != null && partyTypePair.Id > 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPR = uow.GetRepository<PartyTypePair>();
                IRepository<PartyRelationshipType> repoRel = uow.GetRepository<PartyRelationshipType>();
                var entity = repoPR.Reload(partyTypePair);
                if (entity.PartyRelationshipType.PartyRelationships.Count() > 0)
                    //if (repoRel.Get(item => item.AssociatedPairs.Contains(partyTypePair)).Count() > 0)
                    BexisException.Throw(entity, "There are some relations between this entity and 'PartyRelationshipType'.", BexisException.ExceptionType.Delete);

                repoPR.Delete(entity);
                uow.Commit();
            }
            return (true);
        }

        public bool RemovePartyTypePair(IEnumerable<PartyTypePair> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyTypePair e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyTypePair e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPR = uow.GetRepository<PartyTypePair>();
                IRepository<PartyRelationshipType> repoRel = uow.GetRepository<PartyRelationshipType>();
                foreach (var entity in entities)
                {
                    if (repoRel.Get(item => item.AssociatedPairs.Contains(entity)).Count() > 0)
                        BexisException.Throw(entity, "There are some relations between this entity and 'PartyRelationshipType'.", BexisException.ExceptionType.Delete, true);

                    var latest = repoPR.Reload(entity);
                    repoPR.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        #endregion PartyTypePair

        #region additional_methods

        public IEnumerable<PartyTypePair> GetPartyTypePairs(PartyRelationshipType partyRelationshipType, PartyType sourcePartyType, PartyType targetPartyType)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();
                var partyTypePairs = repoPartyTypePair.Get(cc => cc.PartyRelationshipType.Id == partyRelationshipType.Id && cc.SourcePartyType.Id == sourcePartyType.Id && cc.TargetPartyType.Id == targetPartyType.Id);
                return partyTypePairs;
            }
        }

        public IEnumerable<PartyType> GetRootPartyTypes()
        {
            var partyTypes = new List<PartyType>();
            var rootPartiesDic = new Dictionary<String, List<String>>();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();
                var inheritencePartyTypepairs = repoPartyTypePair.Get(item => item.PartyRelationshipType.IndicatesHierarchy).ToList();
                var inheritenceTargetParties = inheritencePartyTypepairs.Select(cc => cc.TargetPartyType).ToList();
                return inheritencePartyTypepairs.Where(cc => !inheritenceTargetParties.Contains(cc.SourcePartyType)).Select(cc => cc.SourcePartyType);
            }
        }

        public void GetChildrenPartyTypes(int rootPartyTypeId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("metadataStructureId", rootPartyTypeId);
        }

        public Dictionary<String, List<String>> GetRootPartyTypesAndChildren()
        {
            var partyTypes = new List<PartyType>();
            var rootPartiesDic = new Dictionary<String, List<String>>();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();
                var inheritencePartyTypepairs = repoPartyTypePair.Get(item => item.PartyRelationshipType.IndicatesHierarchy).ToList();
                var inheritenceTargetParties = inheritencePartyTypepairs.Select(cc => cc.TargetPartyType).ToList();
                var parentPartyTypePairs = inheritencePartyTypepairs.Where(cc => !inheritenceTargetParties.Contains(cc.SourcePartyType));

                foreach (PartyTypePair partyTypePair in parentPartyTypePairs)
                {
                    List<string> res = new List<string>();
                    res.Add(partyTypePair.TargetPartyType.DisplayName);
                    res.AddRange(getAllchildrens(partyTypePair.TargetPartyType, inheritencePartyTypepairs));
                    if (rootPartiesDic.ContainsKey(partyTypePair.SourcePartyType.DisplayName))
                    {
                        var combinedChild = rootPartiesDic[partyTypePair.SourcePartyType.DisplayName];
                        combinedChild.AddRange(res);
                        rootPartiesDic[partyTypePair.SourcePartyType.DisplayName] = combinedChild.Distinct().ToList();
                    }
                    else
                        rootPartiesDic.Add(partyTypePair.SourcePartyType.DisplayName, res.Distinct().ToList());
                }
            }
            return rootPartiesDic;
        }

        private string[] getAllchildrens(PartyType partyType, List<PartyTypePair> partyTypePairs)
        {
            List<string> res = new List<string>();
            foreach (var ptp in partyTypePairs)
            {
                if (ptp.SourcePartyType == partyType)
                {
                    res.Add(ptp.TargetPartyType.Title);
                    res.AddRange(getAllchildrens(ptp.TargetPartyType, partyTypePairs));
                }
            }
            return res.ToArray();
        }

        /// <summary>
        /// there is an inheritance relationship between the party types which comes from the party relationship type
        /// if the indicate inheritance is true it means it is a root party types and all the source parties
        /// </summary>
        /// <param name="targetPartyTypeId"></param>
        /// <returns></returns>
        public IEnumerable<PartyType> GetChildPartyTypes(int targetPartyTypeId)
        {
            var partyTypes = new List<PartyType>();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();

                //Find all the typePair which are IndicatesHierarchy and their target is equal to the input
                var partyTypePairs = repoPartyTypePair.Get(item => item.TargetPartyType.Id == targetPartyTypeId && item.PartyRelationshipType.IndicatesHierarchy);
                //Add all of their parties
                partyTypes.AddRange(partyTypePairs.Select(item => item.SourcePartyType).Distinct());
            }
            return partyTypes;
        }

        #endregion additional_methods
    }
}