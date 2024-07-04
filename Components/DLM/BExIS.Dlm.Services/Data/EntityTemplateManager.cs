using BExIS.Dlm.Entities.Data;
using System;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Data
{
    public class EntityTemplateManager : IDisposable
    {
        private IUnitOfWork guow = null;

        public EntityTemplateManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<EntityTemplate>();
        }

        private bool isDisposed = false;

        ~EntityTemplateManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }

        public IReadOnlyRepository<EntityTemplate> Repo { get; private set; }

        public EntityTemplate Create(EntityTemplate entityTemplate)
        {
            if (entityTemplate == null) throw new ArgumentNullException("Entity template must not be null.");
            if (entityTemplate.EntityType == null) throw new ArgumentNullException("Entity type must not be null.");
            if (entityTemplate.MetadataStructure == null) throw new ArgumentNullException("MetadataStructure must not be null.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<EntityTemplate> repo = uow.GetRepository<EntityTemplate>();
                    repo.Put(entityTemplate);
                    uow.Commit();

                    return (entityTemplate);
                }
                catch (Exception ex)
                {
                    throw new Exception("EntityTemplate creation failed.", ex);
                }
            }
        }

        public EntityTemplate Update(EntityTemplate entityTemplate)
        {
            if (entityTemplate == null) throw new ArgumentNullException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<EntityTemplate>() != null && Contract.Result<EntityTemplate>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<EntityTemplate> repo = uow.GetRepository<EntityTemplate>();
                    repo.Merge(entityTemplate);
                    var merged = repo.Get(entityTemplate.Id);
                    repo.Put(merged);
                    uow.Commit();

                    return (merged);
                }
                catch (Exception ex)
                {
                    throw new Exception("EntityTemplate creation failed.", ex);
                }
            }
        }

        public bool Delete(long id)
        {
            if (id == 0) throw new ArgumentException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<EntityTemplate>() != null && Contract.Result<EntityTemplate>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<EntityTemplate> repo = uow.GetRepository<EntityTemplate>();

                var e = repo.Get(id);

                if (e != null)
                {
                    repo.Delete(e);
                    uow.Commit();

                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("the entity template with the id {0} does not exist", id));
                }
            }
        }

        public bool Delete(EntityTemplate entityTemplate)
        {
            if (entityTemplate == null) throw new ArgumentNullException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<EntityTemplate>() != null && Contract.Result<EntityTemplate>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<EntityTemplate> repo = uow.GetRepository<EntityTemplate>();

                repo.Delete(entityTemplate);
                uow.Commit();

                return true;
            }
        }
    }
}