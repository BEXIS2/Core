using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ext.Model.DB
{
    public interface IMigrationRunner
    {
        bool Install(String moduleCode, Version version, List<Assembly> migrationContainers);
        bool Uninstall(String moduleCode, Version version, List<Assembly> migrationContainers);
    }
}
