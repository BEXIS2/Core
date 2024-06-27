using System;

namespace BExIS.Ext.Model.DB
{
    public class DBVersionProvider : IVersionInfoProvider
    {
        public Version GetLatestVersion(string moduleCode)
        {
            throw new NotImplementedException();
        }

        public bool UpdateToVersion(String moduleCode, Version version)
        {
            throw new NotImplementedException();
        }

        public bool RemoveLatestVersion(string moduleCode)
        {
            throw new NotImplementedException();
        }

        public bool RemoveModule(string moduleCode)
        {
            throw new NotImplementedException();
        }
    }
}