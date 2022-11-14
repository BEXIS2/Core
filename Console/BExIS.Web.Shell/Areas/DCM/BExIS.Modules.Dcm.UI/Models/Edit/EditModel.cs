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
}