using System;
using System.Collections.Generic;
using System.Web;
using Vaiona.Model.MTnt;

namespace Vaiona.MultiTenancy.Api
{
    public interface ITenantResolver
    {
        /// <summary>
        /// Loads the root manifest of the tenants, the concrete tenant data may be loaded by further calls
        /// The loaded data is cached. cache may get invalidated/updated by Registration service
        /// A filesystem watchdog mechanism would be helpful
        /// </summary>
        void Load(ITenantPathProvider pathProvider);

        /// <summary>
        /// The loaded list of tenants' root manifests
        /// </summary>
        List<Tenant> Manifest { get; }

        Tenant DefaultTenant { get; }

        /// <summary>
        /// Resolves the tenant using the incoming http request and the tenants' matching rules.
        /// If no match found, the default one is returned, provided that it is active.
        /// If no default is set, an exception is thrown
        /// </summary>
        /// <param name="request">http request that contains url and port number to be used for matching</param>
        /// <returns></returns>
        Tenant Resolve(HttpRequest request);

        /// <summary>
        /// Resolves the tanent using its identifier
        /// </summary>
        /// <param name="id">tenant's ID</param>
        /// <returns></returns>
        Tenant Resolve(string id);

        /// <summary>
        /// Resolves the tenant using the incoming URI and the tenants' matching rules
        /// </summary>
        /// <param name="request">The URI should contain the url/IP address and the port number at minimum.</param>
        /// <returns></returns>
        Tenant Resolve(Uri request);
    }
}