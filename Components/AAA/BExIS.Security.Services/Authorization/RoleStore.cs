using BExIS.Security.Entities.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class RoleStore : IQueryableRoleStore<Role, long>
    {
        public IReadOnlyRepository<Role> RoleRepository { get; }

        public RoleStore()
        {
            var uow = this.GetUnitOfWork();

            RoleRepository = uow.GetReadOnlyRepository<Role>();
        }

        public IQueryable<Role> Roles => RoleRepository.Query();

        public Task CreateAsync(Role role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var roleRepository = uow.GetRepository<Role>();
                roleRepository.Put(role);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        public Task UpdateAsync(Role role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var roleRepository = uow.GetRepository<Role>();
                roleRepository.Put(role);
                uow.Commit();
            }
            return Task.FromResult(0);
        }

        public Task DeleteAsync(Role role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var roleRepository = uow.GetRepository<Role>();
                roleRepository.Delete(role);
                uow.Commit();
            }
            return Task.FromResult(0);
        }

        public Task<Role> FindByIdAsync(long roleId)
        {
            return Task.FromResult(RoleRepository.Get(roleId));
        }

        public Task<Role> FindByNameAsync(string roleName)
        {
            return Task.FromResult(RoleRepository.Query(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()).FirstOrDefault());
        }

        public void Dispose()
        {
            // DO NOTHING!
        }
    }
}