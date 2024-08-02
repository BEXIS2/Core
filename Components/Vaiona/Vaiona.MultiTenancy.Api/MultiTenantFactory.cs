using System;
using Vaiona.IoC;

namespace Vaiona.MultiTenancy.Api
{
    public static class MultiTenantFactory
    {
        public static ITenantRegistrar GetTenantRegistrar()
        {
            ITenantRegistrar registrar = null;
            try
            {
                registrar = IoCFactory.Container.Resolve<ITenantRegistrar>() as ITenantRegistrar;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load tenant registration service", ex);
            }
            return (registrar);
        }

        public static ITenantResolver GetTenantResolver()
        {
            ITenantResolver resolver = null;
            try
            {
                resolver = IoCFactory.Container.Resolve<ITenantResolver>() as ITenantResolver;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load tenant resolution service", ex);
            }
            return (resolver);
        }
    }
}