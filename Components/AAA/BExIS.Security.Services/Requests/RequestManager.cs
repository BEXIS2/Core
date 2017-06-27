using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using System;
using Vaiona.Entities.Common;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public sealed class RequestManager
    {
        public RequestManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            UserRepository = uow.GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<Entity> EntityRepository { get; private set; }
        public IReadOnlyRepository<Request> RequestRepository { get; private set; }
        public IReadOnlyRepository<User> UserRepository { get; private set; }

        #endregion Data Readers

        public Request CreateRequest(User applicant, BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRequest(long id)
        {
            throw new NotImplementedException();
        }
    }
}