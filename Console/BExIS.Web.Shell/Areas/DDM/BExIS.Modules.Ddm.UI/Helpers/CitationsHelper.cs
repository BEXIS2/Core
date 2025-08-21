using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Versions;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Telerik.Web.Mvc.Infrastructure;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class CitationsHelper
    {
        public static CitationDataModel GetCitationDataModel(long datasetVersionId)
        {
            try
            {
                using (var datasetManager = new DatasetManager())
                using (var conceptManager = new ConceptManager())
                using (var entityPermissionManager = new EntityPermissionManager())
                using (var entityManager = new EntityManager())
                {
                    var datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                    var metadata = datasetVersion.Metadata;

                    var concept = conceptManager.MappingConceptRepository.Query(c => c.Name.ToLower() == "citation").FirstOrDefault();

                    if (concept == null)
                        return null;

                    var conceptOutput = MappingUtils.GetConceptOutput(datasetVersion.Dataset.MetadataStructure.Id, concept.Id, metadata);

                    var model = new CitationDataModel();
                    XmlSerializer serializer = new XmlSerializer(typeof(CitationDataModel), new XmlRootAttribute("data"));
                    using (XmlReader reader = new XmlNodeReader(conceptOutput))
                    {
                        model = (CitationDataModel)serializer.Deserialize(reader);
                    }

                    if(model == null)
                        return null;    

                    var settingsHelper = new SettingsHelper();

                    var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                    var useTags = Convert.ToBoolean(moduleSettings.GetValueByKey("use_tags"));
                    var citationSettings = settingsHelper.GetCitationSettings();

                    // Authors
                    if (citationSettings.NumberOfAuthors > 0 && model.Authors.Count > citationSettings.NumberOfAuthors)
                    {
                        model.Authors = new List<string>() { model.Authors.Take(citationSettings.NumberOfAuthors) + " et al." };
                    }

                    // Version 
                    if (model.Version == null || string.IsNullOrEmpty(model.Version))
                    {
                        if (useTags && datasetVersion.Tag != null)
                            model.Version = datasetVersion.Tag.ToString();

                        model.Version = datasetManager.GetDatasetVersionNr(datasetVersionId).ToString();
                    }

                    var isPublic = entityPermissionManager.IsPublicAsync(datasetVersion.Dataset.EntityTemplate.EntityType.Id, datasetVersion.Dataset.Id).Result;

                    // Entity Type
                    if (String.IsNullOrEmpty(model.EntityType))
                    {
                        model.EntityType = datasetVersion.Dataset.EntityTemplate.EntityType.Name;
                    }

                    // Publication Year
                    if (model.Year == null || String.IsNullOrEmpty(model.Year))
                    {                       
                        if (isPublic)
                            model.Year = datasetVersion.PublicAccessDate.Date.ToString("yyyy");

                        model.Year = datasetVersion.Timestamp.Date.ToString("yyyy");
                    }

                    // Publisher
                    if (citationSettings != null && !String.IsNullOrEmpty(citationSettings.Publisher))
                    {
                        model.Publisher = citationSettings.Publisher;
                    }
                    else
                    {
                        model.Publisher = "BEXIS2";

                    }

                    // URL
                    if (String.IsNullOrEmpty(model.URL))
                    {
                        using (var publicationManager = new PublicationManager())
                        {
                            var pub = publicationManager.GetPublication(datasetVersion.Dataset.Id);
                            if (pub != null)
                            {
                                if (!String.IsNullOrEmpty(pub.Doi))
                                    model.URL = pub.Doi;
                            }
                            else
                            {
                                if(useTags)
                                    model.URL = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + $"/ddm/data/Showdata/{datasetVersion.Dataset.Id}?tag={model.Version}";

                                model.URL = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + $"/ddm/data/Showdata/{datasetVersion.Dataset.Id}?version={model.Version}";

                            }
                        }
                    }

                    return model;
                } 
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool IsCitationDataModelValid(CitationDataModel model)
        {
            try
            {
                var validationResults = new List<ValidationResult>();
                var context = new ValidationContext(model, serviceProvider: null, items: null);
                return Validator.TryValidateObject(model, context, validationResults, true);
            }
            catch (Exception ex) 
            { 
                return false;
            }
            
        }

        public static string DownloadCitationString(CitationDataModel model, DownloadCitationFormat format)
        {
            try
            {
                switch (format)
                {
                    default:
                        return "";
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return string.Empty;
            }
        }

        public static string DownloadApaFromCitationDataModel(CitationDataModel model)
        {
            if (model == null)
            {
                return string.Empty;
            }
            var citation = new StringBuilder();
            citation.AppendLine($"{string.Join(", ", model.Authors)} ({model.Year}). {model.Title}. Version {model.Version}.");
            if (!string.IsNullOrEmpty(model.DOI))
            {
                citation.AppendLine($"https://doi.org/{model.DOI}");
            }
            if (model.Projects != null && model.Projects.Count > 0)
            {
                citation.AppendLine($"Projects: {string.Join(", ", model.Projects)}");
            }
            return citation.ToString();
        }
        public static string DownloadBibTexFromCitationDataModel(CitationDataModel model)
        {
            if (model == null)
            {
                return string.Empty;
            }
            var bibTex = new StringBuilder();
            bibTex.AppendLine("@misc{");
            bibTex.AppendLine($"  title = {{{model.Title}}},");
            bibTex.AppendLine($"  version = {{{model.Version}}},");
            bibTex.AppendLine($"  date = {{{model.Year}}},");
            bibTex.AppendLine($"  doi = {{{model.DOI}}},");
            if (model.Authors != null && model.Authors.Count > 0)
            {
                bibTex.AppendLine("  author = {");
                for (int i = 0; i < model.Authors.Count; i++)
                {
                    bibTex.Append($"    {model.Authors[i]}");
                    if (i < model.Authors.Count - 1)
                    {
                        bibTex.Append(",");
                    }
                    bibTex.AppendLine();
                }
                bibTex.AppendLine("  },");
            }
            if (model.Projects != null && model.Projects.Count > 0)
            {
                bibTex.AppendLine("  projects = {");
                for (int i = 0; i < model.Projects.Count; i++)
                {
                    bibTex.Append($"    {model.Projects[i]}");
                    if (i < model.Projects.Count - 1)
                    {
                        bibTex.Append(",");
                    }
                    bibTex.AppendLine();
                }
                bibTex.AppendLine("  },");
            }
            bibTex.Append("}");
            return bibTex.ToString();
        }

        public static string DownloadRISFromCitationDataModel(CitationDataModel model)
        {
            if (model == null)
            {
                return string.Empty;
            }
            var ris = new StringBuilder();
            ris.AppendLine("TY  - GEN");
            ris.AppendLine($"TI  - {model.Title}");
            ris.AppendLine($"VL  - {model.Version}");
            ris.AppendLine($"PY  - {model.Year}");
            if (!string.IsNullOrEmpty(model.DOI))
            {
                ris.AppendLine($"DO  - {model.DOI}");
            }
            if (model.Authors != null && model.Authors.Count > 0)
            {
                foreach (var author in model.Authors)
                {
                    ris.AppendLine($"AU  - {author}");
                }
            }
            if (model.Projects != null && model.Projects.Count > 0)
            {
                foreach (var project in model.Projects)
                {
                    ris.AppendLine($"PB  - {project}");
                }
            }
            ris.AppendLine("ER  - ");
            return ris.ToString();
        }

        public static string GetTextFromCitationDataModel(CitationDataModel model)
        {
            return $"{model.Title}. ";
        }
    }
}