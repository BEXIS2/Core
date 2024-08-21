using BExIS.Dlm.Entities.Data;
using System;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Data
{
    public class TagManager : IDisposable
    {
        private IUnitOfWork guow = null;

        public TagManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<Tag>();
        }

        private bool isDisposed = false;

        ~TagManager()
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

        public IReadOnlyRepository<Tag> Repo { get; private set; }

        public Tag Create(Tag Tag)
        {
            if (Tag == null) throw new ArgumentNullException("Tag must not be null.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<Tag> repo = uow.GetRepository<Tag>();
                    repo.Put(Tag);
                    uow.Commit();

                    return (Tag);
                }
                catch (Exception ex)
                {
                    throw new Exception("Tag creation failed.", ex);
                }
            }
        }

        public Tag Create()
        {
            Tag Tag = new Tag()
            {
                Nr = 0,
                Show = false,
                Final = false
            };

            if (Tag == null) throw new ArgumentNullException("Tag must not be null.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<Tag> repo = uow.GetRepository<Tag>();
                    repo.Put(Tag);
                    uow.Commit();

                    return (Tag);
                }
                catch (Exception ex)
                {
                    throw new Exception("Tag creation failed.", ex);
                }
            }
        }

        public Tag Update(Tag Tag)
        {
            if (Tag == null) throw new ArgumentNullException("Tag must not be null.");

            Contract.Ensures(Contract.Result<Tag>() != null && Contract.Result<Tag>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<Tag> repo = uow.GetRepository<Tag>();
                    repo.Merge(Tag);
                    var merged = repo.Get(Tag.Id);
                    repo.Put(merged);
                    uow.Commit();

                    return (merged);
                }
                catch (Exception ex)
                {
                    throw new Exception("Tag creation failed.", ex);
                }
            }
        }

        public bool Delete(long id)
        {
            if (id == 0) throw new ArgumentException("Tag must not be null.");

            Contract.Ensures(Contract.Result<Tag>() != null && Contract.Result<Tag>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Tag> repo = uow.GetRepository<Tag>();

                var e = repo.Get(id);

                if (e != null)
                {
                    repo.Delete(e);
                    uow.Commit();

                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("the Tag with the id {0} does not exist", id));
                }
            }
        }

        public bool Delete(Tag Tag)
        {
            if (Tag == null) throw new ArgumentNullException("Tag must not be null.");

            Contract.Ensures(Contract.Result<Tag>() != null && Contract.Result<Tag>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Tag> repo = uow.GetRepository<Tag>();

                repo.Delete(Tag);
                uow.Commit();

                return true;
            }
        }
    }
}