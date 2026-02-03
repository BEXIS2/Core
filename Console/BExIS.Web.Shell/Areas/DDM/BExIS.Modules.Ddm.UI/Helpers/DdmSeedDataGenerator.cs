using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Web.Mvc.Modularity;
using static System.Net.Mime.MediaTypeNames;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class DdmSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void GenerateSeedData()
        {
            var featureManager = new FeatureManager();
            var operationManager = new OperationManager();
            var featurePermissionManager = new FeaturePermissionManager();

            #region SECURITY

            //workflows = größere sachen, vielen operation
            //operations = einzelne actions

            //1.controller -> 1.Operation

            var dataDiscoveryFeature = featureManager.Find(f => f.Name == "Data Discovery" && f.Parent == null).SingleOrDefault() ?? featureManager.Create("Data Discovery", "Data Discovery");
            var searchFeature = featureManager.Find(f => f.Name == "Search" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Search", "Search", dataDiscoveryFeature);
            var searchManagementFeature = featureManager.Find(f => f.Name == "Search Managment" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Search Managment", "Search Managment", dataDiscoveryFeature);
            var dashboardFeature = featureManager.Find(f => f.Name == "Dashboard" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Dashboard", "Dashboard", dataDiscoveryFeature);
            var manageRequestsFeature = featureManager.Find(f => f.Name == "Requests Manage" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Requests Manage", "Manage Requests", dataDiscoveryFeature);
            var sendRequestsFeature = featureManager.Find(f => f.Name == "Requests Send" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Requests Send", "Send Requests", dataDiscoveryFeature);
            //worklfows -> create dataset ->
            //WorkflowManager workflowManager = new WorkflowManager();

            //var operation = new Operation();
            //Workflow workflow = new Workflow();

            //List<Workflow> workflows = workflowManager.WorkflowRepository.Find().ToList();

            #region Help Workflow

            //workflow =
            //    workflows.FirstOrDefault(w =>
            //    w.Name.Equals("Search Help") &&
            //    w.Feature != null &&
            //    w.Feature.Id.Equals(DataDiscovery.Id));

            //if (workflow == null) workflow = workflowManager.Create("Search Help", "", DataDiscovery);

            //operationManager.Create("DDM", "Help", "*", null, workflow);
            operationManager.Create("DDM", "Help", "*");

            #endregion Help Workflow

            #region Search Workflow

            // ToDo -> David, Sven
            // [Sven / 2017-08-21]
            // I had to remove the feature to get dashboard running without DDM feature permissions.
            // We have to think about how we can fix it in a long run. Maybe "DDM/Home" is not the proper
            // place for dashboard!?
            operationManager.Create("DDM", "PublicSearch", "*");
            operationManager.Create("DDM", "Home", "*", searchFeature);
            operationManager.Create("DDM", "Search", "*", searchFeature);
            operationManager.Create("DDM", "Data", "*");

            if (!featurePermissionManager.Exists(null, searchFeature.Id, PermissionType.Grant))
            {
                var result_create = featurePermissionManager.Create(null, searchFeature.Id, PermissionType.Grant);
            }

            #endregion Search Workflow

            #region Search Admin Workflow

            operationManager.Create("DDM", "Admin", "*", searchManagementFeature);

            #endregion Search Admin Workflow

            #region Dashboard

            operationManager.Create("DDM", "Dashboard", "*", dashboardFeature);

            #endregion Dashboard

            #region Requests

            operationManager.Create("DDM", "RequestsManage", "*", manageRequestsFeature);
            operationManager.Create("DDM", "RequestsSend", "*", sendRequestsFeature);

            #endregion Requests


            var SearchApiFeature = featureManager.Find(f => f.Name == "Search Api" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Search Api", "Search Api", dataDiscoveryFeature);
            if (!operationManager.Exists("Api", "SearchApi", "*")) operationManager.Create("Api", "SearchApi", "*", SearchApiFeature);

            var citationApiFeature = featureManager.Find(f => f.Name == "Citation Api" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Citation Api", "Citation Api", dataDiscoveryFeature);
            if (!operationManager.Exists("Api", "Citation", "*")) operationManager.Create("Api", "Citation", "*", citationApiFeature);


            var dataTableFeature = featureManager.Find(f => f.Name == "Api" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Api", "Api", dataDiscoveryFeature);
            if (!operationManager.Exists("Api", "DataTable", "*")) operationManager.Create("Api", "DataTable", "*", citationApiFeature);


            #region Tags

            var tagFeature = featureManager.Find(f => f.Name == "Tag" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Tag", "Tag", dataDiscoveryFeature);

            var tagViewFeature = featureManager.Find(f => f.Name == "View" && f.Parent.Id == tagFeature.Id).SingleOrDefault() ?? featureManager.Create("View", "View the tag(s).", tagFeature);
            var tagEditFeature = featureManager.Find(f => f.Name == "Edit" && f.Parent.Id == tagFeature.Id).SingleOrDefault() ?? featureManager.Create("Edit", "Edit the tag(s).", tagFeature);

            if (!operationManager.Exists("Api", "TagInfoView", "*")) operationManager.Create("Api", "TagInfoView", "*", tagViewFeature);
            if (!operationManager.Exists("Api", "TagInfoEdit", "*")) operationManager.Create("Api", "TagInfoEdit", "*", tagEditFeature);
            if (!operationManager.Exists("Api", "TagInfo", "*")) operationManager.Create("Api", "TagInfo", "*", tagEditFeature);

            #endregion Requests


            #region Curation

            var curationFeature = featureManager.Find(f => f.Name == "Curation" && f.Parent.Id == dataDiscoveryFeature.Id).SingleOrDefault() ?? featureManager.Create("Curation", "Curation", dataDiscoveryFeature);
            if (!operationManager.Exists("Ddm", "Curation", "*")) operationManager.Create("Ddm", "Curation", "*", curationFeature);

            #endregion


            #endregion SECURITY

            #region Mapping

            createCitationMappingConcept();

            #endregion
        }

        public void Dispose()
        {
        }

        private void createCitationMappingConcept()
        {
            using (var conceptManager = new ConceptManager())
            {

                /*
                APA,
                RIS,
                Text,
                Bibtex*/
                foreach (CitationFormat value in Enum.GetValues(typeof(CitationFormat)))
                {
                    var keys = new List<MappingKey>();
                    // check if concept exist
                    var concept = conceptManager.MappingConceptRepository.Query(c => c.Name.Equals("Citation_" + value)).FirstOrDefault();
                    if (concept == null) //if not create
                        concept = conceptManager.CreateMappingConcept("Citation_"+ value, "The concept is needed to create a " + value + " citation string", "", "");
                    else // if exist load available keys
                    {
                        keys = conceptManager.MappingKeyRepo.Query(k => k.Concept.Id.Equals(concept.Id)).ToList();
                    }

                    //title
                    if (!keys.Any(k => k.Name.Equals("data/title")))
                        conceptManager.CreateMappingKey("Title", "", "", false, false, "data/title", concept);
                    //version
                    if (!keys.Any(k => k.Name.Equals("data/version")))
                        conceptManager.CreateMappingKey("Version", "", "", true, false, "data/version", concept);
                    //year
                    if (!keys.Any(k => k.Name.Equals("data/year")))
                        conceptManager.CreateMappingKey("Year", "", "", true, false, "data/year", concept);

                    //entityType
                    if (!keys.Any(k => k.Name.Equals("data/entityType")))
                        conceptManager.CreateMappingKey("EntityType", "", "", true, false, "data/entityType", concept);

                    //entryType
                    if (value == CitationFormat.Text)
                    {
                        if (!keys.Any(k => k.Name.Equals("data/entryType")))
                            conceptManager.CreateMappingKey("EntryType", "", "", true, false, "data/entryType", concept);
                    }
                    else
                    {
                        if (!keys.Any(k => k.Name.Equals("data/entryType")))
                            conceptManager.CreateMappingKey("EntryType", "", "", false, false, "data/entryType", concept);
                    }

                    //publisher
                    if (!keys.Any(k => k.Name.Equals("data/publisher")))
                        conceptManager.CreateMappingKey("Publisher", "", "", false, false, "data/publisher", concept);

                    if (value == CitationFormat.Bibtex)
                    {
                        if (!keys.Any(k => k.Name.Equals("data/keyword")))
                            conceptManager.CreateMappingKey("Keyword", "", "", false, false, "data/keyword", concept);
                    }

                    //note
                    if (!keys.Any(k => k.Name.Equals("data/note")))
                        conceptManager.CreateMappingKey("Note", "", "", true, false, "data/note", concept);

                    //doi
                    if (!keys.Any(k => k.Name.Equals("data/doi")))
                        conceptManager.CreateMappingKey("DOI", "", "", true, false, "data/doi", concept);

                    //projects
                    MappingKey projects = null;
                    if (!keys.Any(k => k.Name.Equals("data/projects")))
                        projects = conceptManager.CreateMappingKey("Projects", "", "", true, true, "data/projects", concept);

                    if (!keys.Any(k => k.Name.Equals("data/projects/project")))
                        conceptManager.CreateMappingKey("Project", "", "", true, false, "data/projects/project", concept, projects);

                    //authors
                    MappingKey authors = null;
                    if (!keys.Any(k => k.Name.Equals("data/authorNames")))
                        authors = conceptManager.CreateMappingKey("AuthorNames", "", "", false, true, "data/authorNames", concept);

                    if (!keys.Any(k => k.Name.Equals("data/authorNames/authorname")))
                        conceptManager.CreateMappingKey("AuthorName", "", "", false, false, "data/authorNames/authorName", concept, authors);
                }
            }
        }
    }
}