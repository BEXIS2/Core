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
    public class PartyTypeManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public PartyTypeManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            PartyTypeRepository = _guow.GetReadOnlyRepository<PartyType>();
            PartyCustomAttributeRepository = _guow.GetReadOnlyRepository<PartyCustomAttribute>();
        }

        ~PartyTypeManager()
        {
            Dispose(true);
        }
        public IReadOnlyRepository<PartyCustomAttribute> PartyCustomAttributeRepository { get; }
        public IReadOnlyRepository<PartyType> PartyTypeRepository { get; }
        public IQueryable<PartyType> PartyTypes => PartyTypeRepository.Query();

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

        #region PartyType

        public PartyType Create(string title, string description, string displayName, List<PartyStatusType> statusTypes, bool systemType = false)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(statusTypes != null && statusTypes.Count() > 0); // there should be at least one status defined for each party type --> status type 
            Contract.Ensures(Contract.Result<PartyType>() != null && Contract.Result<PartyType>().Id >= 0);
            //
            PartyType entity = new PartyType()
            {
                Title = title,
                Description = description,
                DisplayName = displayName,
                StatusTypes = statusTypes,
                SystemType = systemType
            };
            statusTypes.ForEach(item => item.PartyType = entity);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repo = uow.GetRepository<PartyType>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        /// <summary>
        /// delete rules:
        /// It shouldn't have any relevant party
        /// It shouldn't have a any statusType which has party status (it means partyStatusTypes without party status would be delete)
        /// It shouldn't have a any CustomAttributes which has CustomAttributeValues (it means CustomAttributes without CustomAttributeValues would be delete)
        /// </summary>
        /// <param name="partyType"></param>
        /// <returns></returns>
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
                //It shouldn't have any relevant party
                if (partyType.Parties != null && partyType.Parties.Count() > 0)
                    BexisException.Throw(partyType, "There are one or some parities which are using this entity.", BexisException.ExceptionType.Delete);
                // It shouldn't have a any statusType which has party status (it means partyStatusTypes without party status would be delete)
                if (partyType.StatusTypes != null && partyType.StatusTypes.Count(p => (p.Statuses != null && p.Statuses.Count() > 0)) > 0)
                    BexisException.Throw(partyType, "There are one or some 'StatusTypes' which have 'PartyStatus' and using this entity.", BexisException.ExceptionType.Delete);
                // It shouldn't have a any CustomAttributes which has CustomAttributeValues (it means CustomAttributes without CustomAttributeValues would be delete)
                if (partyType.CustomAttributes != null && partyType.CustomAttributes.Count(p => p.CustomAttributeValues.Count() > 0) > 0)
                    BexisException.Throw(partyType, "There are one or some 'CustomAttributes' which have 'CustomAttributeValues' and using this entity.", BexisException.ExceptionType.Delete);
                // delete the history
                repoPST.Delete(partyType.StatusTypes);
                repoCA.Delete(partyType.CustomAttributes);
                // remove all associations between the entity and its history items
                // the status types must also be deleted, not just the association between them and the party type.
                // partyType.StatusTypes.ToList().ForEach(a => a.PartyType=null);
                partyType.StatusTypes.Clear();
                // the custmom attributes must be also deleted.
                //partyType.CustomAttributes.ToList().ForEach(a => a.PartyType = null);
                partyType.CustomAttributes.Clear();
                //delete the entity
                repo.Delete(partyType);
                // commit changes
                uow.Commit();
                partyType = null;
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        /// <summary>
        /// delete rules:
        /// It shouldn't have any relevant party
        /// It shouldn't have a any statusType which has party status (it means partyStatusTypes without party status would be delete)
        /// It shouldn't have a any CustomAttributes which has CustomAttributeValues (it means CustomAttributes without CustomAttributeValues would be delete)
        /// </summary>
        /// <param name="partyType"></param>
        /// <returns></returns>
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
                    //It shouldn't have any relevant party
                    if (partyType.Parties != null && partyType.Parties.Count() > 0)
                        BexisException.Throw(partyType, "There are one or some parities which are using this entity.", BexisException.ExceptionType.Delete, true);
                    // It shouldn't have a any statusType which has party status (it means partyStatusTypes without party status would be delete)
                    if (partyType.StatusTypes != null && partyType.StatusTypes.Count(p => (p.Statuses != null && p.Statuses.Count() > 0)) > 0)
                        BexisException.Throw(partyType, "There are one or some 'StatusTypes' which have 'PartyStatus' and using this entity.", BexisException.ExceptionType.Delete, true);
                    // It shouldn't have a any CustomAttributes which has CustomAttributeValues (it means CustomAttributes without CustomAttributeValues would be delete)
                    if (partyType.CustomAttributes != null && partyType.CustomAttributes.Count(p => p.CustomAttributeValues.Count() > 0) > 0)
                        BexisException.Throw(partyType, "There are one or some 'CustomAttributes' which have 'CustomAttributeValues' and using this entity.", BexisException.ExceptionType.Delete, true);
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
        public PartyCustomAttribute CreatePartyCustomAttribute(PartyType partyType, string dataType, string name, string description, string validValues, string condition, bool isValueOptional = true, bool isUnique = false, bool isMain = false, int? displayOrder = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(partyType != null);

            Contract.Ensures(Contract.Result<PartyCustomAttribute>() != null && Contract.Result<PartyCustomAttribute>().Id >= 0);

            var entity = new PartyCustomAttribute()
            {
                DataType = dataType.ToLower(),
                Description = description,
                PartyType = partyType,
                ValidValues = validValues,
                IsValueOptional = isValueOptional,
                IsUnique = isUnique,
                IsMain = isMain,
                Name = name,
                Condition = condition
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttribute> repo = uow.GetRepository<PartyCustomAttribute>();
                //Name is unique for PartyCustomAttribute with the same party type
                if (repo.Get(item => item.Name == name && item.PartyType == partyType).Count > 0)
                    BexisException.Throw(entity, "This name for this type of 'PartyCustomAttribute' is already exist.", BexisException.ExceptionType.Add);
                //Calculate displayorder
                var partyCustomAttrs = repo.Get(item => item.PartyType == partyType);
                if (partyCustomAttrs.Count() == 0)
                    entity.DisplayOrder = 0;
                //if displayOrder is null then it goes to the last                
                else if (!displayOrder.HasValue)
                    entity.DisplayOrder = partyCustomAttrs.Max(item => item.DisplayOrder) + 1;
                //else it push the other items with the same displayOrder or greater than
                else
                {
                    entity.DisplayOrder = displayOrder.Value;
                    partyCustomAttrs.Where(item => item.DisplayOrder >= displayOrder.Value)
                        .ToList().ForEach(item => item.DisplayOrder = item.DisplayOrder + 1);

                }
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        public PartyCustomAttribute CreatePartyCustomAttribute(PartyCustomAttribute partyCustomeAttribute)
        {
            Contract.Requires(partyCustomeAttribute != null);
            Contract.Requires(partyCustomeAttribute.PartyType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(partyCustomeAttribute.Name));
            Contract.Ensures(Contract.Result<PartyCustomAttribute>() != null && Contract.Result<PartyCustomAttribute>().Id >= 0);

            var entity = new PartyCustomAttribute()
            {
                DataType = partyCustomeAttribute.DataType.ToLower(),
                Description = partyCustomeAttribute.Description,
                PartyType = partyCustomeAttribute.PartyType,
                ValidValues = partyCustomeAttribute.ValidValues,
                IsValueOptional = partyCustomeAttribute.IsValueOptional,
                IsUnique = partyCustomeAttribute.IsUnique,
                IsMain = partyCustomeAttribute.IsMain,
                Name = partyCustomeAttribute.Name,
                DisplayName = partyCustomeAttribute.DisplayName,
                Condition = partyCustomeAttribute.Condition
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttribute> repo = uow.GetRepository<PartyCustomAttribute>();
                //Name is unique for PartyCustomAttribute with the same party type
                if (repo.Get(item => item.Name == partyCustomeAttribute.Name && item.PartyType == partyCustomeAttribute.PartyType).Count > 0)
                    BexisException.Throw(entity, "This name for this type of 'PartyCustomAttribute' is already exist.", BexisException.ExceptionType.Add);
                //Calculate displayorder
                var partyCustomAttrs = repo.Get(item => item.PartyType == partyCustomeAttribute.PartyType);
                if (partyCustomAttrs.Count() == 0)
                    entity.DisplayOrder = 0;
                //if displayOrder is null then it goes to the last                
                else if (partyCustomeAttribute.DisplayOrder == 0)
                    entity.DisplayOrder = partyCustomAttrs.Max(item => item.DisplayOrder) + 1;
                //else it push the other items with the same displayOrder or greater than
                else
                {
                    entity.DisplayOrder = partyCustomeAttribute.DisplayOrder;
                    partyCustomAttrs.Where(item => item.DisplayOrder >= partyCustomeAttribute.DisplayOrder)
                        .ToList().ForEach(item => item.DisplayOrder = item.DisplayOrder + 1);

                }
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }


        public PartyCustomAttribute UpdatePartyCustomAttribute(PartyCustomAttribute partyCustomAttribute)
        {
            Contract.Requires(partyCustomAttribute != null, "Provided entities can not be null");
            Contract.Requires(partyCustomAttribute.Id >= 0, "Provided entitities must have a permanent ID");
            Contract.Ensures(Contract.Result<PartyCustomAttribute>() != null && Contract.Result<PartyCustomAttribute>().Id >= 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyCustomAttribute> repo = uow.GetRepository<PartyCustomAttribute>();
                //Name is unique for PartyCustomAttribute with the same party type
                if (partyCustomAttribute.PartyType.CustomAttributes.Where(item => item.Name == partyCustomAttribute.Name && item.Id != partyCustomAttribute.Id).Count() > 0)
                    BexisException.Throw(partyCustomAttribute, "This name for this type of 'PartyCustomAttribute' is already exist.", BexisException.ExceptionType.Add);
                //Calculate displayorder
                //if find the same displayOrder then it pushes the other items with the same displayOrder or greater than
                if (partyCustomAttribute.PartyType.CustomAttributes.FirstOrDefault(item => item.DisplayOrder == partyCustomAttribute.DisplayOrder && partyCustomAttribute.Id != item.Id) != null)
                    partyCustomAttribute.PartyType.CustomAttributes.Where(item => item.DisplayOrder >= partyCustomAttribute.DisplayOrder && partyCustomAttribute.Id != item.Id)
                        .ToList().ForEach(item => item.DisplayOrder = item.DisplayOrder + 1);
                repo.Merge(partyCustomAttribute);
                var merged = repo.Get(partyCustomAttribute.Id);
                repo.Put(merged);
                uow.Commit();
                return (merged);
            }
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
                //Prevent of deleting if there is a 'customAttributeVaue' for this entity
                if (entity.CustomAttributeValues.Count() > 0)
                    BexisException.Throw(entity, "There is one or more 'customAttributeVaue' for this entity.", BexisException.ExceptionType.Delete);
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
                repo.Evict();
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    //Prevent of deleting if there is a 'customAttributeVaue' for this entity
                    if (latest.CustomAttributeValues.Count() > 0)
                        BexisException.Throw(latest, "There is one or more 'customAttributeVaue' for this entity.", BexisException.ExceptionType.Delete, true);

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
            Contract.Requires(partyType != null && partyType.Id > 0);

            Contract.Ensures(Contract.Result<PartyStatusType>() != null && Contract.Result<PartyStatusType>().Id >= 0);

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
                var ff = repo.Get(item => item.Name == name && item.PartyType == partyType);
                //Name and partyType are unique in PartyStatusTypes 
                if (repo.Get(item => item.Name == name && item.PartyType == partyType).Count() > 0)
                    BexisException.Throw(entity, "This name with this PartyType is already exist.", BexisException.ExceptionType.Add);

                repo.Put(entity);
                uow.Commit();
            }
            return (entity);

        }

        public PartyStatusType GetStatusType(PartyType partyType, string name)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatusType> repoPartyStatusType = uow.GetRepository<PartyStatusType>();
                return repoPartyStatusType.Get(item => item.Name == name && item.PartyType == partyType).FirstOrDefault();
            }
        }
        public bool RemoveStatusType(PartyStatusType entity)
        {
            Contract.Requires(entity != null && entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatusType> repo = uow.GetRepository<PartyStatusType>();
                var latest = repo.Reload(entity);
                if (latest.Statuses.Count() > 0)
                    BexisException.Throw(latest, "There are some relevent 'PartStatus' to this entity.", BexisException.ExceptionType.Delete);

                //Atleast one 'PartyStatusType' is required for each 'PartyType' and 'PartyType' for this entity just has this 'PartyStatusType'.
                if (latest.PartyType.StatusTypes.Count() > 1)
                    BexisException.Throw(latest, "Atleast one 'PartyStatusType' is required for each 'PartyType' and 'PartyType' for this entity just has this 'PartyStatusType'.", BexisException.ExceptionType.Delete);
                //delete the entity
                repo.Delete(latest);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return true;
        }

        public bool RemoveStatusType(List<PartyStatusType> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyStatusType e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyStatusType e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyStatusType> repo = uow.GetRepository<PartyStatusType>();
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    if (latest.Statuses.Count() > 0)
                        BexisException.Throw(entity, "There are some relevent party status to this entity.", BexisException.ExceptionType.Delete, true);
                    //Atleast one 'PartyStatusType' is required for each 'PartyType' and 'PartyType' for this entity just has this 'PartyStatusType'.
                    if (latest.PartyType.StatusTypes.Count() > 1)
                        BexisException.Throw(latest, "Atleast one 'PartyStatusType' is required for each 'PartyType' and 'PartyType' for this entity just has this 'PartyStatusType'.", BexisException.ExceptionType.Delete, true);
                    //delete the entity
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return true;
        }
        #endregion
    }
}
