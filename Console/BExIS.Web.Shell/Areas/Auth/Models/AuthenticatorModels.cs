using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authentication;

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
                Name = authenticator.Name
            };
        }
    }

    public class AuthenticatorSelectListModel
    {
        public long Id { get; set; }

        public List<AuthenticatorSelectListItemModel> AuthenticatorList { get; set; }

        public AuthenticatorSelectListModel(bool internalOnly = false)
        {
            AuthenticatorManager authenticatorManager = new AuthenticatorManager();

            if (internalOnly)
            {
                AuthenticatorList = authenticatorManager.GetInternalAuthenticators().Select(a => AuthenticatorSelectListItemModel.Convert(a)).ToList<AuthenticatorSelectListItemModel>();
            }
            else
            {
                AuthenticatorList = authenticatorManager.GetAllAuthenticators().Select(a => AuthenticatorSelectListItemModel.Convert(a)).ToList<AuthenticatorSelectListItemModel>();
            } 
        }
    }
}