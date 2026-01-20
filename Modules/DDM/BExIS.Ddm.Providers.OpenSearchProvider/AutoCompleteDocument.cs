using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using OpenSearch.Client;

namespace BExIS.Ddm.Providers.OpenSearchProvider
{
    public class AutoCompleteDocument
    {
        public string Id { get; set; }
        public List<CompletionField> Suggest { get; set; } = new List<CompletionField>();
        public CompletionField All { get; set; } = new CompletionField();

        private List<string> _all = new List<string>();

        public AutoCompleteDocument(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Adds single <see cref="OpenSearch.Client.CompletionField"/> to document 
        /// </summary>
        /// <param name="field"></param>
        public void AddCompletionField(CompletionField field) => Suggest.Add(field);

        /// <summary>
        /// Adds single input with context type "category"
        /// </summary>
        /// <param name="input"></param>
        /// <param name="contextValue"></param>
        public void AddCategory(string input, string contextValue)
        {
            var compField = new CompletionField
            {
                Input = new[] { input },
                Contexts = new Dictionary<string, IEnumerable<string>> { { "category", new[] { contextValue } } }
            };
            Suggest.Add(compField);
            _all.Add(input);
        }

        public void WithAllCompletion()
        {
            All = new CompletionField
            {
                Input = _all.ToArray(),
            };
        }


        /// <summary>
        /// Adds single input with context type "location"
        /// </summary>
        /// <param name="input"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void AddLocation(string input, double latitude, double longitude)
        {
            var compField = new CompletionField
            {
                Input = new[] { input },
                Contexts = new Dictionary<string, IEnumerable<string>>
        {
            { "location", new[] { $"{latitude}, {longitude}" } }
        }
            };
            Suggest.Add(compField);
        }


        /// <summary>
        /// Single Input without context
        /// </summary>
        /// <param name="inputs"></param>
        public void AddSuggestion(params string[] inputs)
        {
            var compField = new CompletionField
            {
                Input = inputs
            };
            Suggest.Add(compField);
        }

        /// <summary>
        /// Single input with single context
        /// </summary>
        /// <param name="input"></param>
        /// <param name="contextKey"></param>
        /// <param name="contextValue"></param>
        public void AddSuggestion(string input, string contextKey, string contextValue)
        {
            var compField = new CompletionField
            {
                Input = new[] { input },
                Contexts = new Dictionary<string, IEnumerable<string>> { { contextKey, new[] { contextValue } } }
            };
            Suggest.Add(compField);
        }

        /// <summary>
        /// Multi input with multi contexts
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="contextKey"></param>
        /// <param name="contextValues"></param>
        public void AddSuggestion(IEnumerable<string> inputs, string contextKey, IEnumerable<string> contextValues)
        {
            var compField = new CompletionField
            {
                Input = inputs.ToArray(),
                Contexts = new Dictionary<string, IEnumerable<string>> { { contextKey, contextValues } }
            };
            Suggest.Add(compField);
        }

    }
}
