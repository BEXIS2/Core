using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using BExIS.Dlm.Services.Data;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Searcher;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System.Linq;
using Lucene.Net.Search;


namespace BExIS.Ddm.Providers.LuceneProvider.Indexer
{
    public class BexisIndexer
    {
        private List<Facet> AllFacets = new List<Facet>();
        private List<Property> AllProperties = new List<Property>();
        private List<Category> AllCategories = new List<Category>();
        private bool reIndex = false;
        public List<XmlNode> facetXmlNodeList = new List<XmlNode>();
        public List<XmlNode> propertyXmlNodeList = new List<XmlNode>();
        public List<XmlNode> categoryXmlNodeList = new List<XmlNode>();
        public List<XmlNode> generalXmlNodeList = new List<XmlNode>();
        public List<XmlNode> headerItemXmlNodeList = new List<XmlNode>();

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
                    //c.Id = x.Attributes[Property.ID].InnerText;
                    c.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    c.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value; ;
                    c.DataSourceKey = fieldProperty.Attributes.GetNamedItem("metadata_name").Value;
                    c.UIComponent = fieldProperty.Attributes.GetNamedItem("uiComponent").Value; ;
                    //c.DefaultValue = fieldProperty.Attributes.GetNamedItem("default_value").Value; 
                    // c.AggregationType = fieldProperty.Attributes.GetNamedItem("type").Value; ;
                    c.AggregationType = "distinct";
                    c.DefaultValue = "All";
                    c.DataType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;
                    AllProperties.Add(c);
                }
                else if (fieldType.ToLower().Equals("category_field"))
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

