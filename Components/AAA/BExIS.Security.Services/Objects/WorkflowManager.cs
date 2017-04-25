using BExIS.Security.Entities.Objects;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class WorkflowManager
    {
        public WorkflowManager()
        {
            var uow = this.GetUnitOfWork();

            WorkflowRepository = uow.GetReadOnlyRepository<Workflow>();
        }

        public IQueryable<Workflow> Entities => WorkflowRepository.Query();
        public IReadOnlyRepository<Workflow> WorkflowRepository { get; }

        public void Create(Workflow workflow)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var workflowRepository = uow.GetRepository<Workflow>();
                workflowRepository.Put(workflow);
                uow.Commit();
            }
        }

        public void Delete(Workflow workflow)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var workflowRepository = uow.GetRepository<Workflow>();
                workflowRepository.Delete(workflow);
                uow.Commit();
            }
        }

        public Workflow FindByIdAsync(long workflowId)
        {
            return WorkflowRepository.Get(workflowId);
        }

        public void Update(Workflow workflow)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var workflowRepository = uow.GetRepository<Workflow>();
                workflowRepository.Put(workflow);
                uow.Commit();
            }
        }
    }
}