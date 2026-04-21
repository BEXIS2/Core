using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Helpers.MatchingAPIs
{
    public class MatchingApiProvider
    {
        private readonly Dictionary<string, MatchingApiBase> _apiRegistry;

        public MatchingApiProvider()
        {
            var sharedClient = HttpClientRegistry.SharedClient;

            var apiList = new List<MatchingApiBase> 
            {
                new CLBApi(sharedClient),
            };

            // Register available APIs here
            _apiRegistry = apiList.ToDictionary(
                api => api.Identifier,
                api => api,
                StringComparer.OrdinalIgnoreCase
            );
        }

        public MatchingApiBase GetApi(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("API identifier cannot be null or empty.", nameof(identifier));
            if (_apiRegistry.TryGetValue(identifier, out var api))
            {
                return api;
            }
            throw new KeyNotFoundException($"No matching API found for identifier: {identifier}");
        }
    }
}