using BExIS.Dlm.Entities.Party;
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
        public PartyRelationshipTypeManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<PartyRelationshipType>();
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
                    PartyManager.ThrowException(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", PartyManager.ExceptionType.Delete);
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
                        PartyManager.ThrowException(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", PartyManager.ExceptionType.Delete,true);
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

                //Is it usefull?
                //var similarPartTypePair = repo.Get(item => item.AlowedSource == alowedSource && item.AlowedTarget == alowedTarget);
                //if (similarPartTypePair.Any())
                //    throw new Exception("Add party type pair failed.\r\nThere is already an entity with the same elements.");
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        public PartyTypePair UpdatePartyTypePair(PartyTypePair entity)
        {
            Contract.Requires(entity != null && entity.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                //Is it usefull?
                //var similarPartTypePair = repo.Get(item => item.Id!=entity.Id && (item.AlowedSource == entity.AlowedSource && item.AlowedTarget == entity.AlowedTarget));
                //if (similarPartTypePair.Any())
                //    throw new Exception("Update party type pair failed.\r\nThere is already an entity with the same elements.");
                repo.Put(entity); // Merge is required here!!!!
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
                    PartyManager.ThrowException(entity,"There are some relations between this entity and 'PartyRelationshipType'.",PartyManager.ExceptionType.Delete);
               
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
                        PartyManager.ThrowException(entity, "There are some relations between this entity and 'PartyRelationshipType'.", PartyManager.ExceptionType.Delete,true);

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
