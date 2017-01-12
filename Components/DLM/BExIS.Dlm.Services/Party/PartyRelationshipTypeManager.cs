using BExIS.Dlm.Entities.Party;
using BExIS.Ext.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Party
{
    public class PartyRelationshipTypeManager
    {
        public IReadOnlyRepository<PartyRelationshipType> Repo { get; private set; }
        public IReadOnlyRepository<PartyTypePair> RepoPartyTypePair { get; private set; }
        public PartyRelationshipTypeManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            Repo = uow.GetReadOnlyRepository<PartyRelationshipType>();
            RepoPartyTypePair = uow.GetReadOnlyRepository<PartyTypePair>();
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
        public PartyRelationshipType Create(string title, string description, bool indicatesHierarchy, int maxCardinality,
            int minCardinality, PartyType partyTypePairAlowedSource, PartyType partyTypePairAlowedTarget,
            string partyTypePairTitle, string partyTypePairDescription)
        {

            Contract.Requires(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(partyTypePairTitle));
            Contract.Requires(partyTypePairAlowedSource != null && partyTypePairAlowedSource.Id > 0);
            Contract.Requires(partyTypePairAlowedTarget != null && partyTypePairAlowedTarget.Id > 0);
            Contract.Ensures((Contract.Result<PartyRelationshipType>() != null && Contract.Result<PartyRelationshipType>().Id >= 0)
                && (Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0));

            PartyRelationshipType entity = new PartyRelationshipType()
            {
                Description = description,
                IndicatesHierarchy = indicatesHierarchy,
                MaxCardinality = maxCardinality,
                MinCardinality = minCardinality,
                Title = title
            };
            var partyTypeEntity = new PartyTypePair()
            {
                AlowedSource = partyTypePairAlowedSource,
                AlowedTarget = partyTypePairAlowedTarget,
                Description = partyTypePairDescription,
                PartyRelationshipType = entity,
                Title = partyTypePairTitle
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
        public PartyRelationshipType Update(long id, string title, string description, bool indicatesHierarchy, int maxCardinality,
           int minCardinality)
        {
            Contract.Requires(id > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Ensures((Contract.Result<PartyRelationshipType>() != null && Contract.Result<PartyRelationshipType>().Id >= 0));
            var entity = new PartyRelationshipType();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repo = uow.GetRepository<PartyRelationshipType>();
                entity = repo.Get(id);
                if (entity == null)
                    BexisException.Throw(null, "PartyRelationshipType not found", BexisException.ExceptionType.Edit);
                entity.Title = title;
                entity.Description = description;
                entity.IndicatesHierarchy = indicatesHierarchy;
                entity.MaxCardinality = maxCardinality;
                entity.MinCardinality = minCardinality;
                repo.Put(entity);
                uow.Commit();
            }
            return entity;            
        }

        public bool Delete(PartyRelationshipType partyRelationType)
        {
            Contract.Requires(partyRelationType != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repoPR = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyType> repoType = uow.GetRepository<PartyType>();

                var entity = repoPR.Reload(partyRelationType);
                //If there is a relation between entity and a party we couldn't delete it
                if (entity.PartyRelationships.Count() > 0)
                    BexisException.Throw(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", BexisException.ExceptionType.Delete);
                // remove all associations between the entity and AssociatedPairs
                entity.AssociatedPairs.ToList().ForEach(item => item.PartyRelationshipType = null);
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
                IRepository<PartyType> repoType = uow.GetRepository<PartyType>();
                foreach (var entity in entities)
                {
                    var latest = repoPR.Reload(entity);
                    //If there is a relation between entity and a party we couldn't delete it
                    if (entity.PartyRelationships.Count() > 0)
                        BexisException.Throw(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", BexisException.ExceptionType.Delete,true);
                    // remove all associations between the entity and AssociatedPairs
                    entity.AssociatedPairs.ToList().ForEach(item => item.PartyRelationshipType = null);
                    entity.AssociatedPairs.Clear();
                    repoPR.Delete(entity);
                }
                uow.Commit();
            }
            return (true);
        }
        #endregion
        #region PartyTypePair
        public PartyTypePair AddPartyTypePair(string title, PartyType alowedSource, PartyType alowedTarget, string description,
            PartyRelationshipType partyRelationshipType)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(alowedSource != null && alowedSource.Id > 0);
            Contract.Requires(alowedTarget != null && alowedTarget.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);

            var entity = new PartyTypePair()
            {
                AlowedSource = alowedSource,
                AlowedTarget = alowedTarget,
                Description = description,
                PartyRelationshipType = partyRelationshipType,
                Title = title
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        public PartyTypePair UpdatePartyTypePair(long id, string title, PartyType alowedSource, PartyType alowedTarget, string description,
            PartyRelationshipType partyRelationshipType)
        {
            Contract.Requires(id >0);
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(alowedSource != null && alowedSource.Id > 0);
            Contract.Requires(alowedTarget != null && alowedTarget.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);
            var entity = new PartyTypePair();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                entity = repo.Get(id);
                if (entity == null)
                    BexisException.Throw(null, "PartyTypePair not found", BexisException.ExceptionType.Edit);
                entity.AlowedSource = alowedSource;
                entity.AlowedTarget = alowedTarget;
                entity.Description = description;
                entity.PartyRelationshipType = partyRelationshipType;
                entity.Title = title;               
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        public bool RemovePartyTypePair(PartyTypePair partyTypePair)
        {
            Contract.Requires(partyTypePair != null && partyTypePair.Id > 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPR = uow.GetRepository<PartyTypePair>();
                IRepository<PartyRelationshipType> repoRel = uow.GetRepository<PartyRelationshipType>();
                var entity = repoPR.Reload(partyTypePair);
                if (repoRel.Get(item => item.AssociatedPairs.Contains(partyTypePair)).Count() > 0)
                    BexisException.Throw(entity,"There are some relations between this entity and 'PartyRelationshipType'.",BexisException.ExceptionType.Delete);
               
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
                        BexisException.Throw(entity, "There are some relations between this entity and 'PartyRelationshipType'.", BexisException.ExceptionType.Delete,true);

                    var latest = repoPR.Reload(entity);
                    repoPR.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }
        #endregion

    }
}
