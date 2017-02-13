using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public sealed class EntityPermissionManager
    {
        public EntityPermissionManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            UserRepository = uow.GetReadOnlyRepository<User>();

        }

        #region Data Readers

        public IReadOnlyRepository<Entity> EntityRepository { get; private set; }
        public IReadOnlyRepository<User> UserRepository { get; private set; }

        #endregion
    }
}
