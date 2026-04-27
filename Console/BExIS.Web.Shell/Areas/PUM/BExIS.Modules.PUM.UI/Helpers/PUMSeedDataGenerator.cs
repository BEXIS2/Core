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


            

            // Operations
            using (OperationManager operationManager = new OperationManager())
            {
                var homeController = operationManager.Find("PUM", "Home", "*") ?? operationManager.Create("PUM", "Home", "*");
            }

            using (OperationManager operationManager = new OperationManager())
            {
                var importJSONController = operationManager.Find("PUM", "ImportJSON", "*") ?? operationManager.Create("PUM", "ImportJSON", "*");
            }

            using (OperationManager operationManager = new OperationManager())
            {
                var importCSVController = operationManager.Find("PUM", "ImportCSV", "*") ?? operationManager.Create("PUM", "ImportCSV", "*");
            }

            using (OperationManager operationManager = new OperationManager())
            {
                var viewController = operationManager.Find("PUM", "View", "*") ?? operationManager.Create("PUM", "View", "*");
            }


        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}