using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Providers.Authentication
{
    public sealed class BuiltInAuthenticationProvider : IAuthenticationProvider
    {
        public BuiltInAuthenticationProvider(string connectionString)
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.UsersRepo = uow.GetReadOnlyRepository<User>();
        }

        public IReadOnlyRepository<User> UsersRepo { get; private set; }   

        public bool ValidateUser(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
