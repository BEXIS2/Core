using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace BExIS.Modules.Smm.UI.Helpers.MatchingAPIs
{
    public static class HttpClientRegistry
    {
        public static readonly HttpClient SharedClient = new HttpClient();

        static HttpClientRegistry()
        {
            // Set any default configuration for the shared HttpClient here if needed
        }
    }
}