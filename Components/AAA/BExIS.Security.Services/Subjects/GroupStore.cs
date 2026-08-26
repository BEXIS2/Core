using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Versions;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class GroupStore : IQueryableRoleStore<Group, long>, IDisposable
    {
        public IQueryable<Group> Roles
        {
            get
            {
                using (var uow = this.GetUnitOfWork())
                {
                    return uow.GetReadOnlyRepository<Group>().Query().ToList().AsQueryable();
                }
            }
        }

        public Task CreateAsync(Group role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();

                groupRepository.Put(role);
                uow.Commit();

                return Task.CompletedTask;
            }
        }

        public Task DeleteAsync(Group role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                uow.GetRepository<Group>().Delete(role);
                uow.Commit();

                return Task.CompletedTask;
            }
        }

        public Task UpdateAsync(Group role)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();

                groupRepository.Merge(role);
                uow.Commit();

                return Task.CompletedTask;
            }

        }

        public Task<Group> FindByIdAsync(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return Task.FromResult(uow.GetReadOnlyRepository<Group>().Get(id));
            }
        }

        public Task<Group> FindByNameAsync(string name)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return Task.FromResult(
                    uow.GetReadOnlyRepository<Group>()
                       .Query(g => g.Name == name).SingleOrDefault()
                );
            }
        }

        public void Dispose()
        {

        }
    }
}
