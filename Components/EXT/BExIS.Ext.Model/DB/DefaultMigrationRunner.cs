using System;
using System.Collections.Generic;

namespace BExIS.Ext.Model.DB
{
    /// <summary>
    /// Runs the migrations against the target database, there should be other runners to i.e., generate scripts, etc
    /// </summary>
    public class DefaultMigrationRunner : IMigrationRunner
    {
        public DefaultMigrationRunner(object config) // the config can be a connection string, an ORM session, etc
        {
        }

        public bool Install(string moduleCode, Version version, List<System.Reflection.Assembly> migrationContainers)
        {
            // compute the diff
            // sort
            // run
            // update version provider
            // report
            IVersionInfoProvider versionProvider = new DBVersionProvider();
            Version v1 = versionProvider.GetLatestVersion(moduleCode);
            List<Migration> allMigrations = extractMigrations(migrationContainers);
            List<Migration> effectiveMigrations = diff(v1, version, allMigrations);
            IMigrationSorter sorter = new BuildNumberSorter();
            effectiveMigrations = sorter.Sort(effectiveMigrations);
            // ...
            throw new NotImplementedException();
        }

        public bool Uninstall(string moduleCode, Version version, List<System.Reflection.Assembly> migrationContainers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// return migrations that their version is greater than v1 and less than or equal to v2
        /// </summary>
        /// <param name="v1">is assumed less than or equal to v2.</param>
        /// <param name="v2"></param>
        /// <param name="migrations"></param>
        /// <returns></returns>
        private List<Migration> diff(Version v1, Version v2, List<Migration> migrations)
        {
            return migrations;
        }

        private List<Migration> extractMigrations(List<System.Reflection.Assembly> migrationContainers)
        {
            throw new NotImplementedException();
        }
    }
}