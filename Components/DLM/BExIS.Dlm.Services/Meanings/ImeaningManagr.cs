using BExIS.Dlm.Entities.Meanings;
using System;
using System.Collections.Generic;

namespace BExIS.Dlm.Services.Meanings
{
    public interface ImeaningManagr : IDisposable
    {
        #region Meaning manager class

        Meaning addMeaning(Meaning meaning);

        Meaning addMeaning(string Name, String ShortName, String Description, bool selectable, bool approved, List<MeaningEntry> externalLinks, List<long> meaning_ids, List<long> constriant_ids);

        Boolean deleteMeaning(Meaning meaning);

        List<Meaning> deleteMeaning(Int64 id);

        Meaning editMeaning(Meaning meaning);

        Meaning editMeaning(long id, string Name, String ShortName, String Description, bool selectable, bool approved, List<MeaningEntry> externalLinks, List<long> meaning_ids, List<long> constriant_ids);

        Meaning getMeaning(Int64 id);

        List<Meaning> getMeanings();

        List<Meaning> updateRelatedManings(string parentID, string childID);

        #endregion Meaning manager class

        #region External Links manager

        ExternalLink addExternalLink(ExternalLink externalLink);

        ExternalLink addExternalLink(string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory);

        Boolean deleteExternalLink(ExternalLink externalLink);

        List<ExternalLink> deleteExternalLink(Int64 id);

        ExternalLink editExternalLink(ExternalLink externalLink);

        ExternalLink editExternalLink(string id, string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory);

        ExternalLink getExternalLink(Int64 id);

        ExternalLink getExternalLink(string uri);

        List<ExternalLink> getExternalLinks();

        List<ExternalLink> getPrefixes();

        string getPrefixfromUri(string uri);

        string getfullUri(ExternalLink externalLink);

        string getFormattedLinkUri(ExternalLink externalLink);

        string getViewLinkUri(ExternalLink externalLink);

        Boolean updatePreviousLinks();

        #endregion External Links manager

        #region Prefix Category manager

        PrefixCategory addPrefixCategory(PrefixCategory externalLink);

        PrefixCategory addPrefixCategory(string Name, String Description);

        Boolean deletePrefixCategory(PrefixCategory prefixCategory);

        List<PrefixCategory> deletePrefixCategory(Int64 id);

        PrefixCategory editPrefixCategory(PrefixCategory prefixCategory);

        PrefixCategory editPrefixCategory(string id, string Name, String Description);

        PrefixCategory getPrefixCategory(Int64 id);

        PrefixCategory getPrefixCategory(string Name);

        List<PrefixCategory> getPrefixCategory();

        #endregion Prefix Category manager
    }
}