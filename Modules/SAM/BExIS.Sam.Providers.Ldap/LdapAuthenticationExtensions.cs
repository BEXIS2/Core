using Owin;

namespace BExIS.Sam.Providers.Ldap
{
    public static class LdapAuthenticationExtensions
    {
        public static IAppBuilder UseLdapAuthentication(this IAppBuilder app, LdapAuthenticationOptions options)
        {
            return app.Use(typeof(LdapAuthenticationMiddleware), app, options);
        }
    }
}