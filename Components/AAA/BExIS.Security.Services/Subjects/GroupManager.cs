using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Subjects
{
    /// <summary>
    /// This manager implements all necessary functionalities for group management.
    /// </summary>
    public class GroupManager
    {
        private readonly IdentityGroupManager _identityGroupManager;

        private bool _isDisposed;

        public GroupManager()
        {
            _identityGroupManager = new IdentityGroupManager();
        }

        ~GroupManager()
        {
            Dispose(true);
        }

        public IQueryable<Group> Groups => _identityGroupManager.Roles;

        public Task<IdentityResult> CreateAsync(Group group)
        {
            return _identityGroupManager.CreateAsync(group);
        }

        public Task<IdentityResult> DeleteAsync(Group group)
        {
            return _identityGroupManager.DeleteAsync(group);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Task<Group> FindByIdAsync(long groupId)
        {
            return _identityGroupManager.FindByIdAsync(groupId);
        }

        public Task<Group> FindByNameAsync(string groupName)
        {
            return _identityGroupManager.FindByNameAsync(groupName);
        }

        public Task<IdentityResult> UpdateAsync(Group group)
        {
            return _identityGroupManager.UpdateAsync(group);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (!disposing) return;
            _identityGroupManager?.Dispose();
            _isDisposed = true;
        }

        ///// <summary>
        ///// This pri
        ///// </summary>
        //private IReadOnlyRepository<Group> GroupRepository { get; }

        ///// <summary>
        ///// This method takes a given group and persist it.
        ///// </summary>
        ///// <param name="group">The group that should be persisted.</param>
        //public void Create(Group group)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var groupRepository = uow.GetRepository<Group>();
        //        groupRepository.Put(group);
        //        uow.Commit();
        //    }
        //}

        ///// <summary>
        ///// This method creates a group based on the given parameters and tries to persist it.
        ///// </summary>
        ///// <param name="groupName">The name of the group.</param>
        ///// <param name="description">The description for more details about the group.</param>
        ///// <param name="isSystemGroup">This value reflects if the group is created/managed by the system or not.</param>
        ///// <param name="isValid">The validity of the group. Not used yet.</param>
        ///// <returns>The group that got persisted.</returns>
        //public Group Create(string groupName, string description, bool isSystemGroup = false, bool isValid = true)
        //{
        //    if (string.IsNullOrEmpty(groupName))
        //        return null;

        //    if (Exists(groupName, isSystemGroup))
        //        return null;

        //    var group = new Group()
        //    {
        //        Name = groupName,
        //        Description = description,
        //        IsSystemGroup = isSystemGroup,
        //        IsValid = isValid
        //    };
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var groupRepository = uow.GetRepository<Group>();
        //        groupRepository.Put(group);
        //        uow.Commit();
        //    }

        //    return group;
        //}

        ///// <summary>
        ///// This method takes a given groups and deletes it (removing it from persistence layer).
        ///// </summary>
        ///// <param name="group">The group that should be deleted.</param>
        //public void Delete(Group group)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var groupRepository = uow.GetRepository<Group>();
        //        groupRepository.Delete(group);
        //        uow.Commit();
        //    }
        //}

        ///// <summary>
        ///// This method checks if a group with the given parameters already exists.
        ///// </summary>
        ///// <param name="groupName">The name of the group.</param>
        ///// <param name="isSystemGroup">The information if the group is managed by the system.</param>
        ///// <returns>If a group with the given parameters exists, it returns true, else false.</returns>
        //public bool Exists(string groupName, bool isSystemGroup = false)
        //{
        //    return GroupRepository.Get(g => g.Name.ToUpperInvariant() == groupName.ToUpperInvariant() && g.IsSystemGroup == isSystemGroup).Count == 1;
        //}

        ///// <summary>
        ///// The method is looking for a persisted group based on the given parameter(s).
        ///// </summary>
        ///// <param name="groupId">The unique identifier of the group.</param>
        ///// <returns>If a group with the given parameters exists, it returns the group, else null.</returns>
        //public Group FindById(long groupId)
        //{
        //    return GroupRepository.Get(groupId);
        //}

        //public Group FindByName(string groupName)
        //{
        //    return GroupRepository.Query(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()).FirstOrDefault();
        //}

        ///// <summary>
        ///// The method takes a given group and updates it - e.g. modify description or other properties.
        ///// </summary>
        ///// <param name="group">The group that should be updated.</param>
        //public void Update(Group group)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var groupRepository = uow.GetRepository<Group>();
        //        groupRepository.Put(group);
        //        uow.Commit();
        //    }
        //}
    }
}