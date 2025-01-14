using BExIS.Dlm.Entities.Curation;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Modules.Dcm.UI.Models.Curation
{
    public class CurationEntryModel
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public CurationEntryType Type { get; set; }
        public long DatasetId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Solution { get; set; }
        public int Position { get; set; }
        public string Source { get; set; }
        public IEnumerable<CurationNoteModel> Notes { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatorId { get; set; }
        public bool UserlsDone { get; set; }
        public bool IsApproved { get; set; }
        public DateTime LastChangeDatetime_User { get; set; }
        public DateTime LastChangeDatetime_Curator { get; set; }

        public CurationEntryModel() { 
            
            Id = 0;
            Topic = string.Empty;
            Type = CurationEntryType.None;
            DatasetId = 0;
            Name = string.Empty;
            Description = string.Empty;
            Solution = string.Empty;
            Position = 0;
            Source = string.Empty;
            Notes = new List<CurationNoteModel>();
            CreationDate = DateTime.MinValue;
            CreatorId = 0;
            UserlsDone = false;
            IsApproved = false;
            LastChangeDatetime_User = DateTime.MinValue;
            LastChangeDatetime_Curator = DateTime.MinValue;
        }
    }

    public class CurationNoteModel
    {
        public long Id { get; set; }
        public CurationUserType UserType { get; set; }
        public DateTime CreationDate { get; set; }
        public string Comment { get; set; }
        public long UserId { get; set; }

    }
}
