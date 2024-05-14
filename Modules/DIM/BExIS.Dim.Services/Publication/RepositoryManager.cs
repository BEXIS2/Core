using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Entities.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Publication
{
    public class RepositoryManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        ~RepositoryManager()
        {
            Dispose(true);
        }
        public IReadOnlyRepository<Repository> Repository { get; }
        public IQueryable<Repository> Repositories => Repository.Query();
        public RepositoryManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            Repository = _guow.GetReadOnlyRepository<Repository>();
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

        public Repository FindById(long id)
        {
            return Repository.Get(id);
        }
    }
}
