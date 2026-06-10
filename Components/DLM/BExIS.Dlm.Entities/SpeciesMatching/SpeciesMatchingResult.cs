using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Subjects;
using System;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.SpeciesMatching
{
    public class SpeciesMatchingResult : BaseEntity
    {

        // original unchanged name (used for matching if EditedName is empty, and for display purposes)
        public virtual string OriginalName { get; set; }

        // edited name after data cleaning + manual corrections (used for matching)
        public virtual string EditedName { get; set; }

        // matched name from the external source (the result)
        public virtual string MatchedName { get; set; }

        // taxonomic status of the matched name (e.g. accepted, synonym, etc.)
        public virtual string Status { get; set; }

        // type of the match (e.g. exact, fuzzy, etc.)
        public virtual string MatchType { get; set; }

        // taxonomic rank of the matched name (e.g. species, genus, etc.)
        public virtual string MatchRank { get; set; }

        // unique identifier of the matched name in the external source (e.g. GBIF taxon ID)
        public virtual string MatchId { get; set; }

        // authorship of the matched name (e.g. Linnaeus, 1758)
        public virtual string MatchAuthorship { get; set; }

        // accepted name if (for example) the matched name is a synonym
        public virtual string AcceptedScientificName { get; set; }

        // unique identifier of the accepted name in the external source (e.g. GBIF taxon ID)
        public virtual string AcceptedId { get; set; }

        // authorship of the accepted name
        public virtual string AcceptedAuthorship { get; set; }

        // higher classification of the matched name (e.g. kingdom, phylum, class, order, family, genus)
        public virtual string TaxonKingdom { get; set; }

        public virtual string TaxonPhylum { get; set; }

        public virtual string TaxonClass { get; set; }

        public virtual string TaxonOrder { get; set; }

        public virtual string TaxonFamily { get; set; }

        public virtual string TaxonGenus { get; set; }

        // timestamp of the match
        public virtual DateTime TimestampMatch { get; set; }

        // source of the match (e.g. Catalogue of Life, GBIF, etc.)
        public virtual string MatchSource { get; set; }

        // version of the source used for matching
        public virtual string MatchSourceVersion { get; set; }

        // indicates whether the match has been confirmed by the user
        public virtual bool ConfirmedByUser { get; set; }

        // reference to the dataset where the original name was taken from
        public virtual Dataset Dataset { get; set; }

        // VersionId + Dataset make the unique key for the matching result
        public virtual long DatasetVersionId { get; set; }
    }
}