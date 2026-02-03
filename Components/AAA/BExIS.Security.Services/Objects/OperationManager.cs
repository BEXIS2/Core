using BExIS.Security.Entities.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class OperationManager
    {
        public Operation Create(string module, string controller, string action, Feature feature = null)
        {
            if (string.IsNullOrWhiteSpace(module))
                throw new ArgumentException(nameof(module));

            if (string.IsNullOrWhiteSpace(controller))
                throw new ArgumentException(nameof(controller));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException(nameof(action));

            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetRepository<Operation>();
                
                if (operationRepository.Query(r => r.Module == module && r.Controller == controller && r.Action == action).Any())
                    return null;

                var operation = new Operation()
                {
                    Module = module,
                    Controller = controller,
                    Action = action,
                    Feature = feature
                };

                operationRepository.Put(operation);
                uow.Commit();

                return operation;
            }
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

                return operationRepository.Query(o => o.Module == module && o.Controller == controller && o.Action == action).Take(2).Count() == 1;
            }
        }


        public IList<Operation> Find(Expression<Func<Operation, bool>> predicate)
        {
            using(var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                return operationRepository.Query(predicate).ToList();
            }
        }

        public Operation Get(string module, string controller, string action)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                return operationRepository.Query(x => x.Module == module && x.Controller == controller && x.Action == action).SingleOrDefault();
            }
        }

        public Operation GetById(long operationId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                return operationRepository.Get(operationId);
            }
        }

        public void Update(Operation operation)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Operation>();
                repo.Merge(operation);
                uow.Commit();
            }
        }
    }
}