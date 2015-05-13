using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ext.Model.DB
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class MigrationDescriptorAttribute: Attribute
    {
        public MigrationDescriptorAttribute(String module, Version version, Version previous, String upgradeMessage, String downgradeMessage)
        {

        }
    }
}
