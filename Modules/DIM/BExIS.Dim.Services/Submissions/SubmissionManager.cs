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

        public void Create(Submission submission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Submission>();
                decisionRepository.Put(submission);
                uow.Commit();
            }
        }

        public void Delete(Submission submission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Submission>();
                decisionRepository.Delete(submission);
                uow.Commit();
            }
        }

        public void DeleteById(long submissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Submission>();
                decisionRepository.Delete(submissionId);
                uow.Commit();
            }
        }

        public Submission FindById(long submissionId)
        {
            return SubmissionRepository.Get(submissionId);
        }
    }
}
