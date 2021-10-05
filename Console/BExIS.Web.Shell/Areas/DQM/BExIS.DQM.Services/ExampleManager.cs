using BExIS.DQM.Entities.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.DQM.Services
{
    public class ExampleManager : IDisposable
    {
        private IUnitOfWork guow = null;
        public ExampleManager()
        {
            guow = this.GetIsolatedUnitOfWork();
        }

        private bool isDisposed = false;
        ~ExampleManager()
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

        #region Example

        public IEnumerable<Example> Get()
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Example>().Get();
        }

        public Example Get(long id)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Example>().Get().FirstOrDefault(e => e.Id.Equals(id));
        }

        public Example Create(long id, string name)
        {
            Contract.Requires(id >= 0);

            //LinkElement parent = this.GetLinkElement(parentId);

            Example example;

            example = new Example()
            {
                Id = id,
                Name = name
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Example> repo = uow.GetRepository<Example>();
                repo.Put(example);
                uow.Commit();

            }

            return (example);
        }

        public bool Delete(Example entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Example> repo = uow.GetRepository<Example>();

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public Example Update(long id)
        {
            var example = this.GetUnitOfWork().GetReadOnlyRepository<Example>().Get(id);

            if (example != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Example> repo = uow.GetRepository<Example>();
                    repo.Put(example);
                    uow.Commit();
                }
            }

            return (example);
        }

        #endregion


    }
}
