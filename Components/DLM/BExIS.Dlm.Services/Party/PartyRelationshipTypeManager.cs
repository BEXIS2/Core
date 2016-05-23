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
        public PartyRelationshipType Create(string title,string description, bool indicatesHierarchy,  int maxCardinality,
            int minCardinality, PartyType partyTypePairAlowedSource, PartyType partyTypePairAlowedTarget, 
            string partyTypePairTitle, string partyTypePairDescription, ICollection<PartyRelationship> partyRelationships)
        {

            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            PartyRelationshipType entity = new PartyRelationshipType()
            {                
                 PartyRelationships=partyRelationships,
                 Description= description,
                IndicatesHierarchy= indicatesHierarchy,
                MaxCardinality= maxCardinality,
                MinCardinality= minCardinality,                
                Title= title
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repo = uow.GetRepository<PartyRelationshipType>();
                repo.Put(entity);
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
                if (entity.PartyRelationships.Count()>0)
                    return true;
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
                    if (entity.PartyRelationships.Count()>0)
                        throw new Exception("Delete fail. There is a relation between a party and PartyRelationshipType");
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
        public PartyTypePair AddPartyTypePair(PartyType alowedSource, PartyType alowedTarget,string description,
            PartyRelationshipType partyRelationshipType,string title)
        {
            var entity = new PartyTypePair() {
                AlowedSource= alowedSource,
                AlowedTarget= alowedTarget,
                Description=description,
                PartyRelationshipType= partyRelationshipType,
                Title=title
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        
        public bool RemovePartyTypePair(PartyTypePair partyTypePair)
        {
            Contract.Requires(partyTypePair != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPR = uow.GetRepository<PartyTypePair>();
                var entity = repoPR.Reload(partyTypePair);
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

    }
}
