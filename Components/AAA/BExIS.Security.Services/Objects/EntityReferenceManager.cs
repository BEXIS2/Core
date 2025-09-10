using BExIS.Security.Entities.Objects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class EntityReferenceManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public EntityReferenceManager()
        {
            _guow = this.GetIsolatedUnitOfWork();

            ReferenceRepository = _guow.GetReadOnlyRepository<EntityReference>();
        }

        ~EntityReferenceManager()
        {
            Dispose(true);
        }

        public IQueryable<EntityReference> References => ReferenceRepository.Query();

        public IReadOnlyRepository<EntityReference> ReferenceRepository { get; }

        public bool Exist(EntityReference entityReference, bool includeVersion = false, bool ignoreSourceVersion = false)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();

                if (includeVersion && !ignoreSourceVersion)
                {
                    if (repo.Query(r =>
                         r.SourceId.Equals(entityReference.SourceId) &&
                         r.SourceEntityId.Equals(entityReference.SourceEntityId) &&
                         r.SourceVersion.Equals(entityReference.SourceVersion) &&
                         r.TargetId.Equals(entityReference.TargetId) &&
                         r.TargetEntityId.Equals(entityReference.TargetEntityId) &&
                         r.TargetVersion.Equals(entityReference.TargetVersion) &&
                         r.Context.Equals(entityReference.Context) &&
                         r.ReferenceType.Equals(entityReference.ReferenceType) &&
                         r.LinkType.Equals(entityReference.LinkType) &&
                         r.Category.Equals(entityReference.Category)

                    ).Count() == 0) return false;
                }
                else if (includeVersion && ignoreSourceVersion)
                {
                    if (repo.Query(r =>
                         r.SourceId.Equals(entityReference.SourceId) &&
                         r.SourceEntityId.Equals(entityReference.SourceEntityId) &&
                         r.TargetId.Equals(entityReference.TargetId) &&
                         r.TargetEntityId.Equals(entityReference.TargetEntityId) &&
                         r.TargetVersion.Equals(entityReference.TargetVersion) &&
                         r.Context.Equals(entityReference.Context) &&
                         r.ReferenceType.Equals(entityReference.ReferenceType) &&
                         r.LinkType.Equals(entityReference.LinkType) &&
                         r.Category.Equals(entityReference.Category)


                    ).Count() == 0) return false;
                }
                else
                {
                    if (repo.Query(r =>
                         r.SourceId.Equals(entityReference.SourceId) &&
                         r.SourceEntityId.Equals(entityReference.SourceEntityId) &&
                         r.TargetId.Equals(entityReference.TargetId) &&
                         r.TargetEntityId.Equals(entityReference.TargetEntityId) &&
                         r.Context.Equals(entityReference.Context) &&
                         r.ReferenceType.Equals(entityReference.ReferenceType) &&
                         r.LinkType.Equals(entityReference.LinkType) &&
                         r.Category.Equals(entityReference.Category)

                    ).Count() == 0) return false;
                }

                return true;
            }
        }

        public void Create(EntityReference entityReference)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();
                repo.Put(entityReference);
                uow.Commit();
            }
        }

        public EntityReference Create(long sourceId, long sourceEntityId, int sourceEntityVersion, long targetId, long targetEntityId, int targetEntityVersion, string context, string type, string linkType, string category)
        {
            EntityReference entityReference = new EntityReference(sourceId, sourceEntityId, sourceEntityVersion, targetId, targetEntityId, targetEntityVersion, context, type, DateTime.Now, linkType, category);

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();
                repo.Put(entityReference);
                uow.Commit();

                return entityReference;
            }
        }

        public void Delete(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();
                var entityReference = repo.Get(id);
                repo.Delete(entityReference);
                uow.Commit();
            }
        }

        public void Delete(EntityReference entityReference)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();
                repo.Delete(entityReference);
                uow.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
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
    }
}