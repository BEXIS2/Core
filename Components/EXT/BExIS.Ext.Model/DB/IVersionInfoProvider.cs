using System;

namespace BExIS.Ext.Model.DB
{
    public interface IVersionInfoProvider
    {
        Version GetLatestVersion(String moduleCode);

        bool UpdateToVersion(String moduleCode, Version version);

        bool RemoveLatestVersion(String moduleCode);

        bool RemoveModule(String moduleCode);
    }
}