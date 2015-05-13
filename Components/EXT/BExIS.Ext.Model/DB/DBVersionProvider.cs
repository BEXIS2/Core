using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ext.Model.DB
{
    public class DBVersionProvider: IVersionInfoProvider
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
