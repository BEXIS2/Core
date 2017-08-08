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

        public IReadOnlyRepository<Operation> OperationRepository { get; }
        public IQueryable<Operation> Operations => OperationRepository.Query();

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

        public Operation Find(string module, string controller, string action)
        {
            return
                OperationRepository.Query(x => x.Module == module && x.Controller == controller && x.Action == action)
                    .FirstOrDefault();
        }
    }
}