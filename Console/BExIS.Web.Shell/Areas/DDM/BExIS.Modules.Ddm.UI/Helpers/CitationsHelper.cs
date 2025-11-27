using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using NameParser;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class CitationsHelper
    {
        public static CitationDataModel CreateCitationDataModel(DatasetVersion datasetVersion, CitationFormat format = 0)
        {
            try
            {
                using (var datasetManager = new DatasetManager())
                using (var conceptManager = new ConceptManager())
                using (var entityPermissionManager = new EntityPermissionManager())
                using (var entityManager = new EntityManager())
                {
                    var metadata = datasetVersion.Metadata;

                    var concept = conceptManager.MappingConceptRepository.Query(c => c.Name.ToLower() == "citation_" + format.ToString().ToLower()).FirstOrDefault();

                    if (concept == null)
                        return null;

                    var conceptOutput = MappingUtils.GetConceptOutput(datasetVersion.Dataset.MetadataStructure.Id, concept.Id, metadata);

                    var model = new CitationDataModel();
                    XmlSerializer serializer = new XmlSerializer(typeof(CitationDataModel), new XmlRootAttribute("data"));
                    using (XmlReader reader = new XmlNodeReader(conceptOutput))
                    {
                        model = (CitationDataModel)serializer.Deserialize(reader);
                    }

                    if (model == null)
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

                    //create authorname in the correct format
                    List<string> authors = new List<string>();
                    foreach (string author in model.Authors)
                    {
                        HumanName name = new HumanName(author);
                        if (String.IsNullOrEmpty(name.Middle))
                            authors.Add(name.Last + ", " + name.First);
                        else
                            authors.Add(name.Last + ", " + name.Middle + " " + name.First);
                    }

                    model.Authors = authors;

                    // Version 
                    if (model.Version == null || string.IsNullOrEmpty(model.Version))
                    {
                        if (useTags && datasetVersion.Tag != null)
                            model.Version = datasetVersion.Tag.ToString();

                        model.Version = datasetManager.GetDatasetVersionNr(datasetVersion.Id).ToString();
                    }

                    var isPublic = entityPermissionManager.IsPublicAsync(datasetVersion.Dataset.EntityTemplate.EntityType.Id, datasetVersion.Dataset.Id).Result;

                    // Entity Type
                    if (String.IsNullOrEmpty(model.EntityName))
                    {
                        model.EntityName = datasetVersion.Dataset.EntityTemplate.EntityType.Name;
                    }

                    // Publication Year
                    if (model.Year == null || String.IsNullOrEmpty(model.Year))
                    {
                        if (isPublic)
                            model.Year = datasetVersion.PublicAccessDate.Date.ToString("yyyy");

                        model.Year = datasetVersion.Timestamp.Date.ToString("yyyy");
                    }

                    // URL
                    if (String.IsNullOrEmpty(model.URL))
                    {
                        model.URL = HttpContext.Current.Request.Url.Host;
                    }

                    using (var publicationManager = new PublicationManager())
                    {
                        var pub = publicationManager.GetPublication(datasetVersion.Dataset.Id);
                        if (pub != null)
                        {
                            if (!String.IsNullOrEmpty(pub.Doi))
                                model.DOI = pub.Doi;
                            //else if (!String.IsNullOrEmpty(pub.ExternalLink))
                            //    model.URL = pub.ExternalLink;
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

        public static string GetCitationString(CitationDataModel model, CitationFormat format, bool isPublic, long entityId, bool useTags)
        {
            try
            {
                switch (format)
                {
                    case CitationFormat.Bibtex:
                        return GenerateBibtex(entityId, model, isPublic, useTags);
                    case CitationFormat.RIS:
                        return GenerateRis(entityId, model, isPublic, useTags);
                    case CitationFormat.Text:
                        return GenerateText(entityId, model, isPublic, useTags);
                    case CitationFormat.APA:
                        return GetApaFromCitationDataModel(model);
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

        public static string GetApaFromCitationDataModel(CitationDataModel model)
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

        public static string GenerateBibtex(long entityId,CitationDataModel model, bool isPublic,  bool useTags)
        {
            if (model == null)
            {
                return string.Empty;
            }

            string bibtex = "@misc{" + model.Keyword + "\n";
            bibtex += "title ={" + model.Title + "},\n";
            bibtex += "author ={";
            var lastAuthor = model.Authors.Last();
            foreach (string author in model.Authors)
            {
                if (author == lastAuthor)
                    bibtex += author + "";
                else
                    bibtex += author + " and ";
            }
            bibtex += "},\n";

            bibtex += "version ={" + model.Version + "},\n";

            bibtex += "year ={" + model.Year + "},\n";
            bibtex += "publisher ={" + model.Publisher + "},\n";

            string url = model.URL;
            if (isPublic)
            {
                if (useTags)
                    url += "/ddm/data/Showdata/" + entityId + "?tag=" + model.Version + "";
                else
                    url += "/ddm/data/Showdata/" + entityId + "?version=" + model.Version + "";

                bibtex += "url ={" + url + "},\n";
            }
            else
                bibtex += "url ={" + url + "},\n";

            if (!String.IsNullOrEmpty(model.DOI))
                bibtex += "doi ={" + model.DOI + "},\n";

            if (!String.IsNullOrEmpty(model.EntityName))
            { 
                if (isPublic)
                    bibtex += "type ={" + model.EntityName + ". Published.},\n";
                else
                    bibtex += "type ={" + model.EntityName + ". Unpublished.},\n";
            }
            if(String.IsNullOrEmpty(model.Note))
                bibtex += "note ={" + model.EntityName + " ID: " + entityId + "},\n";
            else
                bibtex += "note ={"+ model.Note + "},\n";

            bibtex += "}";

            return bibtex;
        }

        public static string GenerateRis(long entityId, CitationDataModel model, bool isPublic, bool useTags)
        {
            if (model == null)
            {
                return string.Empty;
            }
            string ris = "TY - " + model.EntryType + " \n";
            ris += "T1 - " + model.Title + "\n";

            foreach (string author in model.Authors)
            {
                ris += "AU - " + author + "\n";
            }
            ris += "PY - " + model.Year + " \n";
            ris += "ET - " + model.Version + " \n";
            ris += "PB - " + model.Publisher + " \n";

            if (!String.IsNullOrEmpty(model.DOI))
                ris += "DO - " + model.DOI + "\n";

            string url = model.URL;
            if (isPublic)
            {
                if (useTags)
                    url += "/ddm/data/Showdata/" + entityId + "?tag=" + model.Version + "";
                else
                    url += "/ddm/data/Showdata/" + entityId + "?version=" + model.Version + "";

                ris += "UR - " + url + "\n";
            }
            else
                ris += "UR - " + url + "\n";

            if (isPublic)
                ris += "N1 - " + model.EntityName +" ID: " + entityId + ", Published. \n";
            else
                ris += "N1 - " + model.EntityName + " ID: " + entityId + ", Unpublished. \n";
            ris += "ER -";

            return ris;
        }

        public static string GenerateText(long entityId,CitationDataModel model, bool isPublic, bool useTags)
        {
            string text = "";
            var lastAuthor = model.Authors.Last();
            foreach (string author in model.Authors)
            {
                if (author.Equals(lastAuthor))
                    text += author;
                else
                    text += author + "; ";
            }
            text += " (" + model.Year + "): ";
            text += model.Title + ". ";
            text += "Version " + model.Version + ". ";
            text += model.Publisher + ". ";
            text += model.EntityName + ". ";

            if (!String.IsNullOrEmpty(model.DOI))
            {
                text += model.DOI;
            }
            else
            {
                string url = model.URL;
                if (isPublic)
                {
                    if (useTags)
                        url += "/ddm/data/Showdata/" + entityId + "?tag=" + model.Version + "";
                    else
                        url += "/ddm/data/Showdata/" + entityId + "?version=" + model.Version + "";

                    text += url + ". ";
                }
                else
                    text += url + ". ";

                if(String.IsNullOrEmpty(model.Note))
                    text += model.EntityName + " ID= " + entityId;
                else
                    text += model.Note;
            }

            return text;
        }
    }
}