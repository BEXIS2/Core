using Newtonsoft.Json;
using System.Collections.Generic;

namespace BExIS.Dim.Entities.Export
{
    public class BioSchema
    {
        [JsonProperty("dataset")]
        public BioSchemaDataset Dataset { get; set; }
    }


    public class BioSchemaDataset
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("dct:conformsTo")]
        public string ConformsTo { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("identifier")]
        public string Identifier { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }
        [JsonProperty("keywords")]
        public List<string> Keywords { get; set; }
        [JsonProperty("citation")]
        public string Citation { get; set; }

        [JsonProperty("creator")]
        public List<BioSchemaPerson> Creator { get; set; }
        public BioSchemaDataset() { 
        
            Context = "https://schema.org/";
            Type = "Dataset";
            ConformsTo = "https://bioschemas.org/profiles/Dataset/1.0-RELEASE";
            Description = "";
            Identifier = "";
            License = "";
            Keywords = new List<string>();
            Name = "";
            Url = "";
            Citation = "";
            Creator = new List<BioSchemaPerson>();
            
        }
    }

    public class BioSchemaPerson
    {

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("familiName")]
        public string FamiliName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("affiliation")]
        public string Affiliation { get; set; }

        public BioSchemaPerson()
        {
            Type = "Person";
            GivenName = "";
            FamiliName = "";
            Email = "";
            Affiliation = "";
        }
    }
}
