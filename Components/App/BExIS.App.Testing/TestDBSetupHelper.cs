using System;
using System.IO;
using Vaiona.IoC;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.App.Testing
{
    public class TestDBSetupHelper : IDisposable
    {
        private IPersistenceManager pManager = null;

        public TestDBSetupHelper(bool configureModules = false)
        {
            initIoC();

            pManager = PersistenceFactory.GetPersistenceManager(); // just to prepare data access environment
            pManager.Configure(AppConfiguration.DefaultApplicationConnection.ConnectionString,
                AppConfiguration.DatabaseDialect, "Default", AppConfiguration.ShowQueries, configureModules);

            Console.WriteLine("db connection");
            Console.WriteLine(AppConfiguration.DefaultApplicationConnection.ConnectionString);
        }

        public void Start()
        {
            pManager.Start();
        }

        public void Dispose()
        {
            pManager.Shutdown();
        }

        private void initIoC()
        {
            string path = Path.Combine(AppConfiguration.AppRoot, "IoC.config");

            //string sFilePath = Path.GetFullPath(path);

            if (!File.Exists(path))
                throw new FileNotFoundException($"IoC.config file expected but was not found in '{path}'.");
            try
            {
                IoCFactory.StartContainer(path, "DefaultContainer");
            }
            catch (System.TypeLoadException) { } // swallow this exception, as it means that the IoC is already running.
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}