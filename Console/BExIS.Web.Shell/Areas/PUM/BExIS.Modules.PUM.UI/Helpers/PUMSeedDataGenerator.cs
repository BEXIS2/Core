using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Pum.UI.Helpers
{
    public class PUMSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            var operationManager = new OperationManager();
            // Operations
            var homeController = operationManager.Get("PUM", "Home", "*") ?? operationManager.Create("PUM", "Home", "*");
            var importJSONController = operationManager.Get("PUM", "ImportJSON", "*") ?? operationManager.Create("PUM", "ImportJSON", "*");
            var importCSVController = operationManager.Get("PUM", "ImportCSV", "*") ?? operationManager.Create("PUM", "ImportCSV", "*");
            var viewController = operationManager.Get("PUM", "View", "*") ?? operationManager.Create("PUM", "View", "*");
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}