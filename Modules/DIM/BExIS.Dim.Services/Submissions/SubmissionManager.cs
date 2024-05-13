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
        public IQueryable<Submission> Submissions => SubmissionRepository.Query();
        public SubmissionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            SubmissionRepository = _guow.GetReadOnlyRepository<Submission>();
        }

        public void Create(Submission submission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Submission>();
                entityRequestRepository.Put(submission);
                uow.Commit();
            }
        }

        public Submission FindById(long id)
        {
            return SubmissionRepository.Get(id);
        }

        public void Update(Submission entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Submission>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
        }

        public void Delete(Submission submission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Submission>();
                entityRequestRepository.Delete(submission);
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
