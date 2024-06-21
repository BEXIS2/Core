using BExIS.Dim.Entities.Publications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Publications
{
    public class RepositoryManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public RepositoryManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            RepositoryRepository = _guow.GetReadOnlyRepository<Repository>();
        }

        ~RepositoryManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Repository> RepositoryRepository { get; }
        public IQueryable<Repository> Repositories => RepositoryRepository.Query();

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

        public Repository Create(Repository repository)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var entityRequestRepository = uow.GetRepository<Repository>();
                    entityRequestRepository.Put(repository);
                    uow.Commit();
                }

                return repository;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Delete(Repository repository)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var brokerRepository = uow.GetRepository<Repository>();
                    brokerRepository.Delete(repository);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteById(long repositoryId)
        {
            return Delete(RepositoryRepository.Get(repositoryId));
        }

        public Task<Repository> FindByIdAsync(long repositoryId)
        {
            return Task.FromResult(RepositoryRepository.Get(repositoryId));

        }

        public Repository FindById(long repositoryId)
        {
            return RepositoryRepository.Get(repositoryId);
        }

        public List<Repository> FindByName(string name)
        {
            return RepositoryRepository.Query(r => string.Equals(r.Name, name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public bool Update(Repository repository)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetRepository<Repository>();
                    repo.Merge(repository);
                    var merged = repo.Get(repository.Id);
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
