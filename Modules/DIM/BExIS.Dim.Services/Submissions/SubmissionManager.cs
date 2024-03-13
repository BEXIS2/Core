using BExIS.Dim.Entities.Submissions;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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

        public List<Submission> GetSubmissions(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            var orderbyClause = orderBy?.ToLINQ();
            var whereClause = filter?.ToLINQ();
            count = 0;
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    if (whereClause != null && orderBy != null)
                    {
                        var l = Submissions.Where(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = Submissions.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    if (orderBy != null)
                    {
                        count = Submissions.Count();
                        return Submissions.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    return Submissions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered submissions."), ex);
            }
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

        public Submission Create(Submission submission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var submissionRepository = uow.GetRepository<Submission>();
                submissionRepository.Put(submission);
                uow.Commit();
            }

            return submission;
        }

        public void Delete(Submission submission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var submissionRepository = uow.GetRepository<Submission>();
                submissionRepository.Delete(submission);
                uow.Commit();
            }
        }

        public void DeleteById(long submissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var submissionRepository = uow.GetRepository<Submission>();
                submissionRepository.Delete(submissionId);
                uow.Commit();
            }
        }

        public Submission FindById(long submissionId)
        {
            return SubmissionRepository.Get(submissionId);
        }

        public Submission Update(Submission submission)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetRepository<Submission>();
                    repo.Merge(submission);
                    var merged = repo.Get(submission.Id);
                    repo.Put(merged);
                    uow.Commit();

                    return merged;
                }
            }
            catch(Exception)
            {
                return null;
            }

        }
    }
}
