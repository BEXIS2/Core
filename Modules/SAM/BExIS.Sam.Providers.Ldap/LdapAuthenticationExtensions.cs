using Owin;
using System;

namespace BExIS.Sam.Providers.Ldap
{
    public static class LdapAuthenticationExtensions
    {
        public static IAppBuilder UseLdapAuthentication(this IAppBuilder app, LdapAuthenticationOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            app.Use(typeof(LdapAuthenticationMiddleware), app, options);

            return app;
        }
    }
}