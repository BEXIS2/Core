using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vaiona.Web.Mvc.Modularity
{
    public interface IModuleSeedDataGenerator : IDisposable
    {
        void GenerateSeedData();
    }
}
