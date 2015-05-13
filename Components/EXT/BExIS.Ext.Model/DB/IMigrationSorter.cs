using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ext.Model.DB
{
    public interface IMigrationSorter
    {
        List<Migration> Sort(List<Migration> migrations); // IEmumerable?
    }
}
