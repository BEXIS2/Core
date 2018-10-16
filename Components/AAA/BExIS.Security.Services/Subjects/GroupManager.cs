using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class GroupManager : IQueryableRoleStore<Group, long>
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public GroupManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            GroupRepository = _guow.GetReadOnlyRepository<Group>();
        }

        ~GroupManager()
        {
            Dispose(true);
        }

        public IQueryable<Group> Groups => GroupRepository.Query();
        public IQueryable<Group> Roles => GroupRepository.Query();
        private IReadOnlyRepository<Group> GroupRepository { get; }

        public Task CreateAsync(Group role)
        {
            if (string.IsNullOrEmpty(role.Name))
                return Task.FromResult(0);

            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Put(role);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        public Task DeleteAsync(Group role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Delete(role);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Task<Group> FindByIdAsync(long roleId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                return Task.FromResult(groupRepository.Get(roleId));
            }
        }

        public Task<Group> FindByNameAsync(string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                return Task.FromResult(groupRepository.Query().FirstOrDefault(u => u.Name.ToUpperInvariant() == roleName.ToUpperInvariant()));
            }
        }

        public Task UpdateAsync(Group role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Merge(role);
                var r = groupRepository.Get(role.Id);
                groupRepository.Put(r);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        protected virtual void Dispose(bool disposing)
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