using System;

namespace BExIS.Ext.Model.DB
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MigrationDescriptorAttribute : Attribute
    {
        public MigrationDescriptorAttribute(String module, Version version, Version previous, String upgradeMessage, String downgradeMessage)
        {
        }
    }
}