using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.View
{
    public class ViewModel
    {
        public long Id { get; set; }
        public long VersionId { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public List<Hook> Hooks { get; set; }

        public ViewModel()
        {
            Id = 0;
            Version = 0;
            VersionId = 0;
            Title = "";
            Hooks = new List<Hook>();
        }
    }
}