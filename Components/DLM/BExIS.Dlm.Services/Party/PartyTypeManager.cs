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
    public class PartyTypeManager
    {
        public IReadOnlyRepository<PartyType> Repo { get; private set; }
        public PartyTypeManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<PartyType>();
        }


        #region PartyType

        public PartyType Create(string title, string description, List<PartyStatusType> statusTypes=null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(statusTypes != null && statusTypes.Count() > 0); // there should be at least one status defined for each party type
           // Contract.Ensures(Contract.Result<PartyType>() != null && Contract.Result<PartyType>().Id >= 0);

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

        public bool Delete(PartyType partyType)
        {
            Contract.Requires(partyType != null);
            Contract.Requires(partyType.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repo = uow.GetRepository<PartyType>();
                IRepository<PartyStatusType> repoPST = uow.GetRepository<PartyStatusType>();
                IRepository<PartyCustomAttribute> repoCA = uow.GetRepository<PartyCustomAttribute>();
                partyType = repo.Reload(partyType);
                // check whether the party type is used i party, status types are used in some statuses, and custom attributes too. if yes, the party type can ot bedeleted
                if (
                    (partyType.Parties!=null && partyType.Parties.Count()>0 )
                    || (partyType.StatusTypes!=null && partyType.StatusTypes.Count(p => (p.Statuses!=null && p.Statuses.Count()>0))>0)
                    || partyType.CustomAttributes!=null && partyType.CustomAttributes.Count(p => p.CustomAttributeValues.Count()>0)>0
                    )
                {
                    throw new Exception("Delete fail. There are some data in another tables with this data type.");
                }

              
                // delete the history
                repoPST.Delete(partyType.StatusTypes);
                repoCA.Delete(partyType.CustomAttributes);

                // remove all associations between the entity and its history items
                // the status types must also be deleted, not just the association between them and the party type.
                partyType.StatusTypes.ToList().ForEach(a => a.PartyType = null);
                partyType.StatusTypes.Clear();
                // the custmom attributes must be also deleted.
                partyType.CustomAttributes.ToList().ForEach(a => a.PartyType = null);
                partyType.CustomAttributes.Clear();

                //delete the entity
                repo.Delete(partyType);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<PartyType> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyType e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyType e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repo = uow.GetRepository<PartyType>();
                IRepository<PartyStatusType> repoPST = uow.GetRepository<PartyStatusType>();
                IRepository<PartyCustomAttribute> repoCA = uow.GetRepository<PartyCustomAttribute>();
                foreach (var entity in entities)
                {
                    var partyType = repo.Reload(entity);
                    // check whether the party type is used i party, status types are used in some statuses, and custom attributes too. if yes, the party type can ot bedeleted
                    if (
                        (partyType.Parties != null && partyType.Parties.Count()>0)
                        || (partyType.StatusTypes != null && partyType.StatusTypes.Count(p => (p.Statuses != null && p.Statuses.Count()>0))>0)
                        || partyType.CustomAttributes != null && partyType.CustomAttributes.Count(p => p.CustomAttributeValues.Count()>0)>0
                        )
                    {
                        throw new Exception("Delete fail. There are some data in another tables with this data type.");
                    }


                    // delete the history
                    repoPST.Delete(partyType.StatusTypes);
                    repoCA.Delete(partyType.CustomAttributes);

                    // remove all associations between the entity and its history items
                    // the status types must also be deleted, not just the association between them and the party type.
                    partyType.StatusTypes.ToList().ForEach(a => a.PartyType = null);
                    partyType.StatusTypes.Clear();
                    // the custmom attributes must be also deleted.
                    partyType.CustomAttributes.ToList().ForEach(a => a.PartyType = null);
                    partyType.CustomAttributes.Clear();

                    //delete the entity
                    repo.Delete(partyType);
                }
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        #endregion

        #region PartyCustomAttribute
        public PartyCustomAttribute CreatePartyCustomAttribute(PartyType partyType,string dataType, string name, string description, int displayOrder)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(description));
            Contract.Requires(partyType != null);
            var entity = new PartyCustomAttribute()
            {
                DataType = dataType,
                Description = description,
                DisplayOrder = displayOrder,
                PartyType= partyType,
                Name = name
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttribute> repo = uow.GetRepository<PartyCustomAttribute>();              
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public bool DeletePartyCustomAttribute(PartyCustomAttribute entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttribute> repo = uow.GetRepository<PartyCustomAttribute>();
                IRepository<PartyCustomAttributeValue> repoCAV = uow.GetRepository<PartyCustomAttributeValue>();
                entity = repo.Reload(entity);
                // remove all associations between the entity and its history items
                entity.CustomAttributeValues.Clear();
                repoCAV.Delete(entity.CustomAttributeValues);
                //delete the entity
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeletePartyCustomAttribute(IEnumerable<PartyCustomAttribute> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyCustomAttribute e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyCustomAttribute e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttribute> repo = uow.GetRepository<PartyCustomAttribute>();
                IRepository<PartyCustomAttributeValue> repoCAV = uow.GetRepository<PartyCustomAttributeValue>();
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    // remove all associations between the entity and its history items
                    latest.CustomAttributeValues.Clear();
                    repoCAV.Delete(latest.CustomAttributeValues);
                    //delete the entity
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        #endregion

        #region Associations
        public PartyStatusType AddStatusType(PartyType partyType, string name, string description, int displayOrder)
        {
            // reorder the other status types that confclict with the displayOrder passed here
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            
            var entity = new PartyStatusType()
            {
                Description = description,
                DisplayOrder = displayOrder,
                Name = name,
                PartyType = partyType

            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatusType> repo = uow.GetRepository<PartyStatusType>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);

        }

        public void RemoveStatusType( PartyStatusType statusType)
        {
            // there should remain at least 1 status type attached to the party type after removal of the passed one

            return;
        }

        public void RemoveStatusType(List<PartyStatusType> statusType)
        {
            // there should remain at least 1 status type attached to the party type after removal of the passed one

            return;
        }
        #endregion
    }
}
