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
    }
}
