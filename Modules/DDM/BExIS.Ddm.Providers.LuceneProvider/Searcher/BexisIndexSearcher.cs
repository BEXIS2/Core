using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Indexer;
using BExIS.Dlm.Services.Data;
using BExIS.Utils.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HeaderItem = BExIS.Utils.Models.HeaderItem;
using Row = BExIS.Utils.Models.Row;
using SearchResult = BExIS.Utils.Models.SearchResult;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider.Searcher
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public static class BexisIndexSearcher
    {
        private static string luceneIndexPath = Path.Combine(FileHelper.IndexFolderPath, "BexisSearchIndex");
        private static string autoCompleteIndexPath = Path.Combine(FileHelper.IndexFolderPath, "BexisAutoComplete");

        private static Lucene.Net.Store.Directory pathIndex = FSDirectory.Open(new DirectoryInfo(luceneIndexPath));
        private static Lucene.Net.Store.Directory autoCompleteIndex = FSDirectory.Open(new DirectoryInfo(autoCompleteIndexPath));
        public static IndexReader _Reader = IndexReader.Open(pathIndex, true);
        public static IndexSearcher searcher = new IndexSearcher(_Reader);

        public static IndexReader autoCompleteIndexReader = IndexReader.Open(autoCompleteIndex, true);
        public static IndexSearcher autoCompleteSearcher = new IndexSearcher(autoCompleteIndexReader);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string[] facetFields { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string[] storedFields { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string[] categoryFields { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string[] propertyFields { get; set; }

        private static Boolean isInit = false;
        private static XmlDocument configXML;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        /// <returns></returns>
        public static string[] getCategoryFields()
        { init(); return categoryFields; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        /// <returns></returns>
        public static string[] getStoredFields()
        { init(); return storedFields; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public static IndexReader getIndexReader()
        {
            init();
            return _Reader;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public static IndexSearcher getIndexSearcher()
        {
            init();
            return searcher;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        private static void init()
        {
            if (!isInit) { BexisIndexSearcherInit(); isInit = true; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        private static void BexisIndexSearcherInit()
        {
            List<string> facetFieldList = new List<string>();
            List<string> categoryFieldList = new List<string>();
            List<string> propertyFieldList = new List<string>();
            List<string> storedFieldList = new List<string>();

            configXML = new XmlDocument();
            configXML.Load(FileHelper.ConfigFilePath);
            XmlNodeList fieldProperties = configXML.GetElementsByTagName("field");

            foreach (XmlNode fieldProperty in fieldProperties)
            {
                String metadataIndexingType = fieldProperty.Attributes.GetNamedItem("type").Value;
                String metadataIndexingStore = fieldProperty.Attributes.GetNamedItem("store").Value;
                if (metadataIndexingType.ToLower().Equals("category_field")) { categoryFieldList.Add("category_" + fieldProperty.Attributes.GetNamedItem("lucene_name").Value); }
                else if (metadataIndexingType.ToLower().Equals("property_field")) { propertyFieldList.Add("property_" + fieldProperty.Attributes.GetNamedItem("lucene_name").Value); }
                else if (metadataIndexingType.ToLower().Equals("facet_field")) { facetFieldList.Add("facet_" + fieldProperty.Attributes.GetNamedItem("lucene_name").Value); }
                if (metadataIndexingStore.ToLower().Equals("yes")) { storedFieldList.Add(fieldProperty.Attributes.GetNamedItem("lucene_name").Value); }
            }
            storedFields = storedFieldList.ToArray();
            facetFields = facetFieldList.ToArray();
            propertyFields = propertyFieldList.ToArray();
            categoryFields = categoryFieldList.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="query"></param>
        /// <param name="headerItemXmlNodeList"></param>
        /// <returns></returns>
        public static SearchResult search(Query query, List<XmlNode> headerItemXmlNodeList)
        {
            int n = 0;
            DatasetManager dm = null;
            try
            {
                dm = new DatasetManager();
                n = dm.DatasetRepo.Get().Count;

                if (n == 0) n = 1000;
            }
            catch
            {
                n = 1000;
            }
            finally
            {
                dm.Dispose();
                if (n <= 0)
                    n = 1000;
            }
            TopDocs docs = searcher.Search(query, n);
            SearchResult sro = new SearchResult();
            sro.PageSize = 10;
            sro.CurrentPage = 1;
            sro.NumberOfHits = 100;

            List<HeaderItem> Header = new List<HeaderItem>();
            List<HeaderItem> DefaultHeader = new List<HeaderItem>();

            // create id
            HeaderItem id = new HeaderItem();
            id.DisplayName = "ID";
            id.Name = "ID";
            id.DataType = "Integer";
            sro.Id = id;
            Header.Add(id);
            DefaultHeader.Add(id);

            // create entity
            HeaderItem entity = new HeaderItem();
            entity.DisplayName = "Type";
            entity.Name = "entity_name";
            entity.DataType = "string";
            entity.Placeholder = "entity";
            Header.Add(entity);

            // create entitytemplate
            HeaderItem entitytemplate = new HeaderItem();
            entitytemplate.DisplayName = "Template";
            entitytemplate.Name = "entitytemplate";
            entitytemplate.DataType = "string";
            entitytemplate.Placeholder = "entitytemplate";
            Header.Add(entitytemplate);

            // create date
            HeaderItem modifieddate = new HeaderItem();
            modifieddate.DisplayName = "Last modified date";
            modifieddate.Name = "modifieddate";
            modifieddate.DataType = "string";
            modifieddate.Placeholder = "date";
            Header.Add(modifieddate);

            // create date
            HeaderItem doi = new HeaderItem();
            doi.DisplayName = "DOI";
            doi.Name = "doi";
            doi.DataType = "string";
            doi.Placeholder = "doi";
            Header.Add(doi);

            //DefaultHeader.Add(entity);

            foreach (XmlNode ade in headerItemXmlNodeList)
            {
                HeaderItem hi = new HeaderItem();
                hi = new HeaderItem();
                hi.Name = ade.Attributes.GetNamedItem("lucene_name").Value;
                hi.DisplayName = ade.Attributes.GetNamedItem("display_name").Value;
                hi.Placeholder = ade.Attributes.GetNamedItem("placeholder").Value;
                Header.Add(hi);

                if (ade.Attributes.GetNamedItem("default_visible_item").Value.ToLower().Equals("yes"))
                {
                    DefaultHeader.Add(hi);
                }
            }

            List<Row> RowList = new List<Row>();
            string valueLastEntity = ""; // var to store last entity value
            bool moreThanOneEntityFound = false; // var to set, if more than one entity name was found

            foreach (ScoreDoc sd in docs.ScoreDocs)
            {
                Document doc = searcher.Doc(sd.Doc);
                Row r = new Row();
                List<object> ValueList = new List<object>();
                ValueList = new List<object>();
                ValueList.Add(doc.Get("doc_id"));
                ValueList.Add(doc.Get("gen_entity_name"));
                ValueList.Add(doc.Get("gen_entitytemplate"));
                ValueList.Add(doc.Get("gen_modifieddate"));
                ValueList.Add(doc.Get("gen_doi"));

                // check if there are more than one entities in the result list
                if (moreThanOneEntityFound == false && ValueList[1].ToString() != valueLastEntity && valueLastEntity != "")
                {
                    moreThanOneEntityFound = true;
                }
                valueLastEntity = ValueList[1].ToString();

                foreach (XmlNode ade in headerItemXmlNodeList)
                {
                    String fieldType = ade.Attributes.GetNamedItem("type").Value;
                    String luceneName = ade.Attributes.GetNamedItem("lucene_name").Value;
                    if (fieldType.ToLower().Equals("facet_field"))
                    {
                        luceneName = "facet_" + luceneName;
                    }
                    else if (fieldType.ToLower().Equals("category_field"))
                    {
                        luceneName = "category_" + luceneName;
                    }
                    else if (fieldType.ToLower().Equals("property_field"))
                    {
                        luceneName = "property_" + luceneName;
                    }

                    var values = doc.GetValues(luceneName);

                    ValueList.Add(string.Join(", ",values));
                }
                r.Values = ValueList;
                RowList.Add(r);
            }

            // show column of entities, if there are more than one found
            if (moreThanOneEntityFound == true)
            {
                DefaultHeader.Add(entity);
            }

            sro.Header = Header;
            sro.DefaultVisibleHeaderItem = DefaultHeader;
            sro.Rows = RowList;
            return sro;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="origQuery"></param>
        /// <param name="queryFilter"></param>
        /// <param name="searchtext"></param>
        /// <returns></returns>
        public static IEnumerable<TextValue> doTextSearch(Query origQuery, String queryFilter, String searchtext)
        {
            using (Analyzer analyzer = new NGramAnalyzer())
            using (KeywordAnalyzer ka = new KeywordAnalyzer())
            {
                String filter = queryFilter;
                BooleanQuery query = new BooleanQuery();
                query.Add(origQuery, Occur.MUST);
                if (!filter.ToLower().StartsWith("ng_"))
                {
                    filter = "ng_" + filter;
                }
                if (filter.ToLower().Equals("ng_all"))
                {
                    filter = "ng_all";
                    queryFilter = "ng_all";
                }
                HashSet<string> uniqueText = new HashSet<string>();
                searchtext = searchtext.ToLower();
                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, filter, ka);
                parser.DefaultOperator = QueryParser.Operator.AND;
                Query X1 = parser.Parse(searchtext);
                query.Add(X1, Occur.MUST);
                // Query query = parser.Parse("tree data");
                TopDocs tds = searcher.Search(query, 50);
                QueryScorer scorer = new QueryScorer(query, searchtext);

                List<TextValue> autoCompleteTextList = new List<TextValue>();
                foreach (ScoreDoc sd in tds.ScoreDocs)
                {
                    Document doc = searcher.Doc(sd.Doc);
                    String docId = doc.GetField("doc_id").StringValue;
                    TermQuery q1 = new TermQuery(new Term("id", docId.ToLower()));
                    TermQuery q0 = new TermQuery(new Term("field", queryFilter.ToLower()));
                    QueryParser parser1 = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "value", ka);
                    parser1.DefaultOperator = QueryParser.Operator.AND;
                    Query q2 = parser1.Parse(searchtext);
                    BooleanQuery q3 = new BooleanQuery();
                    q3.Add(q1, Occur.MUST);
                    q3.Add(q2, Occur.MUST);
                    q3.Add(q0, Occur.MUST);
                    TopDocs tdAutoComp = autoCompleteSearcher.Search(q3, 100);
                    foreach (ScoreDoc sdAutoComp in tdAutoComp.ScoreDocs)
                    {
                        Document docAutoComp = autoCompleteSearcher.Doc(sdAutoComp.Doc);
                        String toAdd = docAutoComp.GetField("value").StringValue;
                        if (!uniqueText.Contains(toAdd))
                        {
                            TextValue tv = new TextValue();
                            tv.Name = toAdd;
                            tv.Value = toAdd;
                            autoCompleteTextList.Add(tv);
                            uniqueText.Add(toAdd);
                        }
                    }

                    if (autoCompleteTextList.Count > 7) break;
                }
                return autoCompleteTextList;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="query"></param>
        /// <param name="facets"></param>
        /// <returns></returns>
        public static IEnumerable<Facet> facetSearch(Query query, IEnumerable<Facet> facets)
        {
            List<Facet> l = new List<Facet>();
            foreach (Facet f in facets)
            {
                Facet c = new Facet();
                c.Name = f.Name;
                c.Text = f.Text;
                c.Value = f.Value;
                c.DisplayName = f.DisplayName;
                c.Childrens = new List<Facet>();
                using (SimpleFacetedSearch sfs = new SimpleFacetedSearch(_Reader, new string[] { "facet_" + f.Name }))
                {
                    SimpleFacetedSearch.Hits hits = sfs.Search(query);
                    int cCount = 0;
                    foreach (SimpleFacetedSearch.HitsPerFacet hpg in hits.HitsPerFacet)
                    {
                        if (!hpg.Name.ToString().Equals(""))
                        {
                            Facet cc = new Facet();
                            cc.Name = hpg.Name.ToString();
                            cc.Text = hpg.Name.ToString();
                            cc.Value = hpg.Name.ToString();
                            cc.Count = (int)hpg.HitCount;
                            if (cc.Count > 0)
                            {
                                cCount++;
                                foreach (XmlElement item in configXML.GetElementsByTagName("field"))
                                {
                                    if ((!item.Attributes["primitive_type"].InnerText.ToLower().Contains("date")) && 
                                        (!item.Attributes["primitive_type"].InnerText.ToLower().Contains("string")) && 
                                        (item.Attributes["display_name"].InnerText.ToLower().Contains(c.DisplayName.ToLower())))
                                    {
                                        foreach (var doc in hpg.Documents)
                                        {
                                            IList<IFieldable> numericFields = doc.GetFields("facet_" + f.Name);
                                            foreach (var field in numericFields)
                                            {
                                                cc.Name = field.StringValue;
                                                cc.Text = field.StringValue;
                                                cc.Value = field.StringValue;
                                                if (!c.Childrens.Exists(x => x.Name == cc.Name)) c.Childrens.Add(cc);
                                            }
                                        }
                                    }
                                }
                                c.Childrens.Add(cc);
                            }
                        }
                    }
                    c.Count = cCount;
                    l.Add(c);
                }
            }
            return l;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="query"></param>
        /// <param name="facets"></param>
        /// <returns></returns>
        public static IEnumerable<Property> propertySearch(Query query, IEnumerable<Property> properties)
        {
            foreach (Property p in properties)
            {
                using (SimpleFacetedSearch sfs = new SimpleFacetedSearch(_Reader, new string[] { "property_" + p.Name }))
                {
                    SimpleFacetedSearch.Hits hits = sfs.Search(query);
                    int cCount = 0;

                    List<string> tmp = new List<string>();

                    foreach (SimpleFacetedSearch.HitsPerFacet hpg in hits.HitsPerFacet)
                    {
                        if (!String.IsNullOrEmpty(hpg.Name.ToString()))
                        {
                            if ((int)hpg.HitCount > 0)
                                tmp.Add(hpg.Name.ToString());
                        }
                    }

                    p.Values = tmp;
                }
            }
            return properties;
        }
    }
}