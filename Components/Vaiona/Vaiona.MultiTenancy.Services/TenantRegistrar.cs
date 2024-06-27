using System;
using System.Linq;
using Vaiona.Model.MTnt;
using Vaiona.MultiTenancy.Api;

namespace Vaiona.MultiTenancy.Services
{
    public class TenantRegistrar : ITenantRegistrar
    {
        private XmlTenantStore store = new XmlTenantStore(null); // think about the path provider
        //public List<Tenant> GetAll()
        //{
        //    List<Tenant> tenants = store.List();
        //    tenants.ForEach(p => tenants.Add(store.Load(p.Id)));
        //    return tenants;
        //}

        public void Activate(string id)
        {
            Tenant tenant = null;
            try
            {
                tenant = store.Tenants.Where(p => id.Equals(p.Id, StringComparison.InvariantCultureIgnoreCase)).Single();
                tenant.Status = TenantStatus.Active;
            }
            catch
            {
                throw new Exception(string.Format("Tenant '{0}' was not found. Operation aborted.", id));
            }
            store.UpdateStatus(tenant);
        }

        public void Inactivate(string id)
        {
            Tenant tenant = null;
            try
            {
                tenant = store.Tenants.Where(p => id.Equals(p.Id, StringComparison.InvariantCultureIgnoreCase)).Single();
                tenant.Status = TenantStatus.Inactive;
            }
            catch
            {
                throw new Exception(string.Format("Tenant '{0}' was not found. Operation aborted.", id));
            }
            store.UpdateStatus(tenant);
        }

        public void MakeDefault(string id)
        {
            Tenant tenant = null;
            try
            {
                // It is not reaaly needed to set the default here, but it is done so that if a request arrives during the update
                // it is served with the latest information
                store.Tenants.ForEach(p => p.IsDefault = false);
                tenant = store.Tenants.Where(p => id.Equals(p.Id, StringComparison.InvariantCultureIgnoreCase)).Single();
                tenant.IsDefault = true;
            }
            catch
            {
                throw new Exception(string.Format("Tenant '{0}' was not found. Operation aborted.", id));
            }
            store.MakeDefault(tenant);
        }

        public void Register(string tenatId, string tenantZipPackagePath, bool deleteSource = true)
        {
            try
            {
                store.Create(tenatId, tenantZipPackagePath, deleteSource);
            }
            catch (Exception ex)
            { // more detailed exception handeling
            }
        }

        public void Unregister(string id)
        {
            Tenant tenant;
            try
            {
                tenant = store.Tenants.Where(p => id.Equals(p.Id, StringComparison.InvariantCultureIgnoreCase)).Single();
            }
            catch
            {
                throw new Exception(string.Format("Tenant '{0}' was not found! Operation aborted.", id));
            }

            if (store.Tenants.Count() <= 1)
                throw new Exception(string.Format("Tenant '{0}' could not be unregistered. It is the only registered tenent.", id));

            // It is not allowed to delete the DEFAULT tenant. If needed, another tenant must be set as the default, before deleting the chosen one.
            if (tenant.IsDefault == true)
                throw new Exception(string.Format("Tenant '{0}' could not be unregistered. It is the default tenent.", id));

            // It is not allowed to delete an ACTIVE tenant. If needed, the chosen tenant must be inctivated first.
            if (tenant.Status == TenantStatus.Active)
                throw new Exception(string.Format("Tenant '{0}' could not be unregistered. It is active.", id));

            // There MUST at least one active tenant remaining registered after deleting this one
            if (store.Tenants.Where(p => p.Status == TenantStatus.Active).Count() <= 0)
                throw new Exception(string.Format("Tenant '{0}' could not be unregistered. There would not be another active tenant after unregistering '{0}'.", id));

            try
            {
                store.Remove(tenant);
            }
            catch
            {
                throw new Exception(string.Format("Tenant '{0}' was not unresigtered! Operation aborted.", id));
            }
        }
    }
}