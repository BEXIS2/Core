namespace Vaiona.Web.Mvc.Modularity
{
    public static class ModuleBootstrapper
    {
        static ModuleBootstrapper()
        {
        }

        /// <summary>
        /// register the embedded views of the plugins by registering thier containing assemblies
        /// </summary>
        public static void Initialize()
        {
            foreach (var plugin in ModuleManager.ModuleInfos)
            {
                //BoC.Web.Mvc.PrecompiledViews.ApplicationPartRegistry.Register(plugin.EntryType.Assembly);
            }
        }
    }
}