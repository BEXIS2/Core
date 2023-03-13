using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers.GBIF;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BExIS.Dim.Helpers.Export
{
    public class GBIFDataRepoConverter : IDataRepoConverter
    {
        private Repository _dataRepo { get; set; }
        private Broker _broker { get; set; }

        public string Convert(long datasetVersionId)
        {
            throw new NotImplementedException();
        }

        public bool Validate(long datasetVersionId, out List<string> errors)
        {
            long metadataStructureId = 0;
            bool valid = true;
            errors = new List<string>();
            // metadata ist valid
            using (var datasetManager = new DatasetManager())
            {
                var datasetversion = datasetManager.GetDatasetVersion(datasetVersionId);
                long datasetId = datasetversion.Dataset.Id;
                metadataStructureId = datasetversion.Dataset.MetadataStructure.Id;
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                var t = xmlDatasetHelper.GetAllTransmissionInformation(datasetId, TransmissionType.mappingFileExport, AttributeNames.name).ToList();

                string convertTo = t.ToArray().FirstOrDefault();
                string metadataValidMessage = OutputMetadataManager.IsValideAgainstSchema(datasetversion.Dataset.Id, TransmissionType.mappingFileExport, convertTo);

                // no metadata validation available for now
                //if (!string.IsNullOrEmpty(metadataValidMessage))
                //{
                //    errors.Add(metadataValidMessage);
                //}
            }

            // concept exist and is mapped to structure

            using (var conceptManager = new ConceptManager())
            {
                var concept = conceptManager.MappingConceptRepo.Get().Where(c => c.Name.Equals(_dataRepo.Name)).FirstOrDefault();

                if (concept == null)
                    errors.Add("Concept not exist.");
                else
                {
                    List<string> errorsList = new List<string>();
                    if (MappingUtils.IsMapped(metadataStructureId, LinkElementType.MetadataStructure, concept.Id, LinkElementType.MappingConcept, out errorsList) == false)
                        errors.AddRange(errorsList);
                }



                // in V2 file for structure exist
                // in data structure - dcw terms
                GbifHelper helper = new GbifHelper();
                helper.GenerateMeta(GbifDataType.samplingEvent);
            }

            if (errors.Any()) valid = false;

            return valid;

        }

        public GBIFDataRepoConverter(Repository datarepo)
        {
            _dataRepo = datarepo;
            _broker = datarepo.Broker;
        }
    }
}
