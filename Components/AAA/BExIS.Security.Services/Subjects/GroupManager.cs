using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class GroupManager
    {
        public GroupManager()
        {
            var uow = this.GetUnitOfWork();

            GroupRepository = uow.GetReadOnlyRepository<Group>();
            RoleRepository = uow.GetReadOnlyRepository<Role>();
        }

        public IReadOnlyRepository<Group> GroupRepository { get; }
        public IQueryable<Group> Groups => GroupRepository.Query();
        public IReadOnlyRepository<Role> RoleRepository { get; }

        public void AddToRole(Group group, string roleName)
        {
            group.Roles.Add(RoleRepository.Query(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()).FirstOrDefault());
            Update(group);
        }

        public void Create(Group group)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Put(group);
                uow.Commit();
            }
        }

        public void Delete(Group group)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Delete(group);
                uow.Commit();
            }
        }

        public Group FindByIdAsync(long groupId)
        {
            return GroupRepository.Get(groupId);
        }

        public Group FindByNameAsync(string groupName)
        {
            return GroupRepository.Query(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()).FirstOrDefault();
        }

        public void RemoveFromRole(Group group, string roleName)
        {
            group.Roles.Remove(RoleRepository.Query(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()).FirstOrDefault());
            Update(group);
        }

        public void Update(Group group)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Put(group);
                uow.Commit();
            }
        }
    }
}