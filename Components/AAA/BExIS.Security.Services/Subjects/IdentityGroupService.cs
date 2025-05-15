using BExIS.Security.Entities.Subjects;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Subjects
{
    public class IdentityGroupService : RoleManager<Group, long>
    {
        private readonly GroupManager _groupManager;
        private bool _disposed;

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public IdentityGroupService(GroupManager groupManager) : base(groupManager)
        {
            _groupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));

            RoleValidator = new RoleValidator<Group, long>(this)
            {
            };
        }

        public Task<bool> DeleteByIdAsync(long roleId)
        {
            return _groupManager.DeleteByIdAsync(roleId);
        }

        public List<Group> GetGroups(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            return _groupManager.GetGroups(filter, orderBy, pageNumber, pageSize, out count);
        }
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _groupManager?.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}