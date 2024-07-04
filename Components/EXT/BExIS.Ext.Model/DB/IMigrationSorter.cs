using System.Collections.Generic;

namespace BExIS.Ext.Model.DB
{
    public interface IMigrationSorter
    {
        List<Migration> Sort(List<Migration> migrations); // IEmumerable?
    }
}