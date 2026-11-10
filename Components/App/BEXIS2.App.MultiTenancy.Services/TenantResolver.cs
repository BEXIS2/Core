using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BEXIS2.App.Model.MTnt;
using BEXIS2.App.MultiTenancy.Api;
using BEXIS2.Utils.Cfg;
using Microsoft.AspNetCore.Http;

namespace BEXIS2.App.MultiTenancy.Services
{
    public class TenantResolver : ITenantResolver
    {
        private XmlTenantStore store = null;

        public void Load(ITenantPathProvider pathProvider)
        {
            store = new XmlTenantStore(pathProvider);
            store.Load();
        }

        public List<Tenant> Manifest
        { get { return store.Tenants; } }

        public Tenant DefaultTenant
        {
            get
            {
                if (store.Tenants.Any(p => p.IsDefault))
                    return store.Tenants.Where(p => p.IsDefault).SingleOrDefault();
                else if (store.Tenants != null)
                    return store.Tenants.FirstOrDefault();
                return null;
            }
        }

        public Tenant Resolve(Uri request)
        {
            throw new NotImplementedException();
        }

        public Tenant Resolve(string id)
        {
            throw new NotImplementedException();
        }

        public Tenant Resolve()
        {
            List<Tenant> resolved = null;
            string tenantId = AppConfiguration.TenantId;
            // If a tenant is registered in the web.config, it takes precendece!
            // If no entry is there or has no value, the matching rules of tenants manifest files will be used
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                resolved = store.Tenants
                    .Where(p => p.Status == TenantStatus.Active && p.Id.Equals(tenantId, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }
            
            // To avoid failing on overlapping matching rules, in case of more than one match, the first is returned.
            // No guarantee the first in the matched list, is the first in the manifest order! but it is still the same tenant
            if (resolved.Count >= 1)
            {
                return resolved.First();
            }
            else // return the default tenant if no match found, or if the matched ones are nor active
            {
                if (store.Tenants.Where(p => p.IsDefault && p.Status == TenantStatus.Active).Any())
                    return store.Tenants.Where(p => p.IsDefault && p.Status == TenantStatus.Active).First();
            }
            return null; // no active match, nor active default!
        }
    }
}