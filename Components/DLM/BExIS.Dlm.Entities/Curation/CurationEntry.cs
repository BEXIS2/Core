using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Curation
{
    public class CurationEntry:BaseEntity
    {
        public CurationEntry()
        {
            // Initialize properties with empty values
            Id = 0;
            Topic = string.Empty;
            Type = CurationEntryType.None;
            Dataset = null;
            Name = string.Empty;
            Description = string.Empty;
            Solution = string.Empty;
            Position = 0;
            Source = string.Empty;
            Notes = new List<CurationNote>();
            CreationDate = DateTime.MinValue;
            Creator = null;
            UserIsDone = false;
            IsApproved = false;
            LastChangeDatetime_User = DateTime.MinValue;
            LastChangeDatetime_Curator = DateTime.MinValue;
        }

        public CurationEntry(string topic, CurationEntryType type, Dataset dataset, string name, string description, string solution, int position, string source, IEnumerable<CurationNote> notes, DateTime creationDate, User creator, bool userIsDone, bool isApproved, DateTime lastChangeDatetime_User, DateTime lastChangeDatetime_Curator)
        {
            Id = 0;
            Topic = topic;
            Type = type;
            Dataset = dataset;
            Name = name;
            Description = description;
            Solution = solution;
            Position = position;
            Source = source;
            Notes = notes;
            CreationDate = creationDate;
            Creator = creator;
            UserIsDone = userIsDone;
            IsApproved = isApproved;
            LastChangeDatetime_User = lastChangeDatetime_User;
            LastChangeDatetime_Curator = lastChangeDatetime_Curator;
        }

        public virtual string Topic { get; set; }
        public virtual CurationEntryType Type { get; set; }
        public virtual Dataset Dataset { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Solution { get; set; }
        public virtual int Position { get; set; }
        public virtual string Source { get; set; }
        public virtual IEnumerable<CurationNote> Notes { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual User Creator { get; set; }
        public virtual bool UserIsDone { get; set; }
        public virtual bool IsApproved { get; set; }
        public virtual DateTime LastChangeDatetime_User { get; set; }
        public virtual DateTime LastChangeDatetime_Curator { get; set; }

        public static CurationUserType GetCurationUserType(User user, string curationGroupName)
        {
            if (user.Groups.Any(g => g.Name.Equals(curationGroupName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return CurationUserType.Curator;
            }
            return CurationUserType.User;
        }
    }

    #region curation types

    public enum CurationEntryType
    { 
        None,
        StatusEntryItem,
        GeneralEntryItem,
        MetadataEntryItem,
        PrimaryDataEntryItem,
        DatastrutcureEntryItem,
        LinkEntryItem,
        AttachmentEntryItem
    }

    public class MetadataEntryItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
        public long Id { get; set; }

    }

    public class PrimaryDataEntryItem
    {
        public string Column { get; set; }
        public int Row { get; set; }
        public string Value { get; set; }

    }

    public class DataStructureEntryItem
    {
        public string VarName { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

    }

    #endregion
}
