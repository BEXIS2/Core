using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Security;
using BExIS.Security.Services.Security;

namespace BExIS.Web.Shell.Areas.Auth.Models
{

    public class AuthenticatorCreateModel
    {

    }

    public class AuthenticatorUpdateModel
    {

    }

    public class AuthenticatorReadModel
    {

    }

    public class AuthenticatorDeleteModel
    {

    }

    public class AuthenticatorGridRowModel
    {

    }

    public class AuthenticatorSelectListItemModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public static AuthenticatorSelectListItemModel Convert(Authenticator authenticator)
        {
            return new AuthenticatorSelectListItemModel()
            {
                Id = authenticator.Id,
                Name = authenticator.Alias
            };
        }
    }

    public class AuthenticatorSelectListModel
    {
        public long Id { get; set; }

        public List<AuthenticatorSelectListItemModel> AuthenticatorList { get; set; }

        public AuthenticatorSelectListModel()
        {
            AuthenticatorManager authenticatorManager = new AuthenticatorManager();
            AuthenticatorList = authenticatorManager.GetAllAuthenticators().Select(a => AuthenticatorSelectListItemModel.Convert(a)).ToList<AuthenticatorSelectListItemModel>();
        }
    }
}