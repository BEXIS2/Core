using BExIS.Security.Entities.Requests;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public class DecisionManager
    {
        public DecisionManager()
        {
            var uow = this.GetUnitOfWork();

            DecisionRepository = uow.GetReadOnlyRepository<Decision>();
        }

        public IReadOnlyRepository<Decision> DecisionRepository { get; }
        public IQueryable<Decision> Decisions => DecisionRepository.Query();

        public void Accept(long decisionId, string reason)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityDecisionRepository = uow.GetRepository<Decision>();
                var entityDecision = entityDecisionRepository.Get(decisionId);

                if (entityDecision != null)
                {
                    entityDecision.Status = DecisionStatus.Accepted;
                    entityDecision.DecisionDate = DateTime.Now;
                    entityDecision.Reason = reason;

                    Update(entityDecision);
                }
            }
        }

        public void Reject(long decisionId, string reason)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityDecisionRepository = uow.GetRepository<Decision>();
                var entityDecision = entityDecisionRepository.Get(decisionId);

                if (entityDecision != null)
                {
                    entityDecision.Status = DecisionStatus.Rejected;
                    entityDecision.DecisionDate = DateTime.Now;
                    entityDecision.Reason = reason;

                    Update(entityDecision);
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