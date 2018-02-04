using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Vim.UI
{
    public class VimModule : ModuleBase
    {
        public VimModule() : base("vim") => LoggerFactory.GetFileLogger().LogCustom("...ctor of vim...");

    }
}
