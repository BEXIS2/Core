using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

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

        // Resolves the options for a given API identifier and options payload
        public IApiOptions ResolveOptions(string apiIdentifier, JObject options)
        {
            if (options == null) return null;
            if (string.IsNullOrWhiteSpace(apiIdentifier))
                throw new ArgumentException("API identifier cannot be null or empty.", nameof(apiIdentifier));

            MatchingApiBase api;
            try
            {
                api = GetApi(apiIdentifier);
            }
            catch (KeyNotFoundException)
            {
                // unknown api -> keep raw
                return new GenericOptions { Raw = options };
            }

            var targetType = api?.OptionsType;
            if (targetType != null)
            {
                try
                {
                    var typed = (IApiOptions)options.ToObject(targetType);
                    Validate(typed); // keep your existing Validate method
                    return typed;
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException("Invalid JSON for options payload.", ex);
                }
            }

            return new GenericOptions { Raw = options };
        }

        // Validates an options object using data annotations
        private void Validate(object obj)
        {
            if (obj == null) return;
            var ctx = new ValidationContext(obj);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(obj, ctx, results, true))
            {
                throw new ValidationException(results.First().ErrorMessage);
            }
        }
    }
}