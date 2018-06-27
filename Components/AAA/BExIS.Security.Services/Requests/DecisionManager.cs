using BExIS.Security.Entities.Requests;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public class DecisionManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public DecisionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            DecisionRepository = _guow.GetReadOnlyRepository<Decision>();
        }

        ~DecisionManager()
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

        public IReadOnlyRepository<Decision> DecisionRepository { get; }
        public IQueryable<Decision> Decisions => DecisionRepository.Query();

        public void Accept(long decisionId, string reason)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();

                var decision = decisionRepository.Get(decisionId);

                if (decision != null)
                {
                    decision.Status = DecisionStatus.Accepted;
                    decision.DecisionDate = DateTime.Now;
                    decision.Reason = reason;

                    decisionRepository.Merge(decision);
                    var mergedDecision = decisionRepository.Get(decision.Id);
                    decisionRepository.Put(mergedDecision);

                    if (decisionRepository.Query(m => m.Request.Id == decision.Request.Id).ToList()
                        .All(m => m.Status != DecisionStatus.Open))
                    {
                        var request = requestRepository.Get(decision.Request.Id);

                        if (request != null)
                        {
                            request.Status = RequestStatus.Accepted;

                            requestRepository.Merge(request);
                            var mergedRequest = requestRepository.Get(request.Id);
                            requestRepository.Put(mergedRequest);

                        }
                    }
                }

                uow.Commit();
            }
        }

        public void Reject(long decisionId, string reason)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();

                var decision = decisionRepository.Get(decisionId);

                if (decision != null)
                {
                    decision.Status = DecisionStatus.Rejected;
                    decision.DecisionDate = DateTime.Now;
                    decision.Reason = reason;

                    decisionRepository.Merge(decision);
                    var mergedDecision = decisionRepository.Get(decision.Id);
                    decisionRepository.Put(mergedDecision);


                    if (decisionRepository.Query(m => m.Request.Id == decision.Request.Id)
                        .All(m => m.Status != DecisionStatus.Open))
                    {
                        var request = requestRepository.Get(decision.Request.Id);

                        if (request != null)
                        {
                            request.Status = RequestStatus.Rejected;

                            requestRepository.Merge(request);
                            var mergedRequest = requestRepository.Get(request.Id);
                            requestRepository.Put(mergedRequest);
                        }
                    }

                    uow.Commit();
                }
            }
        }

        public void Create(Decision decision)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                decisionRepository.Put(decision);
                uow.Commit();
            }
        }

        public void Delete(Decision decision)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                decisionRepository.Delete(decision);
                uow.Commit();
            }
        }

        public Decision FindById(long decisionId)
        {
            return DecisionRepository.Get(decisionId);
        }

        public void Update(Decision entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Decision>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
        }
    }
}