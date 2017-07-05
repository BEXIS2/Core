using BExIS.Security.Entities.Objects;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class OperationManager
    {
        public OperationManager()
        {
            var uow = this.GetUnitOfWork();

            OperationRepository = uow.GetReadOnlyRepository<Operation>();
        }

        public IQueryable<Operation> Operations => OperationRepository.Query();
        public IReadOnlyRepository<Operation> OperationRepository { get; }

        public Operation Create(string module, string controller, string action, Operation parent = null, Workflow workflow = null)
        {
            var operation = new Operation()
            {
                Module = module,
                Controller = controller,
                Action = action,
                Workflow = workflow,
                Parent = parent
            };

            using (var uow = this.GetUnitOfWork())
            {
                var workflowRepository = uow.GetRepository<Operation>();
                workflowRepository.Put(operation);
                uow.Commit();
            }

            return operation;
        }
    }
}
