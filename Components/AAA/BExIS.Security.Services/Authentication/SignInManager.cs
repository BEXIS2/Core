using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace BExIS.Security.Services.Authentication
{
    public sealed class SignInManager : SignInManager<User, long>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public SignInManager(IAuthenticationManager authenticationManager, UserManager userManager)
             : base(new IdentityUserService(userManager), authenticationManager)
        {
        }
    }
}