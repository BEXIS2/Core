using BExIS.Dim.Entities.Publications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Publications
{
    public class PublicationManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public PublicationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            PublicationRepository = _guow.GetReadOnlyRepository<Publication>();
        }

        ~PublicationManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Publication> PublicationRepository { get; }
        public IQueryable<Publication> Publications => PublicationRepository.Query();

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

        public Publication Create(Publication publication)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var publicationRepository = uow.GetRepository<Publication>();
                    publicationRepository.Put(publication);
                    uow.Commit();
                }

                return publication;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Delete(Publication publication)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var publicationRepository = uow.GetRepository<Publication>();
                    publicationRepository.Delete(publication);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool DeleteById(long publicationId)
        {
            return Delete(PublicationRepository.Get(publicationId));
        }

        public Publication FindById(long publicationId)
        {
            return PublicationRepository.Get(publicationId);
        }

        public bool Update(Publication publication)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetRepository<Publication>();
                    repo.Merge(publication);
                    var merged = repo.Get(publication.Id);
                    repo.Put(merged);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
