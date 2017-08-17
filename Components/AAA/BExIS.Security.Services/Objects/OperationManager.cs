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

        public Operation Create(string module, string controller, string action, Feature feature = null)
        {
            var operation = new Operation()
            {
                Module = module,
                Controller = controller,
                Action = action,
                Feature = feature
            };

            using (var uow = this.GetUnitOfWork())
            {
                var workflowRepository = uow.GetRepository<Operation>();
                workflowRepository.Put(operation);
                uow.Commit();
            }

            return operation;
        }

        public bool Exists(string module, string controller, string action)
        {
            if (string.IsNullOrEmpty(module))
                return false;

            if (string.IsNullOrEmpty(controller))
                return false;

            if (string.IsNullOrEmpty(action))
                return false;

            return OperationRepository.Query(o => o.Module.ToUpperInvariant() == module.ToUpperInvariant() && o.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && o.Action.ToUpperInvariant() == action.ToUpperInvariant()).Count() == 1;
        }

        public Operation FindById(long operationId)
        {
            return OperationRepository.Get(operationId);
        }

        public Operation Find(string module, string controller, string action)
        {
            return
                OperationRepository.Query(x => x.Module == module && x.Controller == controller && x.Action == action)
                    .FirstOrDefault();
        }
    }
}