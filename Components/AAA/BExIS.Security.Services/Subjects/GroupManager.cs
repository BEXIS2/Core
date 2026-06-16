using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.FormerMember;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class GroupManager : RoleManager<Group, long>
    {
         public GroupManager(IRoleStore<Group, long> store) : base(store)
        {
        }

        public async Task<IdentityResult> CreateGroupAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return IdentityResult.Failed("Group name is required.");

            if (await RoleExistsAsync(name))
                return IdentityResult.Failed("Group already exists.");

            var group = new Group { Name = name };
            return await CreateAsync(group);
        }

        public Task<Group> GetByIdAsync(long id)
                => FindByIdAsync(id);

        public Task<Group> GetByNameAsync(string name)
            => FindByNameAsync(name);

        public Task<bool> GroupExistsAsync(string name)
            => RoleExistsAsync(name);

        public async Task<IdentityResult> RenameGroupAsync(long id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return IdentityResult.Failed("Name required.");

            var group = await FindByIdAsync(id);
            if (group == null)
                return IdentityResult.Failed("Group not found.");

            group.Name = newName;
            return await UpdateAsync(group);
        }

        public async Task<IdentityResult> DeleteGroupAsync(long id)
        {
            var group = await FindByIdAsync(id);
            if (group == null)
                return IdentityResult.Failed("Group not found.");

            return await DeleteAsync(group);
        }

        public Group Create(Group group)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                groupRepository.Put(group);
                uow.Commit();
                return group;
            }
        }

        public bool Delete(Group group)
        {
            if (group == null)
                return false;

            return Delete(group.Id);
        }

        public bool Delete(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                var group = groupRepository.Get(id);
                if (group != null)
                {
                    groupRepository.Delete(group);
                    uow.Commit();
                    return true;
                }
                return false;
            }
        }

        public int Count(Expression<Func<Group, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                return groupRepository.Query(predicate).Count();
            }
        }

        public int Count()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                return groupRepository.Query().Count();
            }
        }

        public Group Get(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();

                var group = groupRepository.Get(id);
                return group;
            }
        }

        public Group Get(string groupName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();

                return groupRepository.Query(r => r.Name == groupName).SingleOrDefault();
            }
        }

        public IList<Group> Get()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();

                var groups = groupRepository.Get();
                return groups;
            }
        }

        public IList<Group> Find(Expression<Func<Group, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                var groups = groupRepository.Query(predicate).ToList();
                return groups;
            }
        }

        public IList<Group> Find()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                var groups = groupRepository.Query().ToList();
                return groups;
            }
        }

        public IList<Group> Find(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            var orderbyClause = orderBy?.ToLINQ();
            var whereClause = filter?.ToLINQ();
            count = 0;
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var groupRepository = uow.GetReadOnlyRepository<Group>();
                    IQueryable<Group> groups = groupRepository.Query();
                    if (whereClause != null && orderBy != null)
                    {
                        var l = groups.Where(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = groups.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    if (orderBy != null)
                    {
                        count = groups.Count();
                        return groups.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    count = count = groups.Count();

                    // without filter and order
                    return groups.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered groups."), ex);
            }
        }
    }
}