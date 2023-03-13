
using BExIS.Dim.Helpers.Models;
using System;
using System.Collections.Generic;
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

        // generate the metafile 
        public void GenerateMeta(GbifDataType type)
        {
            Archive archive = new Archive();

            archive.Xmlns = "http://rs.tdwg.org/dwc/text/";
            archive.Metadata = "metadata.xml";
            archive.Core.files.Add("data.csv");
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

            XmlSerializer serializer = new XmlSerializer(typeof(Archive));
            string filepath = Path.Combine(AppConfiguration.DataPath, "temp", "meta.xml");
            if(File.Exists(filepath)) File.Delete(filepath);
            serializer.Serialize(File.Create(filepath), archive);



        }

        // generate the metafile 
        public void GenerateResourceMetadata()
        {
            throw new NotImplementedException();
        }
    }

 
    
}
