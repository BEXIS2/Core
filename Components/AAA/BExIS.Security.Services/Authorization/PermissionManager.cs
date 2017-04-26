using BExIS.Security.Entities.Authorization;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class PermissionManager
    {
        public PermissionManager()
        {
            var uow = this.GetUnitOfWork();

            PermissionRepository = uow.GetReadOnlyRepository<Permission>();
        }

        public IReadOnlyRepository<Permission> PermissionRepository { get; private set; }
        public IQueryable<Permission> Permissions => PermissionRepository.Query();
    }
}