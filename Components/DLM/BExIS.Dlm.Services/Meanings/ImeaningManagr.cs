﻿using BExIS.Dlm.Entities.Meanings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BExIS.Dlm.Services.Meanings
{
    public interface ImeaningManagr : IDisposable
    {
        #region Meaning manager class
        Meaning addMeaning(Meaning meaning);
        Meaning addMeaning(string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> variables_id, List<string> externalLink, List<string> meaning_ids);
        Boolean deleteMeaning(Meaning meaning);
        List<Meaning> deleteMeaning(Int64 id);
        Meaning editMeaning(Meaning meaning);
        Meaning editMeaning(string id, string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> variables_id, List<string> externalLink, List<string> meaning_ids);
        Meaning getMeaning(Int64 id);
        List<Meaning> getMeanings();
        List<Meaning> updateRelatedManings(string parentID, string childID);

        #endregion

        #region External Links manager
        ExternalLink addExternalLink(ExternalLink externalLink);
        ExternalLink addExternalLink(string uri, String name, String type);
        Boolean deleteExternalLink(ExternalLink externalLink);
        List<ExternalLink> deleteExternalLink(Int64 id);
        ExternalLink editExternalLink(ExternalLink externalLink);
        ExternalLink editExternalLink(string id, string uri, String name, String type);
        ExternalLink getExternalLink(Int64 id);
        ExternalLink getExternalLink(string uri);
        List<ExternalLink> getExternalLinks();

        #endregion


    }
}
