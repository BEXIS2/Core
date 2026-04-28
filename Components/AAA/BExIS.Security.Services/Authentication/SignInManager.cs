using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Org.BouncyCastle.Crypto;
using Vaiona.IoC;

namespace BExIS.Security.Services.Authentication
{
    public class SignInManager : SignInManager<User, long>
    {
        // Konstruktor bleibt gleich
        public SignInManager(
            UserManager userManager,
            IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        { }

        public static SignInManager Create(IdentityFactoryOptions<SignInManager> options, IOwinContext context)
        {
            var userManager = IoCFactory.Container.Resolve<UserManager>();   // ← aus Unity!
            var authManager = context.Authentication;
            return new SignInManager(userManager, authManager);
        }
    }
}