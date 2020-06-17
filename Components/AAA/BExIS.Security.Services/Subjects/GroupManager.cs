﻿using BExIS.Security.Entities.Subjects;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
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
                return Task.FromResult(0);

            if (string.IsNullOrEmpty(role.Name))
                return Task.FromResult(0);

            if (FindByNameAsync(role.Name)?.Result != null)
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
            roleName = roleName.Trim();

            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                return Task.FromResult(groupRepository.Query().FirstOrDefault(u => u.Name.ToUpperInvariant() == roleName.ToUpperInvariant()));
            }
        }

        public Task UpdateAsync(Group role)
        {
            if (role == null)
                return Task.FromResult(0);

            if (string.IsNullOrEmpty(role.Name))
                return Task.FromResult(0);

            if (FindByIdAsync(role.Id)?.Result == null)
                return Task.FromResult(0);

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
