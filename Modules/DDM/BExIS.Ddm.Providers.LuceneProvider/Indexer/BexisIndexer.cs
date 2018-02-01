using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Searcher;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Utils.Models;
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

        public bool includePrimaryData = true;
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
                    c.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value; ;
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

        XmlDocument configXML;

        private void configureBexisIndexing(bool recreateIndex)
        {
            configXML = new XmlDocument();
            configXML.Load(FileHelper.ConfigFilePath);

            LoadBeforeIndexing();
            Lucene.Net.Store.Directory pathIndex = FSDirectory.Open(new DirectoryInfo(luceneIndexPath));
            Lucene.Net.Store.Directory autoCompleteIndex = FSDirectory.Open(new DirectoryInfo(autoCompleteIndexPath));

            PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper(new BexisAnalyzer());

            indexWriter = new IndexWriter(pathIndex, analyzer, recreateIndex, IndexWriter.MaxFieldLength.UNLIMITED);
            autoCompleteIndexWriter = new IndexWriter(autoCompleteIndex, new NGramAnalyzer(), recreateIndex, IndexWriter.MaxFieldLength.UNLIMITED);


            foreach (XmlNode a in categoryXmlNodeList)
            {
                analyzer.AddAnalyzer("ng_" + a.Attributes.GetNamedItem("lucene_name").Value, new NGramAnalyzer());
            }
            analyzer.AddAnalyzer("ng_all", new NGramAnalyzer());

            isIndexConfigured = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public void Index()
        {
            configureBexisIndexing(true);
            // there is no need for the metadataAccess class anymore. Talked with David and deleted. 30.18.13. Javad/ compare to the previous version to see the deletions
            DatasetManager dm = new DatasetManager();

            try
            {
                List<string> errors = new List<string>();
                IList<long> ids = dm.GetDatasetLatestIds();

                //ToDo only enitities from type dataset should be indexed in this index

                foreach (var id in ids)
                {
                    try
                    {
                        writeBexisIndex(id, dm.GetDatasetLatestMetadataVersion(id));
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
            finally
            {
                dm.Dispose();
                GC.Collect();
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
                                    switch (varr.DataAttribute.DataType.SystemType)
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
                            var variable = uow.GetReadOnlyRepository<Variable>().Get(variableId);

                            generatedStrings.Add(variable.DataAttribute.Name);
                            generatedStrings.Add(variable.Label);
                            if (!string.IsNullOrEmpty(variable.DataAttribute.Description))
                                generatedStrings.Add(variable.DataAttribute.Description);
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public void ReIndex()
        {
            reIndex = true;
            this.Index();
            SearchProvider.Providers.Values.Where(p => p.IsAlive).ToList().ForEach(p => ((SearchProvider)p.Target).Reload());
            IndexReader _Reader = indexWriter.GetReader().Reopen();
            BexisIndexSearcher.searcher.IndexReader.Dispose();
            BexisIndexSearcher.searcher.Dispose();
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
        /// <param name="metadataDoc"></param>
        /// <return></return>
        private void writeBexisIndex(long id, XmlDocument metadataDoc)
        {
            string docId = id.ToString();//metadataDoc.GetElementsByTagName("bgc:id")[0].InnerText;

            var dataset = new Document();
            List<XmlNode> facetNodes = facetXmlNodeList;
            dataset.Add(new Field("doc_id", docId, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

            foreach (XmlNode facet in facetNodes)
            {
                String multivalued = facet.Attributes.GetNamedItem("multivalued").Value;
                string[] metadataElementNames = facet.Attributes.GetNamedItem("metadata_name").Value.Split(',');
                String lucene_name = facet.Attributes.GetNamedItem("lucene_name").Value;

                foreach (string metadataElementName in metadataElementNames)
                {
                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
                    if (elemList != null)
                    {
                        for (int i = 0; i < elemList.Count; i++)
                        {
                            string eleme = elemList[i].InnerText;
                            if (!elemList[i].InnerText.Trim().Equals(""))
                            {
                                dataset.Add(new Field("facet_" + lucene_name, elemList[i].InnerText,
                                    Lucene.Net.Documents.Field.Store.YES, Field.Index.NOT_ANALYZED));
                                dataset.Add(new Field("ng_all", elemList[i].InnerText,
                                    Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
                                writeAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                                writeAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
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
                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
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
                            XmlNodeList dtFormatElements = metadataDoc.GetElementsByTagName(dTFormatElementName);
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
            if(includePrimaryData)
            {                
                indexPrimaryData(id, categoryNodes, ref dataset, docId, metadataDoc);
            }


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

                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
                    for (int i = 0; i < elemList.Count; i++)
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

            indexWriter.AddDocument(dataset);
        }

        private void indexPrimaryData(long id, List<XmlNode> categoryNodes, ref Document dataset, string docId, XmlDocument metadataDoc)
        {
            if (!includePrimaryData)
                return;

            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            if (!dm.IsDatasetCheckedIn(id))
                return;

            DatasetVersion dsv = dm.GetDatasetLatestVersion(id);
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
            if (sds == null)
                return;

            try
            {
                {
                    // Javad: check if the dataset is "checked-in". If yes, then use the paging version of the GetDatasetVersionEffectiveTuples method
                    // number of tuples for the for loop is also available via GetDatasetVersionEffectiveTupleCount
                    // a proper fetch (page) size can be obtained by calling dm.PreferedBatchSize
                    int fetchSize = dm.PreferedBatchSize;
                    long tupleSize = dm.GetDatasetVersionEffectiveTupleCount(dsv);
                    long noOfFetchs = tupleSize / fetchSize + 1;
                    for (int round = 0; round < noOfFetchs; round++)
                    {
                        List<string> primaryDataStringToindex = null;
                        using (DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, round, fetchSize))
                        {
                            //primaryDataStringToindex = generateStringFromTuples(dsVersionTuples, sds); // should take the table
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
                            else
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
                                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
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
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
            }
        }


        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public void updateIndex(Dictionary<long, IndexingAction> datasetsToIndex)
        {

            try
            {
                if (!isIndexConfigured)
                {
                    this.configureBexisIndexing(false);
                }
                foreach (KeyValuePair<long, IndexingAction> pair in datasetsToIndex)
                {
                    DatasetManager dm = new DatasetManager();

                    if (pair.Value == IndexingAction.CREATE)
                    {
                        Query query = new TermQuery(new Term("doc_id", pair.Key.ToString()));
                        TopDocs tds = BexisIndexSearcher.getIndexSearcher().Search(query, 1);

                        if (tds.TotalHits < 1) { writeBexisIndex(pair.Key, dm.GetDatasetLatestMetadataVersion(pair.Key)); }
                        else
                        {
                            indexWriter.DeleteDocuments(new Term("doc_id", pair.Key.ToString()));
                            autoCompleteIndexWriter.DeleteDocuments(new Term("id", pair.Key.ToString()));
                            writeBexisIndex(pair.Key, dm.GetDatasetLatestMetadataVersion(pair.Key));
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
                        writeBexisIndex(pair.Key, dm.GetDatasetLatestMetadataVersion(pair.Key));
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

        public void updateSingleDatasetIndex(long datasetId, IndexingAction indAction)
        {
            try
            {

                if (!isIndexConfigured)
                {
                    this.configureBexisIndexing(false);
                }
                DatasetManager dm = new DatasetManager();
                if (indAction == IndexingAction.CREATE)
                {
                    Query query = new TermQuery(new Term("doc_id", datasetId.ToString()));
                    TopDocs tds = BexisIndexSearcher.getIndexSearcher().Search(query, 1);

                    this.includePrimaryData = false;

                    if (tds.TotalHits < 1) { writeBexisIndex(datasetId, dm.GetDatasetLatestMetadataVersion(datasetId)); }
                    else
                    {
                        indexWriter.DeleteDocuments(new Term("doc_id", datasetId.ToString()));
                        autoCompleteIndexWriter.DeleteDocuments(new Term("id", datasetId.ToString()));
                        writeBexisIndex(datasetId, dm.GetDatasetLatestMetadataVersion(datasetId));
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
                    writeBexisIndex(datasetId, dm.GetDatasetLatestMetadataVersion(datasetId));
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

