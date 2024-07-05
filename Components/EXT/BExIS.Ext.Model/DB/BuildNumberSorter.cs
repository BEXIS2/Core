using System;
using System.Collections.Generic;

namespace BExIS.Ext.Model.DB
{
    /// <summary>
    /// Sorts the migrations bases on the build number of the version of their MigrationDescriptors.
    /// All the migrations are of a same version
    /// </summary>
    public class BuildNumberSorter : IMigrationSorter
    {
        public List<Migration> Sort(List<Migration> migrations)
        {
            throw new NotImplementedException();
        }
    }
}