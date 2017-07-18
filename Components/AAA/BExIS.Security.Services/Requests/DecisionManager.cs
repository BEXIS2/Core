using BExIS.Security.Entities.Requests;
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

        public void Update(Decision decision)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                decisionRepository.Put(decision);
                uow.Commit();
            }
        }
    }
}