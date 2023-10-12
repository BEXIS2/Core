using BExIS.Dlm.Entities.Meanings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BExIS.Dlm.Services.Meanings
{
    public interface ImeaningManagr_JSON : IDisposable
    {
        #region Meaning manager class
        JObject addMeaning(Meaning meaning);
        JObject addMeaning(string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> variables_id, List<string> externalLink, List<string> meaning_ids);
        JObject deleteMeaning(Meaning meaning);
        JObject deleteMeaning(Int64 id);
        JObject editMeaning(Meaning meaning);
        JObject editMeaning(string id, string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> variables_id, List<string> externalLink, List<string> meaning_ids);
        JObject getMeaning(Int64 id);
        JObject getMeanings();
        JObject editRelatedMeaning(Meaning m);
        JObject updateRelatedManings(string parentID, string childID);

        #endregion

        #region External Links manager
        JObject addExternalLink(ExternalLink externalLink);
        JObject addExternalLink(string uri, String name, String type);
        JObject deleteExternalLink(ExternalLink externalLink);
        JObject deleteExternalLink(Int64 id);
        JObject editExternalLink(ExternalLink externalLink);
        JObject editExternalLink(string id, string uri, String name, String type);
        JObject getExternalLink(Int64 id);
        JObject getExternalLinks();

        #endregion


    }
}

