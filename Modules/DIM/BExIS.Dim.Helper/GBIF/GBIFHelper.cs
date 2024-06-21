
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Helpers.Models;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Xml.Helpers.Mapping;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.GBIF
{
    public class GbifHelper
    {
        public string[] OccurrencesRequiredDWTerms = { "occurrenceID", "basisOfRecord", "scientificName", "eventDate" };
        public string[] SamplingEventRequiredDWTerms = { "eventID", "eventDate", "samplingProtocol", "sampleSizeValue", "sampleSizeUnit" };
        private string releation = "hasDwcTerm";

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

        private List<Field> getDarwinCoreTermsFromDatastructure(long datastructureId)
        {
            List<Field> dwcterms = new List<Field>();

            if (datastructureId <= 0) throw new ArgumentNullException("datastructureId");
            using (var datastructureManager = new DataStructureManager())
            {
                var structure = datastructureManager.StructuredDataStructureRepo.Get(datastructureId);
                if(structure == null) throw new NullReferenceException("structure not exist");

                for (int i = 0; i < structure.Variables.Count(); i++) // go throw each varaible
                {
                    var variable = structure.Variables.ElementAt(i);
                    if (variable.Meanings.Any()) // check if meanings exist
                    {
                        //check if meanings belong to darwincore
                        foreach (var meaning in variable.Meanings)
                        {
                            var meaningEntries = meaning.ExternalLinks.Where(e => e.MappingRelation.Type == ExternalLinkType.relationship && e.MappingRelation.Name.Equals(releation));

                            if (meaningEntries.Any() == false) break; // no meanings in this context exist, please skip the for each loop
                            if (meaningEntries.Any() && meaningEntries.Count() > 1) throw new Exception("to many dwc terms mapped to one variable"); // Tomany meaning entries exist

                            var meaningEntry = meaningEntries.First();
                            var links = meaningEntry.MappedLinks;

                            if (links.Any()) // some mapping exist to dcw
                            { 
                                if(links.Count() > 1) throw new Exception("to many dwc terms mapped to one variable");

                                var term = links.FirstOrDefault();
                                var url = term.Prefix.URI+term.URI;
                                // if only one exist
                                Field field = new Field() {
                                    Index = i,
                                    Term = url
                                };
                                dwcterms.Add(field);
                            }
                        }
                    }
                }
            }

            return dwcterms;
        }

        private bool ExistRequiredDWTerms(List<Field> fields, GbifDataType type, out List<string> errors)
        {
            List<string> el = new List<string>();

            if (fields == null || fields.Count == 0) el.Add("no dwc terms linked to variable"); // check if fields has entries

            List<string> fieldNames = fields.Select(f => f.Term.Split('/').Last()).ToList(); // get all names of teh urls

            // compare the list of existing terms with the rwuired onces
            List<string> required = new List<string>();
            switch (type)
            { 
                case GbifDataType.samplingEvent: required = SamplingEventRequiredDWTerms.ToList(); break;
                case GbifDataType.occurrence: required = OccurrencesRequiredDWTerms.ToList(); break;
                default: required = OccurrencesRequiredDWTerms.ToList(); break;
            }

            var missingFromList1 = required.Where(item => !fieldNames.Any(x => x == item)).ToList();

            missingFromList1.ForEach(item => el.Add("dwc term " + item + " is missing."));

            errors = el;

            return missingFromList1.Any()?false:true;

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

                GbifHelper helper = new GbifHelper();

                // set dwc terms in meta
                DWTerms dwterms = new DWTerms();
                dwterms.Type = type;
                dwterms.Field = getDarwinCoreTermsFromDatastructure(structureId);

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
        public string GenerateResourceMetadata(long conceptId, long metadataStrutcureId, XmlDocument metadata, string directory, string xsdPath)
        {
            if (!File.Exists(xsdPath)) throw new FileNotFoundException("xsd");
            if (metadata == null) throw new NullReferenceException("metadata");
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException("directory");
            if (conceptId<=0) throw new ArgumentNullException("conceptId");
            if (metadataStrutcureId <= 0) throw new ArgumentNullException("metadataStrutcureId");

            string metadataFilePath = Path.Combine(directory, "metadata.xml");
            XmlDocument conceptOutput = MappingUtils.GetConceptOutput(metadataStrutcureId, conceptId, metadata);

            // FIX NEEDED - set lizenze url attribute - eml/dataset/intellectualRights/para/ulink/citetitle
            XmlElement linzenz = (XmlElement)conceptOutput.SelectSingleNode("eml/dataset/intellectualRights/para/ulink");
            if (linzenz != null)
            {
                XmlAttribute url = conceptOutput.CreateAttribute("url");
                url.Value = "na";
                linzenz.Attributes.Append(url);
            }

            XmlElement root = conceptOutput.CreateElement("eml", "eml", "eml://ecoinformatics.org/eml-2.1.1");
            root.InnerXml = conceptOutput.DocumentElement.InnerXml;

            XmlAttribute xsi = conceptOutput.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            xsi.Value = "eml://ecoinformatics.org/eml-2.1.1 http://rs.gbif.org/schema/eml-gbif-profile/1.2/eml.xsd";
            root.Attributes.Append(xsi);

            root.SetAttribute("system", "https://demo.bexis2.uni-jena.de");
            root.SetAttribute("scope", "system");
            root.SetAttribute("xml:lang", "en");
            root.SetAttribute("packageId", "na");

            
            conceptOutput.ReplaceChild(root, conceptOutput.DocumentElement);
          

            if (conceptOutput != null) conceptOutput.Save(metadataFilePath);

            // add parameter

            return metadataFilePath;
        }

        // validate metadata Gaianst XSD
        public bool ValidateResourceMetadata(string metadataFilePath, string xsdPath, out List<string> errors)
        {
            List<string>  el = new List<string>();

            if (!File.Exists(metadataFilePath)) throw new FileNotFoundException("metadata");
            if (!File.Exists(xsdPath)) throw new FileNotFoundException("xsd");

            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            xmlSchemaManager.Load(xsdPath, "system");


            XmlDocument metadata = new XmlDocument();
            metadata.Load(metadataFilePath);
            metadata.Schemas = xmlSchemaManager.SchemaSet;

            string msg = "";
            metadata.Validate((o, e) => {
                el.Add(e.Message);
            });

            errors = el;

            if (errors.Any()) return false;

            return true;
        }

        public bool ValidateDWCTerms(long datastructureId, GbifDataType type, out List<string> errors)
        {
            List<string> el = new List<string>();

            List<Field> fields = getDarwinCoreTermsFromDatastructure(datastructureId);

            bool existAllRequiredTerms = ExistRequiredDWTerms(fields, type,  out el);

            errors = el;

            return existAllRequiredTerms;
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
