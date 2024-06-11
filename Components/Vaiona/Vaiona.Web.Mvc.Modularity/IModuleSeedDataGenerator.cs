using System;

namespace Vaiona.Web.Mvc.Modularity
{
    public interface IModuleSeedDataGenerator : IDisposable
    {
        void GenerateSeedData();
    }
}