using System;

using OpenSearch.Client;

namespace BExIS.Ddm.Providers.OpenSearch
{
    public static class OpenSearchProvider
    {

        public static readonly string DefaultIndex = "bexissearchindex";
        public static readonly string AutoCompleteIndex = "autocompleteindex";
        private static readonly string _openSearchUrl = "https://localhost:9200";

        // TODO: redundant? muss es nach ausseshin sichtbar sein?
        public static string OpenSearchUrl
        {
            get { return _openSearchUrl; }
        }

        private static readonly ConnectionSettings _settings = new ConnectionSettings(new Uri(_openSearchUrl))
            .DefaultIndex(DefaultIndex)
            .BasicAuthentication("admin", "BExIS2_OS#")
            .ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true)
            .DisableDirectStreaming();

        // create connection to OpenSearch-REST API with the given settings
        public static OpenSearchClient Client { get; } = new OpenSearchClient(_settings);

    }
}
