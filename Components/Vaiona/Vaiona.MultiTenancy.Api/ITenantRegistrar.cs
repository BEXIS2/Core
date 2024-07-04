namespace Vaiona.MultiTenancy.Api
{
    public interface ITenantRegistrar
    {
        /// <summary>
        /// The tenant package is received as a zip file containing all the required information.
        /// unzip the package ito the tenants' folder, register the tenant with the catalog, and sync the changes by updating the ctalog.
        /// The newly registered tenant is non-default and inactive. These attributes may get changed by calling other methods upon user request.
        /// </summary>
        /// <param name="tenantId">The tenants identifier</param>
        /// <param name="string tenantZipPackagePath">The full path to the tenant package in zipped format. The tenant's file name is excluded.</param>
        /// <remarks>The zip tenant package should be assumed temprary, as the register method may delete the zip after installing it.</remarks>
        /// <param name="deleteSource">determines wheather the the source package should be removed.</param>
        void Register(string tenantId, string tenantZipPackagePath, bool deleteSource = true);

        void Unregister(string id);

        void Activate(string id);

        void Inactivate(string id);

        void MakeDefault(string id);

        //List<Tenant> GetAll();
    }
}