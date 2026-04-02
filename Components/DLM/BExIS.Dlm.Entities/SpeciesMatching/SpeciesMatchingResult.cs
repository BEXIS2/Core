using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Subjects;
using System;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.SpeciesMatching
{
    public class SpeciesMatchingResult : BaseEntity
    {

        // original unchanged name submitted for matching
        public virtual string OriginalName { get; set; }

        // cleaned name after preprocessing (e.g. trimming, removing special characters, etc.)
        public virtual string CleanedName { get; set; }

        // edited name after manual corrections (if any)
        public virtual string EditedName { get; set; }

        // matched name from the external source
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

        // reference to the specific version of the dataset
        // TODO: get this to work (maybe as a normal field instead of many-to-one relation)
        //public virtual DatasetVersion DatasetVersion { get; set; }

        // reference to the user who owns this matching result
        public virtual User Creator { get; set; }
    }
}