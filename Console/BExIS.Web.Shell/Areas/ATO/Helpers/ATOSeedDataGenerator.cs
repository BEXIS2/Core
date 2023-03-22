using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Ato.UI.Helpers
{
    public class ATOSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            createSecuritySeedData();
        }
        private void createSecuritySeedData()
        {
            // Javad:
            // 1) all the create operations should check for existence of the record
            // 2) failure on creating any record should rollback the whole seed data generation. It is one transaction.
            // 3) failues should throw an exception with enough information to pin point the root cause
            // 4) only seed data related to the functions of this modules should be genereated here.
            // BUG: seed data creation is not working because of the changes that were done in the entities and services.
            // TODO: reimplement the seed data creation method.

            //#region Security

            //// Tasks
            var operationManager = new OperationManager();
            var featureManager = new FeatureManager();

            Feature ATOFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("ATO"));
            if (ATOFeature == null) ATOFeature = featureManager.Create("ATO", "ATO");

            Feature DocumentationFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Documents"));
            if (DocumentationFeature == null) DocumentationFeature = featureManager.Create("Documents", "Documents", ATOFeature);

            Feature OrganigramFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Organigram"));
            if (OrganigramFeature == null) OrganigramFeature = featureManager.Create("Organigram", "Organigram");

            var ATOOperation = operationManager.Create("ATO", "Home", "*", DocumentationFeature);
            operationManager.Create("ATO", "Organigram", "*", OrganigramFeature);

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

    }

}
        