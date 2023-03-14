
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Helpers.Models;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.GBIF
{
    public class GbifHelper
    {
        public string[] OccurrencesRequiredDWTerms = { "occurrenceID", "basisOfRecord", "scientificName", "eventDate" };
        public string[] SamplingEventRequiredDWTerms = { "eventID", "eventDate", "samplingProtocol", "sampleSizeValue", "sampleSizeUnit" };

        /// <summary>
        /// this function checks wheter there is in v2 a file for a datastructure 
        /// that maps varaibles to darwin core terms
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool DarwinCoreTermsMappingExist(long datastructureId)
        { 
            throw new NotImplementedException();
        }

        public void GetDarwinCoreTermsFromDatastructure(long datastructureId)
        {
            throw new NotImplementedException();
        }

        private bool ExistRequiredDWTerms(long datastructureId)
        {
            throw new NotImplementedException();
        }

        // generate the metafile 
        public string GenerateMeta(GbifDataType type, long structureId, string directory, string dataFile)
        {
            if (structureId <= 0) throw new ArgumentNullException("structureId");
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException("directory");

            using (var structureManager = new DataStructureManager())
            {
                var structure = structureManager.StructuredDataStructureRepo.Get(structureId);

                if (structure == null) throw new Exception("datastructure is not structured");

                string structurePath = Path.Combine(AppConfiguration.DataPath, "DataStructures", structure.Id.ToString());
                string dwtermsFilePath = Path.Combine(structurePath, "dw_terms.json");
                DWTerms dwterms = new DWTerms(); ;

                // load dc terms
                using (StreamReader r = new StreamReader(dwtermsFilePath))
                {
                    string json = r.ReadToEnd();
                    dwterms = JsonConvert.DeserializeObject<DWTerms>(json);
                }

                Archive archive = new Archive();

                archive.Xmlns = "http://rs.tdwg.org/dwc/text/";
                archive.Metadata = "metadata.xml";
                archive.Core.files.Add(dataFile);
                archive.Core.Encoding = "UTF-8";
                archive.Core.LinesTerminatedBy = ",";
                archive.Core.FieldsTerminatedBy = @"\n";
                archive.Core.IgnoreHeaderLines = "1";

                switch (type)
                {
                    case GbifDataType.samplingEvent:
                        archive.Core.RowType = "https://rs.tdwg.org/dwc/terms/Event"; break;
                    case GbifDataType.occurrence:
                        archive.Core.RowType = "https://rs.tdwg.org/dwc/terms/Occurrence"; break;
                    default:
                        archive.Core.RowType = ""; break;
                }

                // add fields
                if (dwterms.Field.Any())
                {
                    archive.Core.fields = dwterms.Field;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(Archive));
                string filepath = Path.Combine(directory, "meta.xml");
                if (File.Exists(filepath)) File.Delete(filepath);

                using (var stream = File.Create(filepath))
                {
                    serializer.Serialize(stream, archive);
                }
                return filepath;
            }
        }

        // generate the metafile 
        public string GenerateResourceMetadata(long conceptId, long metadataStrutcureId, XmlDocument metadata, string directory)
        {
            string metadataFilePath = Path.Combine(directory, "metadata.xml");
            XmlDocument conceptOutput = MappingUtils.GetConceptOutput(metadataStrutcureId, conceptId, metadata);
            if (conceptOutput != null) conceptOutput.Save(metadataFilePath);

            // add parameter

            return metadataFilePath;
        }

        public string GenerateData(long id, long versionId)
        {
            string datapath = string.Empty;

            var outputDataManager = new OutputDataManager();
            datapath = outputDataManager.GenerateAsciiFile(id, versionId,  "text/csv", false,true);

            return datapath;
        }
    }

 
    
}
