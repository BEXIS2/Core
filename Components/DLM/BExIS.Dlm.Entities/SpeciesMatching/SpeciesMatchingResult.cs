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