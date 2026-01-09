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

namespace BExIS.Modules.Smm.UI.Helpers
{
    public class SMMSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            // Operations
            using (OperationManager operationManager = new OperationManager())
            {
                var homeController = operationManager.Find("SMM", "Home", "*") ?? operationManager.Create("SMM", "Home", "*");
            }


        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}