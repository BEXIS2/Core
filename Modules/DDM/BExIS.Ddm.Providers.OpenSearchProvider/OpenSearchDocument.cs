using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSearch.Client;

namespace BExIS.Ddm.Providers.OpenSearchProvider
{
    public class OpenSearchDocument
    {
        [PropertyName("doc_id")]
        public string DocId { get; set; }

        [PropertyName("gen_isPublic")]
        public bool? IsPublic { get; set; }

        [PropertyName("gen_entity_name")]
        public string EntityName { get; set; }

        [PropertyName("gen_modifieddate")]
        public string ModifiedDate { get; set; }

        [PropertyName("gen_entitytemplate")]
        public string EntityTemplate { get; set; }

        [PropertyName("Property")]
        public string Property {  get; set; }

        [PropertyName("category_id")]
        public string CategoryId { get; set; }

        [PropertyName("category_template")]
        public string CategoryTemplate { get; set; }

        [PropertyName("category_entity")]
        public string CategoryEntity {  get; set; }

        [PropertyName("category_doi")]
        public string CategoryDoi { get; set; }

        [PropertyName("ng_template")]
        public List<string> NgTemplates { get; set; } = new List<string>();

        [PropertyName("ng_entity")]
        public List<string> NgEntities { get; set; } = new List<string>();

        [PropertyName("ng_doi")]
        public List<string> NgDois { get; set; } = new List<string>();

        [PropertyName("ng_all")]
        public List<string> NgAll { get; set; } = new List<string>();

        [PropertyName("ng_id")]
        public List<string> NgIds { get; set; } = new List<string>();

        [PropertyName("Property_Numeric")]
        public string PropertyNumeric { get; set; }

        [PropertyName("Facet")]
        public string Facet { get; set; }

    }
}