        public void Index()
        {
            configXML = new XmlDocument();
            configXML.Load(FileHelper.ConfigFilePath);


            this.LoadBeforeIndexing();
            Lucene.Net.Store.Directory pathIndex = FSDirectory.Open(new DirectoryInfo(luceneIndexPath));
            Lucene.Net.Store.Directory autoCompleteIndex = FSDirectory.Open(new DirectoryInfo(autoCompleteIndexPath));

            PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper(new BexisAnalyzer());

            indexWriter = new IndexWriter(pathIndex, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            autoCompleteIndexWriter = new IndexWriter(autoCompleteIndex, new NGramAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);


            foreach (XmlNode a in categoryXmlNodeList)
            {
                analyzer.AddAnalyzer("ng_" + a.Attributes.GetNamedItem("lucene_name").Value, new NGramAnalyzer());
            }
            analyzer.AddAnalyzer("ng_all", new NGramAnalyzer());


            // there is no need for the metadataAccess class anymore. Talked with David and deleted. 30.18.13. Javad/ compare to the previous version to see the deletions
            DatasetManager dm = new DatasetManager();
            var metadataDic = dm.GetDatasetLatestMetadataVersions();
            

            foreach (var key in metadataDic.Keys)
            {
                //the values in the dictionary are already xml documents or null. Javad
                if (metadataDic[key] != null)
                    writeBexisIndex(key, metadataDic[key]);
            }

            indexWriter.Optimize();
            autoCompleteIndexWriter.Optimize();

            if (!reIndex)
            { 
                indexWriter.Dispose();
                autoCompleteIndexWriter.Dispose();
            }
        }

       
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

        private void writeBexisIndex(long id, XmlDocument metadataDoc)
        {


            String docId = id.ToString();//metadataDoc.GetElementsByTagName("bgc:id")[0].InnerText;

            if (!docId.Equals("10280"))
            {
                var dataset = new Document();
                List<XmlNode> facetNodes = facetXmlNodeList;
                dataset.Add(new Field("doc_id", docId, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                foreach (XmlNode facet in facetNodes)
                {

                    String multivalued = facet.Attributes.GetNamedItem("multivalued").Value;
                    String metadataElementName = facet.Attributes.GetNamedItem("metadata_name").Value;
                    String lucene_name = facet.Attributes.GetNamedItem("lucene_name").Value;


                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        dataset.Add(new Field("facet_" + lucene_name, elemList[i].InnerText, Lucene.Net.Documents.Field.Store.YES, Field.Index.NOT_ANALYZED));
                        dataset.Add(new Field("ng_all", elemList[i].InnerText, Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
                        writeAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                        writeAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                    }

                }

                List<XmlNode> propertyNodes = propertyXmlNodeList;
                foreach (XmlNode property in propertyNodes)
                {
                    String multivalued = property.Attributes.GetNamedItem("multivalued").Value;
                    String lucene_name = property.Attributes.GetNamedItem("lucene_name").Value;
                    String metadataElementName = property.Attributes.GetNamedItem("metadata_name").Value;
                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
                        //.GetElementsByTagName(metadataElementName);
                    String primitiveType = property.Attributes.GetNamedItem("primitive_type").Value;
                    if (primitiveType.ToLower().Equals("string"))
                    {
                        dataset.Add(new Field("property_" + lucene_name, elemList[0].InnerText, Lucene.Net.Documents.Field.Store.YES, Field.Index.NOT_ANALYZED));
                        dataset.Add(new Field("ng_all", elemList[0].InnerText, Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
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

                            NumericField xyz = new NumericField("property_numeric_" + lucene_name).SetLongValue(MyDateTime.Ticks);
                            String dateToString = MyDateTime.Date.ToString("d", CultureInfo.CreateSpecificCulture("en-US"));
                            dataset.Add(xyz);
                            dataset.Add(new Field("property_" + lucene_name, dateToString, Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));

                            writeAutoCompleteIndex(docId, lucene_name, MyDateTime.Date.ToString());
                            writeAutoCompleteIndex(docId, "ng_all", MyDateTime.Date.ToString());
                        }
                    }
                    else if (primitiveType.ToLower().Equals("integer"))
                    {
                        dataset.Add(new NumericField("property_numeric" + lucene_name).SetIntValue(Convert.ToInt32(elemList[0].InnerText)));
                        dataset.Add(new Field("property_" + lucene_name, elemList[0].InnerText, Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
                        //  writeAutoCompleteIndex(lucene_name, elemList[0].InnerText);
                    }
                    else if (primitiveType.ToLower().Equals("double"))
                    {
                        dataset.Add(new NumericField("property_numeric" + lucene_name).SetDoubleValue(Convert.ToDouble(elemList[0].InnerText)));
                        dataset.Add(new Field("property_" + lucene_name, elemList[0].InnerText, Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
                        writeAutoCompleteIndex(docId, lucene_name, elemList[0].InnerText);
                        writeAutoCompleteIndex(docId, "ng_all", elemList[0].InnerText);
                    }
                }
                List<XmlNode> categoryNodes = categoryXmlNodeList;
                foreach (XmlNode category in categoryNodes)
                {
                    String multivalued = category.Attributes.GetNamedItem("multivalued").Value;
                    String primitiveType = category.Attributes.GetNamedItem("primitive_type").Value;
                    String lucene_name = category.Attributes.GetNamedItem("lucene_name").Value;
                    String metadataElementName = category.Attributes.GetNamedItem("metadata_name").Value;
                    String storing = category.Attributes.GetNamedItem("store").Value;
                    String analysing = category.Attributes.GetNamedItem("analysed").Value;
                    float boosting = Convert.ToSingle(category.Attributes.GetNamedItem("boost").Value);
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


                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        Console.WriteLine(metadataElementName + " " + elemList[i].InnerText);
                        Field a = new Field("category_" + lucene_name, elemList[i].InnerText, toStore, toAnalyse);
                        a.Boost = boosting;
                        dataset.Add(a);
                        dataset.Add(new Field("ng_" + lucene_name, elemList[i].InnerText, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                        dataset.Add(new Field("ng_all", elemList[i].InnerText, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                        writeAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                        writeAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                    }
                }


                List<XmlNode> generalNodes = generalXmlNodeList;

                foreach (XmlNode general in generalNodes)
                {

                    String multivalued = general.Attributes.GetNamedItem("multivalued").Value;
                    String primitiveType = general.Attributes.GetNamedItem("primitive_type").Value;
                    String lucene_name = general.Attributes.GetNamedItem("lucene_name").Value;
                    String metadataElementName = general.Attributes.GetNamedItem("metadata_name").Value;

                    String storing = general.Attributes.GetNamedItem("store").Value;
                    String analysing = general.Attributes.GetNamedItem("analysed").Value;
                    float boosting = Convert.ToSingle(general.Attributes.GetNamedItem("boost").Value);

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
                    XmlNodeList elemList = metadataDoc.SelectNodes(metadataElementName);
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        Console.WriteLine(metadataElementName + " " + elemList[i].InnerText);
                        Field a = new Field(lucene_name, elemList[i].InnerText, toStore, toAnalyse);
                        a.Boost = boosting;
                        dataset.Add(a);
                        writeAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                        writeAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                    }

                }

                indexWriter.AddDocument(dataset);
            }


        
        }

        private void writeAutoCompleteIndex(String docId, String f, String V)
        {
            var dataset = new Document();
            dataset.Add(new Field("id", docId.ToLower(), Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
            dataset.Add(new Field("field", f.ToLower(), Lucene.Net.Documents.Field.Store.NO, Field.Index.NOT_ANALYZED));
            dataset.Add(new Field("value", V.ToLower(), Lucene.Net.Documents.Field.Store.YES, Field.Index.ANALYZED));
            autoCompleteIndexWriter.AddDocument(dataset);
        }



    }
}

