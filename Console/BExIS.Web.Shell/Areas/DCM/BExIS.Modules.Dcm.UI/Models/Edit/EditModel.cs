using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.UI.Hooks;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class EditModel
    {
        public long Id { get; set; }
        public long VersionId { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public List<Hook> Hooks { get; set; }
        public List<BExIS.UI.Hooks.View> Views { get; set; }

        public EditModel()
        {
            Id = 0;
            Version = 0;
            VersionId = 0;
            Title = "";
            Hooks = new List<Hook>();
            Views = new List<BExIS.UI.Hooks.View>();
        }
    }

    public class SortedError
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string Issue { get; set; }
        public ErrorType Type { get; set; }

        public List<string> Errors { get; set; }

        public SortedError()
        {
            Name = string.Empty;
            Count = 0;
            Issue = string.Empty;
            Type = ErrorType.Other;
            Errors = new List<string>();
        }

        public SortedError(string name, int count, string issue, ErrorType type, List<string> errors)
        {
            Name = name;
            Count = count;
            Issue = issue;
            Type = type;
            Errors = errors;
        }
    }
}