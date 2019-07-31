using BExIS.Security.Entities.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Create(EntityReference entityReference)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();
                repo.Put(entityReference);
                uow.Commit();
            }
        }

        public EntityReference Create(long sourceId, long sourceEntityId, long targetId, long targetEntityId, string context)
        {
            EntityReference entityReference = new EntityReference(sourceId, sourceEntityId, targetId, targetEntityId, context);

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityReference>();
                repo.Put(entityReference);
                uow.Commit();

                return entityReference;
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