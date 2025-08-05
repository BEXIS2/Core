using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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

        /// <summary>
        /// returns subset of groups based on the parameters
        /// and also count of filtered list
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<Group> GetGroups(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            var orderbyClause = orderBy?.ToLINQ();
            var whereClause = filter?.ToLINQ();
            count = 0;
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    if (whereClause != null && orderBy != null)
                    {
                        var l = Groups.Where(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = Groups.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    if (orderBy != null)
                    {
                        count = Groups.Count();
                        return Groups.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    count = count = Groups.Count();

                    // without filter and order
                    return Groups.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered groups."), ex);
            }
        }

        public Task CreateAsync(Group role)
        {
            if (role == null)
                //return Task.FromException(new Exception());
                return Task.CompletedTask;

            if (string.IsNullOrEmpty(role.Name))
                //return Task.FromException(new Exception());
                return Task.CompletedTask;

            if (FindByNameAsync(role.Name)?.Result != null)
                //return Task.FromException(new Exception());
                return Task.CompletedTask;

            var groupRepository = _guow.GetRepository<Group>();
            groupRepository.Put(role);
            _guow.Commit();

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Group role)
        {
            var groupRepository = _guow.GetRepository<Group>();
            groupRepository.Delete(role);
            _guow.Commit();

            return Task.CompletedTask;
        }

        public Task<bool> DeleteByIdAsync(long roleId)
        {
            var groupRepository = _guow.GetRepository<Group>();
            var group = groupRepository.Get(roleId);

            if (group == null)
                //return Task.FromException(new Exception());
                return Task.FromResult(false);


            // Users
            var userRepository = _guow.GetRepository<User>();
            foreach (var user in group.Users)
            {
                user.Groups.Remove(group);
                userRepository.Put(user);
            }

            // EntityPermissions
            var entityPermissionRepository = _guow.GetRepository<EntityPermission>();
            foreach (var entityPermission in entityPermissionRepository.Get(e => e.Subject.Id == roleId))
            {
                entityPermissionRepository.Delete(entityPermission);
            }

            // FeaturePermissions
            var featurePermissionRepository = _guow.GetRepository<FeaturePermission>();
            foreach (var featurePermission in featurePermissionRepository.Get(e => e.Subject.Id == roleId))
            {
                featurePermissionRepository.Delete(featurePermission);
            }

            var result = groupRepository.Delete(group);

            _guow.Commit();

            return Task.FromResult(result);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public Task<Group> FindByIdAsync(long roleId)
        {
            return Task.FromResult(GroupRepository.Get(roleId));
        }

        public Task<Group> FindByNameAsync(string roleName)
        {
            var groups = GroupRepository.Query(u => u.Name.ToLowerInvariant() == roleName.ToLowerInvariant()).ToList();

            if (!groups.Any())
                return Task.FromResult<Group>(null);

            if (groups.Count > 1)
                return Task.FromResult<Group>(null);

            return Task.FromResult(groups.Single());
        }

        public Task UpdateAsync(Group role)
        {
            if (role == null)
                return Task.CompletedTask;

            if (string.IsNullOrEmpty(role.Name))
                return Task.CompletedTask;

            if (FindByIdAsync(role.Id)?.Result == null)
                return Task.CompletedTask;

            _guow.GetRepository<Group>().Merge(role);
            var merged = _guow.GetRepository<Group>().Get(role.Id);
            _guow.GetRepository<Group>().Put(merged);
            _guow.Commit();

            return Task.CompletedTask;
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