using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace BExIS.Security.Services.Authentication
{
    public sealed class SignInManager : SignInManager<User, long>
    {
        public SignInManager(UserManager userManager, IAuthenticationManager authenticationManager)
             : base(userManager, authenticationManager)
        {
        }
    }
}
