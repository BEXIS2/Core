using BExIS.Dim.Entities.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Submissions
{
    public class RepositoryManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public IReadOnlyRepository<Repository> RepositoryRepository { get; }

        public IQueryable<Repository> Agents => RepositoryRepository.Query();

        public RepositoryManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            RepositoryRepository = _guow.GetReadOnlyRepository<Repository>();
        }

        ~RepositoryManager()
        {
            Dispose(true);
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
