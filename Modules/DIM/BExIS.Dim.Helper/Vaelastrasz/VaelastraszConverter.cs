using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dlm.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace BExIS.Dim.Helpers.Vaelastrasz
{
    public class VaelastraszConverter : IDataRepoConverter
    {
        private List<VaelastraszConfigurationItem> _mappings;
        private List<VaelastraszConfigurationItem> _placeholders;
        private Broker _broker;

        public VaelastraszConverter(Broker broker, List<VaelastraszConfigurationItem> mappings, List<VaelastraszConfigurationItem> placeholders)
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

                    var datasetversion = datasetManager.GetDatasetVersion(datasetVersionId);

                    var concept = conceptManager.MappingConcepts.Where(c => string.Equals(c.Name, _broker.Repository.Name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (concept == null)
                    {
                        errors.Add("The concept mapping does not exist.");
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
