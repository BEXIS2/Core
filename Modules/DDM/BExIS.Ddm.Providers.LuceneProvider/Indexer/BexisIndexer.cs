﻿using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Searcher;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Metadata;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Logging;
using Vaiona.Persistence.Api;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider.Indexer
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class BexisIndexer
    {
        private List<Facet> AllFacets = new List<Facet>();
        private List<Property> AllProperties = new List<Property>();
        private List<Category> AllCategories = new List<Category>();
        private bool reIndex = false;
        private bool isIndexConfigured = false;
        public List<XmlNode> facetXmlNodeList = new List<XmlNode>();
        public List<XmlNode> propertyXmlNodeList = new List<XmlNode>();
        public List<XmlNode> categoryXmlNodeList = new List<XmlNode>();
        public List<XmlNode> generalXmlNodeList = new List<XmlNode>();
        public List<XmlNode> headerItemXmlNodeList = new List<XmlNode>();

        public bool includePrimaryData = false;

        private EntityManager entityManager;
        private EntityPermissionManager entityPermissionManager;
        private long? entityTypeId;

        public BexisIndexer()
        {
            entityPermissionManager = new EntityPermissionManager();

            entityManager = new EntityManager();
            entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
            entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        private void LoadBeforeIndexing()
        {
            XmlNodeList fieldProperties = configXML.GetElementsByTagName("field");
            Category category = new Category();
            category.Name = "All";
            category.Value = "All";
            category.DefaultValue = "nothing";
            AllCategories.Add(category);
            foreach (XmlNode fieldProperty in fieldProperties)
            {
                String fieldType = fieldProperty.Attributes.GetNamedItem("type").Value;
                String fieldName = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;

                if (fieldType.ToLower().Equals("facet_field"))
                {
                    facetXmlNodeList.Add(fieldProperty);
                    Facet c = new Facet();
                    c.Name = fieldName;
                    c.Text = fieldName;
                    c.Value = fieldName;
                    //c.Expanded = true;
                    //c.Enabled = true;
                    c.Childrens = new List<Facet>();
                    AllFacets.Add(c);
                }
                else if (fieldType.ToLower().Equals("property_field"))
                {
                    propertyXmlNodeList.Add(fieldProperty);
                    Property c = new Property();
                    c.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    c.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value; ;

                    c.DataSourceKey = fieldProperty.Attributes.GetNamedItem("metadata_name").Value;

                    c.UIComponent = fieldProperty.Attributes.GetNamedItem("uiComponent").Value; ;
                    c.AggregationType = "distinct";
                    c.DefaultValue = "All";
                    c.DataType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;
                    AllProperties.Add(c);
                }
                else if (fieldType.ToLower().Equals("category_field") || fieldType.ToLower().Equals("primary_data_field"))
                {
                    categoryXmlNodeList.Add(fieldProperty);
                    Category c = new Category();
                    c.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    c.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    c.DefaultValue = "nothing";
                    AllCategories.Add(c);
                }
                else if (fieldType.ToLower().Equals("general_field"))
                {
                    generalXmlNodeList.Add(fieldProperty);
                }
            }
        }

        private string luceneIndexPath = Path.Combine(FileHelper.IndexFolderPath, "BexisSearchIndex");
        private string autoCompleteIndexPath = Path.Combine(FileHelper.IndexFolderPath, "BexisAutoComplete");

        private IndexWriter indexWriter;
        private IndexWriter autoCompleteIndexWriter;
        private Lucene.Net.Store.Directory pathIndex;
        private Lucene.Net.Store.Directory autoCompleteIndex;
        private XmlDocument configXML;

        private void configureBexisIndexing(bool recreateIndex)
        {
            configXML = new XmlDocument();
            configXML.Load(FileHelper.ConfigFilePath);

            LoadBeforeIndexing();
            pathIndex = FSDirectory.Open(new DirectoryInfo(luceneIndexPath));
            autoCompleteIndex = FSDirectory.Open(new DirectoryInfo(autoCompleteIndexPath));

            using (var bexisAnalyzer = new BexisAnalyzer())
            using (var nGramAnalyzer = new NGramAnalyzer())
            using (PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper(bexisAnalyzer))
            {
                indexWriter = new IndexWriter(pathIndex, analyzer, recreateIndex, IndexWriter.MaxFieldLength.UNLIMITED);
                autoCompleteIndexWriter = new IndexWriter(autoCompleteIndex, nGramAnalyzer, recreateIndex, IndexWriter.MaxFieldLength.UNLIMITED);

                foreach (XmlNode a in categoryXmlNodeList)
                {
                    analyzer.AddAnalyzer("ng_" + a.Attributes.GetNamedItem("lucene_name").Value, nGramAnalyzer);
                }
                analyzer.AddAnalyzer("ng_all", nGramAnalyzer);

                isIndexConfigured = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void Index(bool onlyReleasedTags)
        {
            configureBexisIndexing(true);

            List<string> errors = new List<string>();
            try
            {
                //ToDo only enitities from type dataset should be indexed in this index
                using (var publicationManager = new PublicationManager())
                using(DatasetManager dm = new DatasetManager())
                {
                    // if the system is using tags, then only datasets that have a tag and its released should be indexeds

                    IList<long> ids = new List<long>();
                if (!onlyReleasedTags) ids = dm.GetDatasetLatestIds(); // index all datasets
                else // index only with tags
                { 
                    ids = dm.GetDatasetIdsWithTag();
                }
               
                IList<long> ids_rev = ids.Reverse().ToList();

                
                    foreach (var id in ids_rev)
                    {
                        try
                        {
                            string doi = "";
                            string entityTemplate = "";
                            string date = "";
                            XmlDocument metadata = null;
                            DatasetVersion version = null;
                            

                            // set system fields
                            if (version != null)
                            {
                                date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                                entityTemplate = version.Dataset.EntityTemplate.Name;
                            }

                            writeBexisIndex(id, onlyReleasedTags, dm);
                            //GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(string.Format("Enountered a probelm indexing dataset '{0}'. Details: {1}", id, ex.Message));
                        }
                    }
                    //GC.Collect();

                    indexWriter.Optimize();
                    autoCompleteIndexWriter.Optimize();

                    if (!reIndex)
                    {
                        indexWriter.Dispose();
                        autoCompleteIndexWriter.Dispose();
                    }
                    if (errors.Count > 0)
                        throw new Exception(string.Join("\n\r", errors));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
          
                GC.Collect();

                using (var emailService = new EmailService())
                {
                    emailService.Send(MessageHelper.GetSearchReIndexHeader(),
                        MessageHelper.GetSearchReIndexMessage(errors),
                        GeneralSettings.SystemEmail);
                }
                    
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dsVersionTuples"></param>
        /// <param name="sds"></param>
        /// <returns></returns>
        private List<string> generateStringFromTuples(IEnumerable<AbstractTuple> dsVersionTuples, StructuredDataStructure sds)
        {
            using (var uow = this.GetUnitOfWork())
            {
                try
                {
                    if (dsVersionTuples.Count() > 0)
                    {
                        List<string> generatedStrings = new List<string>();
                        foreach (var tuple in dsVersionTuples)
                        {
                            tuple.Materialize();

                            foreach (var vv in tuple.VariableValues)
                            {
                                if (vv.VariableId > 0)
                                {
                                    Variable varr = sds.Variables.Where(p => p.Id == vv.VariableId).SingleOrDefault();
                                    switch (varr.DataType.SystemType)
                                    {
                                        case "String":
                                            {
                                                if (vv.Value != null && !String.IsNullOrEmpty(vv.Value.ToString()))
                                                {
                                                    generatedStrings.Add(vv.Value.ToString());
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                break;
                                            }
                                    }
                                }
                            }
                        }

                        foreach (var variableId in sds.Variables.Select(v => v.Id))
                        {
                            var variable = uow.GetReadOnlyRepository<VariableInstance>().Get(variableId);

                            generatedStrings.Add(variable.VariableTemplate.Label);
                            generatedStrings.Add(variable.Label);
                            if (!string.IsNullOrEmpty(variable.Description))
                                generatedStrings.Add(variable.Description);
                        }

                        return generatedStrings;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private List<string> getAllStringValuesFromTable(DataTable dataTable)
        {
            List<string> tmp = new List<string>();

            // get list of index, where a itemarray is a string
            List<int> indexes = new List<int>();

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn dc = dataTable.Columns[i];
                if (dc.DataType.Equals(typeof(string))) indexes.Add(i);
            }

            foreach (var index in indexes)
            {
                tmp.AddRange(dataTable.AsEnumerable().Select(s => s.Field<string>(index)).ToArray<string>());
            }

            tmp = tmp.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            return tmp;
        }

        private List<string> getListOfValuesFromDataStructure(StructuredDataStructure structuredDataStructure)
        {
            List<string> tmp = new List<string>();

            foreach (var variable in structuredDataStructure.Variables)
            {
                if (variable.VariableTemplate != null)
                {
                    tmp.Add(variable.VariableTemplate.Label);
                    if (!string.IsNullOrEmpty(variable.VariableTemplate.Description) && variable.VariableTemplate.Description != "Unknown")
                        tmp.Add(variable.VariableTemplate.Description);
                }

                tmp.Add(variable.Label);
                tmp.Add(variable.Description);
            }

            return tmp;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void ReIndex(bool onlyReleasedTags)
        {
            reIndex = true;
            this.Index(onlyReleasedTags);
            SearchProvider.Providers.Values.Where(p => p.IsAlive).ToList().ForEach(p => ((SearchProvider)p.Target).Reload());
            IndexReader _Reader = indexWriter.GetReader().Reopen();

            if (BexisIndexSearcher.searcher != null)
            {
                if (BexisIndexSearcher.searcher.IndexReader != null) BexisIndexSearcher.searcher?.IndexReader?.Dispose();
                BexisIndexSearcher.searcher.Dispose();
            }

            BexisIndexSearcher.searcher = new IndexSearcher(_Reader);
            BexisIndexSearcher._Reader = _Reader;
            indexWriter.GetReader().Dispose();
            indexWriter.Dispose();

            IndexReader _ReaderAutocomplete = autoCompleteIndexWriter.GetReader().Reopen();
            BexisIndexSearcher.autoCompleteSearcher.IndexReader.Dispose();
            BexisIndexSearcher.autoCompleteSearcher.Dispose();
            BexisIndexSearcher.autoCompleteSearcher = new IndexSearcher(_ReaderAutocomplete);
            BexisIndexSearcher.autoCompleteIndexReader = _ReaderAutocomplete;

            autoCompleteIndexWriter.GetReader().Dispose();
            autoCompleteIndexWriter.Dispose();
            reIndex = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        /// <return></return>
        private void writeBexisIndex(long id, bool onlyReleasedTags, DatasetManager dm)
        {
           

            string docId = id.ToString();//metadata.GetElementsByTagName("bgc:id")[0].InnerText;
            string doi  = "";
            string date = "";
            string entityTemplate = "";
            DatasetVersion version = null; 
            XmlDocument metadata = null;

                if (onlyReleasedTags)
                {
                    var latestTag = dm.GetLatestTag(id, true);
                    if (latestTag != null)
                    {
                        version = dm.GetLatestVersionByTagNr(id, latestTag.Nr);
                        metadata = version.Metadata;
                    }
                }
                else
                {
                    version = dm.GetDatasetLatestVersion(id);
                    metadata = dm.GetDatasetLatestMetadataVersion(id);
                }

                if (version != null)
                {
                    // doi
                    entityTemplate = version.Dataset.EntityTemplate.Name;
                    date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                    if(date == null) version.CreationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                    if (date == null) date = "";
                }

                var dataset = new Document();
                List<XmlNode> facetNodes = facetXmlNodeList;
                dataset.Add(new Field("doc_id", docId, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                ///
                /// Add a field to indicte whether the dataset is public, this will be used for the public datasets' search page.
                ///
                dataset.Add(new Field("gen_isPublic", entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result ? "TRUE" : "FALSE", Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                var entityName = xmlDatasetHelper.GetEntityName(id);
                dataset.Add(new Field("gen_entity_name", entityName, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                dataset.Add(new Field("gen_doi", doi, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                dataset.Add(new Field("gen_modifieddate", date, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                dataset.Add(new Field("gen_entitytemplate", entityTemplate, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                indexSystemInfos(id, doi, entityName, entityTemplate, ref dataset, docId);


                foreach (XmlNode facet in facetNodes)
                {
                    String multivalued = facet.Attributes.GetNamedItem("multivalued").Value;
                    string[] metadataElementNames = facet.Attributes.GetNamedItem("metadata_name").Value.Split(',');
                    String lucene_name = facet.Attributes.GetNamedItem("lucene_name").Value;

                    foreach (string metadataElementName in metadataElementNames)
                    {
                        if (!String.IsNullOrEmpty(metadataElementName))
                        {
                            XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                            if (elemList != null)
                            {
                                for (int i = 0; i < elemList.Count; i++)
                                {
                                    string eleme = elemList[i].InnerText;
                                    if (!elemList[i].InnerText.Trim().Equals(""))
                                    {
                                        dataset = write_data_facet(facet, elemList[i].InnerText.Trim(), dataset, docId, metadataElementName);
                                    }
                                }
                            }
                        }
                    }
                }

                List<XmlNode> propertyNodes = propertyXmlNodeList;
                foreach (XmlNode property in propertyNodes)
                {
                    String multivalued = property.Attributes.GetNamedItem("multivalued").Value;
                    String lucene_name = property.Attributes.GetNamedItem("lucene_name").Value;
                    string[] metadataElementNames = property.Attributes.GetNamedItem("metadata_name").Value.Split(',');

                    foreach (string metadataElementName in metadataElementNames)
                    {
                        XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                        if (elemList != null)
                        {
                            String primitiveType = property.Attributes.GetNamedItem("primitive_type").Value;
                            if (elemList[0] != null)
                            {
                                if (primitiveType.ToLower().Equals("string"))
                                {
                                    dataset.Add(new Field("property_" + lucene_name, elemList[0].InnerText,
                                        Lucene.Net.Documents.Field.Store.YES, Field.Index.NOT_ANALYZED));
                                    dataset.Add(new Field("ng_all", elemList[0].InnerText,
                                        Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
                                    writeAutoCompleteIndex(docId, lucene_name, elemList[0].InnerText);
                                    writeAutoCompleteIndex(docId, "ng_all", elemList[0].InnerText);
                                }
                                else if (primitiveType.ToLower().Equals("date"))
                                {
                                    //DateTime MyDateTime = DateTime.Now;
                                    DateTime MyDateTime = new DateTime();
                                    /*String dTFormatElementName = property.Attributes.GetNamedItem("date_format").Value;
                                XmlNodeList dtFormatElements = metadata.GetElementsByTagName(dTFormatElementName);
                                String dateTimeFormat = dtFormatElements[0].InnerText;*/

                                    if (DateTime.TryParse(elemList[0].InnerText, out MyDateTime))
                                    {
                                        //MyDateTime = DateTime.ParseExact(elemList[0].InnerText, dateTimeFormat,
                                        //            CultureInfo.InvariantCulture);
                                        long t = MyDateTime.Ticks;

                                        NumericField xyz =
                                            new NumericField("property_numeric_" + lucene_name).SetLongValue(
                                                MyDateTime.Ticks);
                                        String dateToString = MyDateTime.Date.ToString("d",
                                            CultureInfo.CreateSpecificCulture("en-US"));
                                        dataset.Add(xyz);
                                        dataset.Add(new Field("property_" + lucene_name, dateToString,
                                            Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));

                                        writeAutoCompleteIndex(docId, lucene_name, MyDateTime.Date.ToString());
                                        writeAutoCompleteIndex(docId, "ng_all", MyDateTime.Date.ToString());
                                    }
                                }
                                else if (primitiveType.ToLower().Equals("integer"))
                                {
                                    dataset.Add(
                                        new NumericField("property_numeric" + lucene_name).SetIntValue(
                                            Convert.ToInt32(elemList[0].InnerText)));
                                    dataset.Add(new Field("property_" + lucene_name, elemList[0].InnerText,
                                        Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
                                    //  writeAutoCompleteIndex(lucene_name, elemList[0].InnerText);
                                }
                                else if (primitiveType.ToLower().Equals("double"))
                                {
                                    dataset.Add(
                                        new NumericField("property_numeric" + lucene_name).SetDoubleValue(
                                            Convert.ToDouble(elemList[0].InnerText)));
                                    dataset.Add(new Field("property_" + lucene_name, elemList[0].InnerText,
                                        Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
                                    writeAutoCompleteIndex(docId, lucene_name, elemList[0].InnerText);
                                    writeAutoCompleteIndex(docId, "ng_all", elemList[0].InnerText);
                                }
                            }
                        }
                    }
                }
                List<XmlNode> categoryNodes = categoryXmlNodeList;

                // add categories to index
                foreach (XmlNode category in categoryNodes)
                {
                    String primitiveType = category.Attributes.GetNamedItem("primitive_type").Value;
                    String lucene_name = category.Attributes.GetNamedItem("lucene_name").Value;
                    String analysing = category.Attributes.GetNamedItem("analysed").Value;
                    float boosting = Convert.ToSingle(category.Attributes.GetNamedItem("boost").Value);
                    var toAnalyse = Lucene.Net.Documents.Field.Index.NOT_ANALYZED;

                    if (analysing.ToLower().Equals("yes"))
                    {
                        toAnalyse = Lucene.Net.Documents.Field.Index.ANALYZED;
                    }

                    if (!category.Attributes.GetNamedItem("type").Value.Equals("primary_data_field"))
                    {
                        String multivalued = category.Attributes.GetNamedItem("multivalued").Value;
                        String storing = category.Attributes.GetNamedItem("store").Value;

                        var toStore = Lucene.Net.Documents.Field.Store.NO;
                        if (storing.ToLower().Equals("yes"))
                        {
                            toStore = Lucene.Net.Documents.Field.Store.YES;
                        }

                        string[] metadataElementNames = category.Attributes.GetNamedItem("metadata_name").Value.Split(',');

                        foreach (string metadataElementName in metadataElementNames)
                        {
                            XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                            if (elemList != null)
                            {
                                for (int i = 0; i < elemList.Count; i++)
                                {
                                    Field a = new Field("category_" + lucene_name, elemList[i].InnerText, toStore, toAnalyse);
                                    a.Boost = boosting;
                                    dataset.Add(a);
                                    dataset.Add(new Field("ng_" + lucene_name, elemList[i].InnerText,
                                        Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                                    dataset.Add(new Field("ng_all", elemList[i].InnerText,
                                        Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                                    writeAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                                    writeAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                                }
                            }
                        }
                    }
                    else
                    {
                        //if the primary data index exist in the config - this means the primary data should be indexed
                        includePrimaryData = true;
                    }
                }

                indexPrimaryData(id, categoryNodes, ref dataset, docId, metadata);

                List<XmlNode> generalNodes = generalXmlNodeList;

                foreach (XmlNode general in generalNodes)
                {
                    String multivalued = general.Attributes.GetNamedItem("multivalued").Value;
                    String primitiveType = general.Attributes.GetNamedItem("primitive_type").Value;
                    String lucene_name = general.Attributes.GetNamedItem("lucene_name").Value;

                    String storing = general.Attributes.GetNamedItem("store").Value;
                    String analysing = general.Attributes.GetNamedItem("analysed").Value;

                    var toStore = Lucene.Net.Documents.Field.Store.NO;
                    var toAnalyse = Lucene.Net.Documents.Field.Index.NOT_ANALYZED;

                    if (storing.ToLower().Equals("yes"))
                    {
                        toStore = Lucene.Net.Documents.Field.Store.YES;
                    }
                    if (analysing.ToLower().Equals("yes"))
                    {
                        toAnalyse = Lucene.Net.Documents.Field.Index.ANALYZED;
                    }
                    float boosting = Convert.ToSingle(general.Attributes.GetNamedItem("boost").Value);

                    string[] metadataElementNames = general.Attributes.GetNamedItem("metadata_name").Value.Split(',');

                    foreach (string metadataElementName in metadataElementNames)
                    {
                        XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                        for (int i = 0; i < elemList.Count; i++)
                        {
                            if (!elemList[i].InnerText.Trim().Equals(""))
                            {
                                Field a = new Field(lucene_name, elemList[i].InnerText, toStore, toAnalyse);
                                a.Boost = boosting;
                                dataset.Add(a);
                                dataset.Add(new Field("ng_all", elemList[i].InnerText, Lucene.Net.Documents.Field.Store.NO, Field.Index.ANALYZED));
                                writeAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                                writeAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                            }
                        }
                    }
                }

                indexWriter.AddDocument(dataset);

            
        }

        private void indexPrimaryData(long id, List<XmlNode> categoryNodes, ref Document dataset, string docId, XmlDocument metadata)
        {
            using (DatasetManager dm = new DatasetManager())
            using (DataStructureManager dsm = new DataStructureManager())
            {
                if (!dm.IsDatasetCheckedIn(id))
                    return;

                DatasetVersion dsv = dm.GetDatasetLatestVersion(id);

                // if dataset has no structure -> nothing to do
                if (dsv.Dataset.DataStructure == null) return;

                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                if (sds == null)
                    return;

                indexStructureDataStructcure(sds, ref dataset, docId);

                if (!includePrimaryData)
                    return;

                // Javad: check if the dataset is "checked-in". If yes, then use the paging version of the GetDatasetVersionEffectiveTuples method
                // number of tuples for the for loop is also available via GetDatasetVersionEffectiveTupleCount
                // a proper fetch (page) size can be obtained by calling dm.PreferedBatchSize
                int fetchSize = dm.PreferedBatchSize;
                long tupleSize = dm.GetDatasetVersionEffectiveTupleCount(dsv);
                long noOfFetchs = tupleSize / fetchSize + 1;

                if (tupleSize > 0)
                {
                    for (int round = 0; round < noOfFetchs; round++)
                    {
                        List<string> primaryDataStringToindex = null;
                        using (DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, round, fetchSize))
                        {
                            primaryDataStringToindex = getAllStringValuesFromTable(table); // should take the table
                            table.Dispose();
                        }

                        foreach (XmlNode category in categoryNodes)
                        {
                            String primitiveType = category.Attributes.GetNamedItem("primitive_type").Value;
                            String lucene_name = category.Attributes.GetNamedItem("lucene_name").Value;
                            String analysing = category.Attributes.GetNamedItem("analysed").Value;
                            float boosting = Convert.ToSingle(category.Attributes.GetNamedItem("boost").Value);
                            var toAnalyse = Lucene.Net.Documents.Field.Index.NOT_ANALYZED;

                            if (analysing.ToLower().Equals("yes"))
                            {
                                toAnalyse = Lucene.Net.Documents.Field.Index.ANALYZED;
                            }

                            if (category.Attributes.GetNamedItem("type").Value.Equals("primary_data_field"))
                            {
                                if (primaryDataStringToindex != null && primaryDataStringToindex.Count > 0)
                                {
                                    primaryDataStringToindex = primaryDataStringToindex.Distinct().ToList();
                                    foreach (string pDataValue in primaryDataStringToindex)
                                    // Loop through List with foreach
                                    {
                                        Field a = new Field("category_" + lucene_name, pDataValue,
                                            Lucene.Net.Documents.Field.Store.NO, toAnalyse);
                                        a.Boost = boosting;
                                        dataset.Add(a);
                                        dataset.Add(new Field("ng_" + lucene_name, pDataValue,
                                            Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                                        dataset.Add(new Field("ng_all", pDataValue, Lucene.Net.Documents.Field.Store.YES,
                                            Lucene.Net.Documents.Field.Index.ANALYZED));
                                        writeAutoCompleteIndex(docId, lucene_name, pDataValue);
                                        writeAutoCompleteIndex(docId, "ng_all", pDataValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void indexStructureDataStructcure(StructuredDataStructure sds, ref Document dataset, string docId)
        {
            if (sds == null)
                return;

            List<string> sdsStrings = getListOfValuesFromDataStructure(sds);

            String primitiveType = "string";
            String lucene_name = "data_structure_field";
            String analysing = "yes";
            float boosting = 3;
            var toAnalyse = Lucene.Net.Documents.Field.Index.NOT_ANALYZED;

            foreach (string pDataValue in sdsStrings)
            // Loop through List with foreach
            {
                Field a = new Field("category_" + lucene_name, pDataValue,
                    Lucene.Net.Documents.Field.Store.NO, toAnalyse);
                a.Boost = boosting;
                dataset.Add(a);
                dataset.Add(new Field("ng_" + lucene_name, pDataValue,
                    Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                dataset.Add(new Field("ng_all", pDataValue, Lucene.Net.Documents.Field.Store.YES,
                    Lucene.Net.Documents.Field.Index.ANALYZED));
                writeAutoCompleteIndex(docId, lucene_name, pDataValue);
                writeAutoCompleteIndex(docId, "ng_all", pDataValue);
            }
        }

        private void indexSystemInfos(long id, string doi, string entity, string template, ref Document dataset, string docId)
        {
   
            String primitiveType = "string";
            String analysing = "yes";
            float boosting = 3;
            var toAnalyse = Lucene.Net.Documents.Field.Index.NOT_ANALYZED;


            // id
            string pDataValue = id.ToString();
            string key = "id";
            Field a = new Field("category_"+ key, pDataValue, Lucene.Net.Documents.Field.Store.NO, toAnalyse);
            a.Boost = boosting;
            dataset.Add(a);
            dataset.Add(new Field("ng_id", pDataValue,
                Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
            dataset.Add(new Field("ng_all", pDataValue, Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.ANALYZED));
            writeAutoCompleteIndex(docId, key, pDataValue);
            writeAutoCompleteIndex(docId, "ng_all", pDataValue);

            // doi
            pDataValue = doi;
            key = "doi";
            a = new Field("category_"+key, pDataValue, Lucene.Net.Documents.Field.Store.NO, toAnalyse);
            a.Boost = boosting;
            dataset.Add(a);
            dataset.Add(new Field("ng_" + key, pDataValue,
                Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
            dataset.Add(new Field("ng_all", pDataValue, Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.ANALYZED));
            writeAutoCompleteIndex(docId, key, pDataValue);
            writeAutoCompleteIndex(docId, "ng_all", pDataValue);

            // entity
            pDataValue = entity;
            key = "entity";
            a = new Field("category_" + key, pDataValue, Lucene.Net.Documents.Field.Store.NO, toAnalyse);
            a.Boost = boosting;
            dataset.Add(a);
            dataset.Add(new Field("ng_" + key, pDataValue,
                Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
            dataset.Add(new Field("ng_all", pDataValue, Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.ANALYZED));
            writeAutoCompleteIndex(docId, key, pDataValue);
            writeAutoCompleteIndex(docId, "ng_all", pDataValue);

            // entity template
            pDataValue = template;
            key = "template";
            a = new Field("category_" + key, pDataValue, Lucene.Net.Documents.Field.Store.NO, toAnalyse);
            a.Boost = boosting;
            dataset.Add(a);
            dataset.Add(new Field("ng_"+key, pDataValue,
                Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
            dataset.Add(new Field("ng_all", pDataValue, Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.ANALYZED));
            writeAutoCompleteIndex(docId, key, pDataValue);
            writeAutoCompleteIndex(docId, "ng_all", pDataValue);

        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void updateIndex(Dictionary<long, IndexingAction> datasetsToIndex, bool onlyReleasedTags)
        {
            using (DatasetManager dm = new DatasetManager())
            {
                try
                {
                    if (!isIndexConfigured)
                    {
                        this.configureBexisIndexing(false);
                    }
                    foreach (KeyValuePair<long, IndexingAction> pair in datasetsToIndex)
                    {
                        DatasetVersion version = dm.GetDatasetLatestVersion(pair.Key);
                        string doi = "";
                        string entityTemplate = "";
                        string date = "";

                        if (version != null)
                        { 
                            // doi
                            entityTemplate = version.Dataset.EntityTemplate.Name;
                            date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                        }

                        if (pair.Value == IndexingAction.CREATE)
                        {
                            Query query = new TermQuery(new Term("doc_id", pair.Key.ToString()));
                            TopDocs tds = BexisIndexSearcher.getIndexSearcher().Search(query, 1);

                            if (tds.TotalHits < 1) { writeBexisIndex(pair.Key, onlyReleasedTags, dm); }
                            else
                            {
                                indexWriter.DeleteDocuments(new Term("doc_id", pair.Key.ToString()));
                                autoCompleteIndexWriter.DeleteDocuments(new Term("id", pair.Key.ToString()));
                                writeBexisIndex(pair.Key, onlyReleasedTags, dm);
                            }
                        }
                        else if (pair.Value == IndexingAction.DELETE)
                        {
                            indexWriter.DeleteDocuments(new Term("doc_id", pair.Key.ToString()));
                            autoCompleteIndexWriter.DeleteDocuments(new Term("id", pair.Key.ToString()));
                        }
                        else if (pair.Value == IndexingAction.UPDATE)
                        {
                            indexWriter.DeleteDocuments(new Term("doc_id", pair.Key.ToString()));
                            autoCompleteIndexWriter.DeleteDocuments(new Term("id", pair.Key.ToString()));
                            writeBexisIndex(pair.Key, onlyReleasedTags, dm);
                        }
                    }
                    indexWriter.Commit();
                    autoCompleteIndexWriter.Commit();
                    BexisIndexSearcher.searcher = new IndexSearcher(indexWriter.GetReader());
                    BexisIndexSearcher._Reader = indexWriter.GetReader();
                    BexisIndexSearcher.autoCompleteSearcher = new IndexSearcher(autoCompleteIndexWriter.GetReader());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    autoCompleteIndexWriter.Dispose();
                    indexWriter.Dispose();

                    BexisIndexSearcher.searcher = new IndexSearcher(indexWriter.GetReader());
                    BexisIndexSearcher.autoCompleteSearcher = new IndexSearcher(autoCompleteIndexWriter.GetReader());
                }
            }
        }

        public void updateSingleDatasetIndex(long datasetId, IndexingAction indAction,bool onlyReleasedTags)
        {
            using (DatasetManager dm = new DatasetManager())
            {
                try
                {

                    if (!isIndexConfigured)
                    {
                        this.configureBexisIndexing(false);
                    }

                    if (indAction == IndexingAction.CREATE)
                    {
                        Query query = new TermQuery(new Term("doc_id", datasetId.ToString()));
                        TopDocs tds = BexisIndexSearcher.getIndexSearcher().Search(query, 1);

                        this.includePrimaryData = false;

                        if (tds.TotalHits < 1) { writeBexisIndex(datasetId, onlyReleasedTags, dm); }
                        else
                        {
                            indexWriter.DeleteDocuments(new Term("doc_id", datasetId.ToString()));
                            autoCompleteIndexWriter.DeleteDocuments(new Term("id", datasetId.ToString()));
                            writeBexisIndex(datasetId, onlyReleasedTags, dm);
                        }
                    }
                    else if (indAction == IndexingAction.DELETE)
                    {
                        indexWriter.DeleteDocuments(new Term("doc_id", datasetId.ToString()));
                        autoCompleteIndexWriter.DeleteDocuments(new Term("id", datasetId.ToString()));
                    }
                    else if (indAction == IndexingAction.UPDATE)
                    {
                        indexWriter.DeleteDocuments(new Term("doc_id", datasetId.ToString()));
                        autoCompleteIndexWriter.DeleteDocuments(new Term("id", datasetId.ToString()));
                        writeBexisIndex(datasetId, onlyReleasedTags, dm);
                    }

                    indexWriter.Commit();
                    autoCompleteIndexWriter.Commit();
                    BexisIndexSearcher.searcher = new IndexSearcher(indexWriter.GetReader());
                    BexisIndexSearcher._Reader = indexWriter.GetReader();
                    BexisIndexSearcher.autoCompleteSearcher = new IndexSearcher(autoCompleteIndexWriter.GetReader());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    BexisIndexSearcher.searcher = new IndexSearcher(indexWriter.GetReader());
                    BexisIndexSearcher.autoCompleteSearcher = new IndexSearcher(autoCompleteIndexWriter.GetReader());

                    indexWriter.Dispose();
                    autoCompleteIndexWriter.Dispose();
                }
            }
        }

        private Document write_data_facet(XmlNode facet, object val, Document dataset, string docId, string variable_node_Label)
        {
            if (facet.Attributes.GetNamedItem("primitive_type")?.Value.ToLower() == "string")
            {
                dataset.Add(new Field("facet_" + facet.Attributes.GetNamedItem("lucene_name").Value, val.ToString(),
                    Lucene.Net.Documents.Field.Store.YES, Field.Index.NOT_ANALYZED));
                dataset.Add(new Field("ng_all", val.ToString(),
                    Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
                writeAutoCompleteIndex(docId, facet.Attributes.GetNamedItem("lucene_name").Value, val.ToString());
                writeAutoCompleteIndex(docId, "ng_all", val.ToString());
            }
            else
            {
                if (facet.Attributes.GetNamedItem("primitive_type")?.Value.ToLower() == "date")
                {
                    DateTime dateValue = DateTime.MinValue;
                    DateTime dateValue_ = dateValue;

                    DateTime.TryParse(val.ToString(), out dateValue);
                    if ((dateValue != dateValue_) && (!string.IsNullOrEmpty(val.ToString())))
                    {
                        Field newField_ = new Field("facet_" + facet.Attributes.GetNamedItem("lucene_name").Value, DateTools.DateToString(dateValue, DateTools.Resolution.DAY), Field.Store.YES, Field.Index.ANALYZED);
                        dataset.Add(newField_);
                    }
                    else
                    {
                        Debug_EMpty_nodes(variable_node_Label, " ", docId);
                    }
                }
                else if ((facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("int")) ||
                    (facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("double")) ||
                    (facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("decimal")) ||
                    (facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("number")))
                {
                    NumericField newField_ = new NumericField("facet_" + facet.Attributes.GetNamedItem("lucene_name").Value, Field.Store.YES, true).SetDoubleValue(double.Parse(val.ToString()));
                    if (!string.IsNullOrEmpty(val.ToString()))
                        dataset.Add(newField_);
                    else
                    {
                        Debug_EMpty_nodes(variable_node_Label, " ", docId);
                    }
                }
            }
            return dataset;
        }

        private void Debug_EMpty_nodes(string Extracted_node_path, string type, string datasetId)
        {
            LoggerFactory.GetFileLogger().LogCustom("==> Dataset ID: " + datasetId + " has Empty value for the " + type + " named : " + Extracted_node_path);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="docId"></param>
        /// <param name="f"></param>
        /// <param name="V"></param>
        /// <return></return>
        private void writeAutoCompleteIndex(String docId, String f, String V)
        {
            /*
             * this line was commented out because it is very time intensive. after tests no further problems were found
             */
            //autoCompleteIndexWriter.GetReader().Reopen();

            var dataset = new Document();
            dataset.Add(new Field("id", docId.ToLower(), Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
            dataset.Add(new Field("field", f.ToLower(), Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
            dataset.Add(new Field("value", V.ToLower(), Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
            autoCompleteIndexWriter.AddDocument(dataset);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void Dispose()
        {
            indexWriter?.Dispose();
            autoCompleteIndexWriter?.Dispose();
        }
    }
}