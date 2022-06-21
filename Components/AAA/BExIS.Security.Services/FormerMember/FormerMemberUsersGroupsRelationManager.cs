using BExIS.Security.Entities.FormerMember;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.FormerMember
{
    public class FormerMemberUsersGroupsRelationManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public IReadOnlyRepository<UsersGroupsRelationFormerMember> FormerMemberUsersGroupsRelationRepository { get; }

        public IQueryable<UsersGroupsRelationFormerMember> FormerMemberFeaturePermissions => FormerMemberUsersGroupsRelationRepository.Query();

        public FormerMemberUsersGroupsRelationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            FormerMemberUsersGroupsRelationRepository = _guow.GetReadOnlyRepository<UsersGroupsRelationFormerMember>();
        }

        ~FormerMemberUsersGroupsRelationManager()
        {
            Dispose(true);
        }


        public void Create(long userRefId, long groupRefId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                
                var formerMemberUsersGroupsRelation = new UsersGroupsRelationFormerMember()
                {
                    UserRef = userRefId,
                    GroupRef = groupRefId
                };

                var formerMemberUsersGroupsRelationRepository = uow.GetRepository<UsersGroupsRelationFormerMember>();
                formerMemberUsersGroupsRelationRepository.Put(formerMemberUsersGroupsRelation);
                uow.Commit();
            }
        }

        public void Delete(UsersGroupsRelationFormerMember formerMemberUsersGroupsRelation)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberUsersGroupsRelationRepository = uow.GetRepository<UsersGroupsRelationFormerMember>();
                formerMemberUsersGroupsRelationRepository.Delete(formerMemberUsersGroupsRelation);
                uow.Commit();
            }
        }


        public void Dispose()
        {
            Dispose(true);
        }


        protected void Dispose(bool disposing)
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
