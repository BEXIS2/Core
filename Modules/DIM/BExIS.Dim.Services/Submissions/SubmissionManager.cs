using BExIS.Dim.Entities.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Submissions
{
    public class SubmissionManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public IReadOnlyRepository<Submission> SubmissionRepository { get; }

        public IQueryable<Submission> Agents => SubmissionRepository.Query();

        public SubmissionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            SubmissionRepository = _guow.GetReadOnlyRepository<Submission>();
        }

        ~SubmissionManager()
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
