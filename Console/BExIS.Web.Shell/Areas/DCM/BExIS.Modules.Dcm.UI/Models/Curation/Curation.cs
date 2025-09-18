using BExIS.Dim.Entities.Publications;
using BExIS.Dlm.Entities.Curation;
using BExIS.Security.Entities.Subjects;
using BExIS.Utils.Config.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool UserIsDone { get; set; }
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
            UserIsDone = false;
            IsApproved = false;
            LastChangeDatetime_User = DateTime.MinValue;
            LastChangeDatetime_Curator = DateTime.MinValue;
        }

        public CurationEntryModel(CurationEntry curationEntry)
        {
            Id = curationEntry.Id;
            Topic = curationEntry.Topic;
            Type = curationEntry.Type;
            DatasetId = curationEntry.Dataset.Id;
            Name = curationEntry.Name;
            Description = curationEntry.Description;
            Solution = curationEntry.Solution;
            Position = curationEntry.Position;
            Source = curationEntry.Source;
            CreationDate = curationEntry.CreationDate;
            CreatorId = curationEntry.Creator.Id;
            UserIsDone = curationEntry.UserIsDone;
            IsApproved = curationEntry.IsApproved;
            LastChangeDatetime_User = curationEntry.LastChangeDatetime_User;
            LastChangeDatetime_Curator = curationEntry.LastChangeDatetime_Curator;

            if (curationEntry.Notes != null && curationEntry.Notes.Any())
            {
                Notes = curationEntry.Notes.Select(n => new CurationNoteModel(n)).ToList();
            } else
            {
                Notes = new List<CurationNoteModel>();
            }
        }
    }

    public class CurationNoteModel
    {
        public long Id { get; set; }
        public CurationUserType UserType { get; set; }
        public DateTime CreationDate { get; set; }
        public string Comment { get; set; }
        public long UserId { get; set; }

        public CurationNoteModel()
        {
            Id = 0;
            UserType = CurationUserType.User;
            CreationDate = DateTime.MinValue;
            Comment = string.Empty;
            UserId = 0;
        }

        public CurationNoteModel(CurationNote curationNote)
        {
            Id = curationNote.Id;
            UserType = curationNote.UserType;
            CreationDate = curationNote.CreationDate;
            Comment = curationNote.Comment;
            UserId = curationNote.User.Id;
        }

    }

    public class CurationUserModel
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public CurationUserType CurationUserType { get; set; }

        public CurationUserModel()
        {
            Id = 0;
            DisplayName = string.Empty;
            CurationUserType = CurationUserType.User;
        }

        public CurationUserModel(long id, string displayName, CurationUserType curationUserType)
        {
            Id = id;
            DisplayName = displayName;
            CurationUserType = curationUserType;
        }

        public CurationUserModel(User user, long id)
        {
            Id = id;
            DisplayName = user.DisplayName;
            CurationUserType = CurationEntry.GetCurationUserType(user);
        }
    }

    public class CurationTemplateModel
    {
        public string name { get; set; }
        public string content { get; set; }

        public CurationTemplateModel()
        {
            name = string.Empty;
            content = string.Empty;
        }

        public CurationTemplateModel(string name, string content)
        {
            this.name = name;
            this.content = content;
        }
    }

    public class CurationModel
    {
        public long DatasetId { get; set; }
        public string DatasetTitle { get; set; }
        public DateTime DatasetVersionDate { get; set; }
        public IEnumerable<CurationEntryModel> CurationEntries { get; set; }
        public IEnumerable<CurationUserModel> CurationUsers { get; set; }
        public IEnumerable<CurationLabel> CurationLabels { get; set; }
        public IEnumerable<CurationEntryType> CurationEntryTypes { get; set; }
        public IEnumerable<CurationTemplateModel> GreetingTemplates { get; set; }
        public IEnumerable<CurationTemplateModel> TaskListTemplates { get; set; }

        public CurationModel()
        {
            DatasetId = 0;
            DatasetTitle = string.Empty;
            DatasetVersionDate = DateTime.MinValue;
            CurationEntries = new List<CurationEntryModel>();
            CurationUsers = new List<CurationUserModel>();
            CurationLabels = new List<CurationLabel>();
            CurationEntryTypes = Enum.GetValues(typeof(CurationEntryType)).Cast<CurationEntryType>().ToList();
            GreetingTemplates = new List<CurationTemplateModel>();
            TaskListTemplates = new List<CurationTemplateModel>();
        }

        public CurationModel(long datasetId, string datasetTitle, DateTime datasetVersionDate, IEnumerable<CurationEntryModel> curationEntries, IEnumerable<CurationUserModel> curationUsers, IEnumerable<CurationLabel> curationLabels, IEnumerable<CurationEntryType> curationEntryTypes, IEnumerable<CurationTemplateModel> greetingTemplates, IEnumerable<CurationTemplateModel> taskListTemplates)
        {
            DatasetId = datasetId;
            DatasetTitle = datasetTitle;
            DatasetVersionDate = datasetVersionDate;
            CurationEntries = curationEntries;
            CurationUsers = curationUsers;
            CurationLabels = curationLabels;
            CurationEntryTypes = curationEntryTypes;
            GreetingTemplates = greetingTemplates;
            TaskListTemplates = taskListTemplates;
        }

    }
}
