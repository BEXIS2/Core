using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.Vaelastrasz
{
    public class VaelastraszConverter : IDataRepoConverter
    {
        private Dictionary<string, string> _mappings;
        private Dictionary<string, string> _placeholders;
        private Broker _broker;

        public VaelastraszConverter(Broker broker, Dictionary<string, string> mappings, Dictionary<string, string> placeholders)
        {
            _broker = broker;
            _mappings = mappings;
            _placeholders = placeholders;
        }

        public string Convert(long datasetVersionId)
        {
            throw new NotImplementedException();
        }

        public bool Validate(long datasetVersionId, out List<string> errors)
        {
            try
            {
                using (var conceptManager = new ConceptManager())
                using (var datasetManager = new DatasetManager())
                {
                    errors = new List<string>();

                    var concept = conceptManager.MappingConcepts.Where(c => string.Equals(c.Name, _broker.Repository.Name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (concept == null)
                    {
                        errors.Add("The concept mapping does not exist.");
                        return false;
                    }

                    var datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                    if(datasetVersion == null)
                    {
                        errors.Add("The dataset version does not exist.");
                        return false;
                    }

                    var metadataStructureId = datasetVersion.Dataset.MetadataStructure.Id;

                    if (!MappingUtils.IsMapped(metadataStructureId, LinkElementType.MetadataStructure, concept.Id, LinkElementType.MappingConcept, out errors))
                    {
                        return false;
                    }

                    return true;
                }

            }catch (Exception ex)
            {
                errors = new List<string>();
                return false;
            }
        }
    
        
    }
}
