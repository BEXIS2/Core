using BExIS.Security.Entities.Objects;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class OperationManager
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public OperationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            OperationRepository = _guow.GetReadOnlyRepository<Operation>();
        }

        ~OperationManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Operation> OperationRepository { get; }

        public IQueryable<Operation> Operations => OperationRepository.Query();

        public Operation Create(string module, string controller, string action, Feature feature = null)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (Exists(module, controller, action))
                    return null;

                var operation = new Operation()
                {
                    Module = module,
                    Controller = controller,
                    Action = action,
                    Feature = feature
                };

                var operationRepository = uow.GetRepository<Operation>();
                operationRepository.Put(operation);
                uow.Commit();

                return operation;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Exists(string module, string controller, string action)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();

                if (string.IsNullOrEmpty(module))
                    return false;

                if (string.IsNullOrEmpty(controller))
                    return false;

                if (string.IsNullOrEmpty(action))
                    return false;

                return operationRepository.Query(o => o.Module.ToUpperInvariant() == module.ToUpperInvariant() && o.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && o.Action.ToUpperInvariant() == action.ToUpperInvariant()).Count() == 1;
            }
        }

        public Operation Find(string module, string controller, string action)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                return operationRepository.Query(x => x.Module.ToUpperInvariant() == module.ToUpperInvariant() && x.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && x.Action.ToUpperInvariant() == action.ToUpperInvariant()).FirstOrDefault();
            }
        }

        public Operation FindById(long operationId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                return operationRepository.Get(operationId);
            }
        }

        public void Update(Operation entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Operation>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
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