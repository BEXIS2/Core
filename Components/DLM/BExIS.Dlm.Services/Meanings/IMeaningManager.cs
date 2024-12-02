using BExIS.Dlm.Entities.Meanings;
using System;
using System.Collections.Generic;

namespace BExIS.Dlm.Services.Meanings
{
    public interface IMeaningManager : IDisposable
    {
        #region Meaning manager class

        Meaning AddMeaning(Meaning meaning);

        Meaning AddMeaning(string Name, String ShortName, String Description, bool selectable, bool approved, List<MeaningEntry> externalLinks, List<long> meaning_ids, List<long> constriant_ids);

        Boolean DeleteMeaning(Meaning meaning);

        List<Meaning> DeleteMeaning(Int64 id);

        Meaning EditMeaning(Meaning meaning);

        Meaning EditMeaning(long id, string Name, String ShortName, String Description, bool selectable, bool approved, List<MeaningEntry> externalLinks, List<long> meaning_ids, List<long> constriant_ids);

        Meaning GetMeaning(Int64 id);

        List<Meaning> GetMeanings();

        List<Meaning> UpdateRelatedManings(string parentID, string childID);

        #endregion Meaning manager class

        #region External Links manager

        ExternalLink AddExternalLink(ExternalLink externalLink);

        ExternalLink AddExternalLink(string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory);

        Boolean DeleteExternalLink(ExternalLink externalLink);

        List<ExternalLink> DeleteExternalLink(Int64 id);

        ExternalLink EditExternalLink(ExternalLink externalLink);

        ExternalLink editExternalLink(string id, string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory);

        ExternalLink GetExternalLink(Int64 id);

        ExternalLink GetExternalLink(string uri);

        List<ExternalLink> GetExternalLinks();

        List<ExternalLink> GetPrefixes();

        string GetPrefixfromUri(string uri);

        string GetfullUri(ExternalLink externalLink);

        string GetFormattedLinkUri(ExternalLink externalLink);

        string GetViewLinkUri(ExternalLink externalLink);

        Boolean UpdatePreviousLinks();

        #endregion External Links manager

        #region Prefix Category manager

        PrefixCategory AddPrefixCategory(PrefixCategory externalLink);

        PrefixCategory AddPrefixCategory(string Name, String Description);

        Boolean DeletePrefixCategory(PrefixCategory prefixCategory);

        List<PrefixCategory> DeletePrefixCategory(Int64 id);

        PrefixCategory EditPrefixCategory(PrefixCategory prefixCategory);

        PrefixCategory EditPrefixCategory(string id, string Name, String Description);

        PrefixCategory GetPrefixCategory(Int64 id);

        PrefixCategory GetPrefixCategory(string Name);

        List<PrefixCategory> GetPrefixCategory();

        #endregion Prefix Category manager
    }
}